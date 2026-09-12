# Test bench and live promotion

New work stays on the test bench until the owner says it is ready for live.
The owner can ask: **“Prepare this bench for live”**, review the candidate, then
**“Promote version X during maintenance.”** Preparing a candidate does not stop
the live server. Only the explicitly approved installation starts downtime.

## Your private launcher

Launcher 1.1.14 determines launcher-update availability from the signed launcher
version, independently of the client version. Rebuilding the same launcher version
can change its binary hash and does not require an update. Downloaded launcher
packages and executables still require their signed checksums to match. Increment
the launcher version in LauncherUpdate.cs and release.py for actual launcher changes.
Same-version or older feeds do not light the launcher-update button or block Play;
pending client updates still block Play and pulse the client Update button.

Launcher 1.1.9 (bench 0.2.57) keeps Play disabled until both the signed feed's
launcher executable and client files are current. Play checks the feed again
before launching. Pending Install/Update and Update launcher buttons gently
pulse on a 2.4-second cycle; busy and current buttons do not pulse. Apply the
launcher update first, then the client update if offered. This change is prepared
on the private feed and awaits owner testing before live promotion.

Launcher 1.1.8 adds a **Patch notes** dropdown. The current feed release is
selected on startup; browsing previous notes never changes the install target.
Refreshing the same release preserves the selected notes. A new feed release
selects its newest notes automatically.

Each signed manifest embeds `NotesHistory`. Private history comes from signed
local numeric releases with file feeds; live history comes only from the public
`client-history` records. Public builds never include private notes. Older
manifests without a history still display their current notes.

Promotions record `SourceBenchVersion` in the signed public manifest. This links
independent version sequences (for example, live alpha-0.0.9 from test bench
0.2.52) without treating their version numbers as interchangeable. Older releases
without this field remain unlinked; no relationship is guessed.

From the development workspace, double-click `Test Bench.cmd`. It starts the
local database/login/game servers and opens **Prominence — TEST BENCH**.
The launcher is compiled to use `127.0.0.1` and its own `client` subfolder;
the folder/address controls are locked and its feed must be a local file.
It does not connect to OVH or use the public client folder.

On first use, click **Install**, then **Play**. For later candidates, close Lineage II and
click **Update** when available. The owner performs these installation steps.
Use **Update launcher** for the launcher executable itself. Launcher version
1.1.0 is independent of the numeric bench or alpha live client release. The
GitHub download is the public launcher; use `Test Bench.cmd` for this private
environment. Old public launchers need one replacement executable download to
gain the new button; preserve their existing launcher.json when replacing it.
Use `Stop Test Bench.cmd` when finished; it gracefully saves and stops the local
servers. The older `Play Local.cmd` now forwards to this private launcher.

| Environment | Launcher/client | Server and player data |
| --- | --- | --- |
| Owner test bench | `outputs/test-bench/Interlude Launcher.exe`, `outputs/test-bench/client` | Local `server/`, local `state/database`, localhost |
| Public live | Distributed public Launcher.zip and each player's chosen folder | OVH, independent live database |

The two databases are independent. Local test accounts/progress do not replace
live accounts/progress. Do not distribute the private launcher, its client,
private feed, signing keys, or deployment credentials.

## 1. Make and test changes locally

1. Record what is being changed and what behavior should be tested.
2. Edit source/datapack files. Deliberately synchronize affected datapack files
   into `server/`; do not blindly copy environment configuration from source.
   For Java changes, stop the local server and run
   `python scripts/local_server.py build`. Then start it again.
3. For client changes, start cumulative staging from the last validated bench
   release. Edit staged files only; never edit either installed client:

   ```powershell
   python launcher/stage_release.py --from-release outputs/releases/0.2.45 --output state/launcher-staging/0.2.46
   # Edit files in that new staging directory.
   python launcher/testbench.py --version 0.2.46 --client-dir state/launcher-staging/0.2.46 --notes "Describe the changes to test."
   ```

   The command builds in staging, validates and signs the candidate, then
   switches only the private feed. Use **Update launcher** to apply executable
   changes; preparing a release does not replace the installed private launcher.
   New numeric test-bench versions remain local. Use a new version for every candidate.
4. The owner closes the game, installs/updates through the private launcher,
   and tests login, affected features, rewards/items if relevant, and relog
   persistence. Check local logs for new errors. Record results and known issues.
