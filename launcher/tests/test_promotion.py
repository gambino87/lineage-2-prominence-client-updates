"""Offline deployment rehearsals. Never connect to the VPS."""
import gzip
import io
import json
from pathlib import Path
import sys
import tarfile
import tempfile
import unittest
from unittest.mock import patch

sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
import server_promotion as worker


class PromotionTests(unittest.TestCase):
    def setUp(self):
        self.temporary=tempfile.TemporaryDirectory()
        self.root=Path(self.temporary.name)
        self.server=self.root/'server'
        self.history=self.root/'history'
        self.server.mkdir()
        self.patches=[patch.object(worker,'SERVER',self.server),patch.object(worker,'HISTORY',self.history),
                      patch.object(worker,'MAINTENANCE',self.root/'maintenance')]
        for p in self.patches:p.start()
        self.write('game/config/Database.ini',b'LIVE DB SETTINGS')
        self.write('game/config/Server.ini',b'LIVE IP')
        self.write('login/config/LoginServer.ini',b'LIVE LOGIN SETTINGS')
        self.write('game/data/old.xml',b'old')

    def tearDown(self):
        for p in reversed(self.patches):p.stop()
        self.temporary.cleanup()

    def write(self,name,data):
        path=self.server/name
        path.parent.mkdir(parents=True,exist_ok=True)
        path.write_bytes(data)

    def bundle(self,files=None):
        files=files or {'game/data/new.xml':b'new','libs/GameServer.jar':b'tested jar'}
        import hashlib
        metadata=dict(version='alpha-0.0.1',files={n:hashlib.sha256(d).hexdigest() for n,d in files.items()},
                      expected_live=worker.inventory(self.server))
        target=self.root/'server.tar.gz'
        with tarfile.open(target,'w:gz') as archive:
            for name,data in {'promotion.json':json.dumps(metadata).encode(),**files}.items():
                info=tarfile.TarInfo(name);info.size=len(data)
                archive.addfile(info,io.BytesIO(data))
        return target,worker.digest(target)

    def test_environment_and_database_excluded(self):
        for name in ['game/config/Database.ini','game/config/Server.ini','login/config/LoginServer.ini',
                     'game/config/hexid.txt','game/data/a.sql','../libs/a.jar','game/data/../../password',
                     '/game/data/a.xml','game\\data\\a.xml']:
            self.assertFalse(worker.managed(name),name)
        self.assertEqual(list(worker.inventory(self.server)),['game/data/old.xml'])

    def test_tampered_and_unsafe_packages_rejected(self):
        target,sha=self.bundle({'game/config/Database.ini':b'bench password'})
        with self.assertRaises(ValueError):worker.check_bundle(target,sha)
        target,sha=self.bundle()
        with self.assertRaises(ValueError):worker.check_bundle(target,'0'*64)

    def test_live_drift_blocks_before_outage(self):
        bundle,sha=self.bundle()
        self.write('game/data/old.xml',b'changed live')
        with patch.object(worker,'begin_maintenance') as gate:
            with self.assertRaises(ValueError):worker.install(bundle,sha)
            gate.assert_not_called()

    def test_install_preserves_environment_and_keeps_maintenance(self):
        bundle,sha=self.bundle()
        with patch.object(worker,'begin_maintenance'),patch.object(worker,'stop') as stop,\
             patch.object(worker,'backup') as backup,patch.object(worker,'command'),patch.object(worker,'gate') as gate:
            worker.install(bundle,sha)
            stop.assert_called_once_with();backup.assert_called_once();gate.assert_not_called()
        self.assertEqual((self.server/'game/config/Database.ini').read_bytes(),b'LIVE DB SETTINGS')
        self.assertEqual((self.server/'login/config/LoginServer.ini').read_bytes(),b'LIVE LOGIN SETTINGS')
        self.assertTrue((self.server/'game/data/new.xml').exists())
        self.assertFalse((self.server/'game/data/old.xml').exists())
        self.assertTrue((self.history/'alpha-0.0.1/previous-server/game/data/old.xml').exists())
        self.assertEqual(json.loads((self.history/'alpha-0.0.1/state.json').read_text())['status'],'installed-maintenance')

    def test_failed_backup_never_swaps(self):
        bundle,sha=self.bundle()
        with patch.object(worker,'begin_maintenance'),patch.object(worker,'stop'),\
             patch.object(worker,'backup',side_effect=RuntimeError('dump failed')):
            with self.assertRaises(RuntimeError):worker.install(bundle,sha)
        self.assertTrue((self.server/'game/data/old.xml').exists())
        self.assertFalse((self.history/'alpha-0.0.1/previous-server').exists())

    def state(self,status):
        folder=self.history/'alpha-0.0.1';folder.mkdir(parents=True,exist_ok=True)
        worker.save(folder,dict(version='alpha-0.0.1',status=status))
        return folder

    def test_health_failure_stays_closed(self):
        folder=self.state('installed-maintenance')
        with patch.object(worker,'gate') as gate,patch.object(worker,'command'),\
             patch.object(worker,'health',side_effect=RuntimeError('failed')):
            with self.assertRaises(RuntimeError):worker.activate('alpha-0.0.1')
            gate.assert_called_once_with(True)
        self.assertEqual(json.loads((folder/'state.json').read_text())['status'],'installed-maintenance')

    def test_no_database_restore_after_admission(self):
        self.state('live')
        with patch.object(worker,'stop') as stop:
            with self.assertRaises(ValueError):worker.rollback('alpha-0.0.1')
            stop.assert_not_called()

    def test_rollback_retry_only_reopens(self):
        self.state('rolled-back')
        with patch.object(worker,'stop') as stop,patch.object(worker,'command') as command,patch.object(worker,'gate') as gate:
            worker.rollback('alpha-0.0.1')
            stop.assert_not_called();command.assert_not_called();gate.assert_called_once_with(False)

    def test_offline_rollback_restores_previous_files_and_backup(self):
        folder=self.state('installed-maintenance')
        old=folder/'previous-server';old.mkdir()
        (old/'prior.txt').write_text('previous')
        with gzip.open(folder/'database.sql.gz','wb') as out:out.write(b'LIVE DATABASE BACKUP')
        with patch.object(worker,'stop'),patch.object(worker,'gate'),patch.object(worker,'health'),patch.object(worker,'command') as command:
            worker.rollback('alpha-0.0.1')
            command.assert_any_call('mariadb','l2jmobiusinterlude',input=b'LIVE DATABASE BACKUP')
        self.assertTrue((self.server/'prior.txt').exists())
        self.assertEqual(json.loads((folder/'state.json').read_text())['status'],'rolled-back')


if __name__=='__main__':unittest.main()