5. Stop the local server after testing and leave its tested runtime unchanged
   until the promotion candidate has been prepared. Server-only work does not
   need a new bench client release if the existing client is still compatible.

## 2. Freeze and review a candidate without downtime

Live versions start at `alpha-0.0.1`, then `alpha-0.0.2`, independently of the
bench sequence (`0.2.45`, `0.2.46`, etc.). Existing published numeric releases
are historical and remain unchanged. These commands are examples:

```powershell
python launcher/promotion.py prepare --version alpha-0.0.1 --bench-release outputs/releases/0.2.46 --test-notes "Owner passed login, affected quest, reward, and relog checks." --notes "Describe this public update."
```

This reads the live file inventory over SSH, freezes the local runtime into
`state/promotions/alpha-0.0.1/server.tar.gz`, builds a signed public client candidate
with the exact bench client payload, and writes `REVIEW.txt` and `candidate.json`.
The public launcher is freshly compiled from the current launcher source without
the private TestBench flag. Validate its public channel and version alongside
the client payload before installation; launcher changes accompany the promotion.

Review every changed/deleted server file, completed test notes, and client
changes before approving. Only JARs, game/login data, and shared game config
are managed. Live database/network identity settings, login config, service
definitions, credentials and SQL files are excluded. Shared game config
changes still deserve review: they can change gameplay or access rules.
Client INI settings retain the updater's existing preservation behavior.

The frozen client/package checksums and live baseline are rechecked at install.
If files changed, prepare a new version and repeat affected tests. Preparation
does not install or publish anything. It uploads only the inspection tool.

## 3. Approved maintenance and promotion

Tell players the maintenance time before starting. The operator performs these
commands only after the owner approves this specific reviewed candidate:

```powershell
python launcher/promotion.py install --version alpha-0.0.1 --approved
python launcher/promotion.py publish --version alpha-0.0.1 --approved
python launcher/promotion.py activate --version alpha-0.0.1 --approved
```

**Install** closes public login/game ports for IPv4 and IPv6, gracefully stops
game then login, verifies shutdown, and backs up the LIVE database. It clones
the live server, applies only managed files, checks hashes, and swaps directories.
The database is not replaced with bench data. Services remain stopped and
maintenance remains active when installation finishes. A systemd gate reapplies
maintenance before the servers can start after a VPS reboot.

**Publish** validates and publishes the matching signed public client release
through GitHub Releases and synchronizes/pushes release history. Resolve any
publication/history push failure before proceeding. Commit/push related tested
launcher source separately, preserving unrelated work.

**Activate** verifies the matching public manifest, starts services while public
ports remain closed, then checks login/game sockets and game registration logs.
Only successful startup reopens the ports. Players close Lineage II and click
**Update** if offered. Identical client content needs no download/install even
when a server-only promotion advances the public manifest version.

After reopening, check a real login and the changed feature, plus live logs.
Automated startup checks do not prove gameplay correctness. Keep the promotion's
backup and previous server directory until the update is known stable; they live
under `/opt/prominence/promotions/<version>/`. Review disk usage before future
promotions because these snapshots are retained, not automatically pruned.

## If an update fails

Stop the sequence; do not manually open ports. Before players have been
readmitted, run:

```powershell
python launcher/promotion.py rollback --version alpha-0.0.1 --approved
```

Rollback restores the previous public feed if it was switched, stops the
candidate, restores the previous server directory and the completed pre-update
LIVE database backup, and checks startup before reopening. An incomplete backup
is never used. If clients already applied the failed release, they must check
the restored feed and click Update to restore its managed files.

Once activation has admitted players, this rollback is deliberately blocked:
restoring that database could erase new progress. Prepare a forward fix or a
separately reviewed rollback that preserves the current live database. A retry
of a completed activation/rollback only retries opening the maintenance gate;
it never repeats a completed database restore.

SQL/schema migrations are outside this tool. Do not use this promotion path
for releases requiring them until a separate migration/rollback plan is tested.
The one-time OVH provisioning scripts are not update scripts; rerunning them
can overwrite configuration/data.

## Validation and limits

Offline rehearsal tests:

```powershell
python -m unittest discover -s launcher/tests -p test_promotion.py -v
```

These test environment exclusions, package validation, live drift detection,
installation, backup failure, startup failure, and rollback without touching
OVH. The first real maintenance remains the first end-to-end Linux deployment
of this tooling; no production downtime is performed just to test the process.
