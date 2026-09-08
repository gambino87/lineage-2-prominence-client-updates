# Interlude Launcher

Portable Windows launcher using the installed .NET Framework 4.x. No Python, Java, GitHub account, or server project is required on testers' computers. Extract Launcher.zip to a writable folder and open Interlude Launcher.exe. Keep launcher.json beside it.

The configured public update repository is [gambino87/lineage-2-prominence-client-updates](https://github.com/gambino87/lineage-2-prominence-client-updates). Download [Launcher.zip](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/latest/download/Launcher.zip) for a portable launcher. Its signed manifest feed is `https://github.com/gambino87/lineage-2-prominence-client-updates/releases/latest/download/manifest.json`. Client updates are distributed through published release assets; committing files to the repository alone does not publish an update.

## Tester flow

1. Choose an empty client folder for installation, or an existing supported Mobius Interlude client.
2. Click Install / Update. Optionally select the original downloaded client ZIP to avoid downloading it again.
3. The launcher verifies a signed release manifest, installs the base client as necessary, and applies changed-file packages.
4. Play becomes available when all managed files pass verification. Repair rechecks and replaces missing/corrupt files. Existing INIs are preserved.

The server address must be an IPv4 address. For remote testing use the host's Tailscale IPv4 address. The game server must separately be configured to accept and advertise remote connections; this launcher does not configure Windows Firewall, Tailscale, or the server.

The connection module in this particular client overrides l2.ini. The launcher accepts only the pinned module version, sets its address, and preserves the corrected 2106/7777 ports. The runtime game remains unaware of the patcher.

## Build and release

From the project root:

The actual source lives in `publishing/interlude-client-updates/launcher/`. The workspace's `launcher/` directory is a junction to it, so existing commands still work and edits appear in the Git checkout immediately. Python and PowerShell tools locate the enclosing development workspace automatically. When using a separate clone, set `PROMINENCE_WORKSPACE` to the full project workspace containing `downloads/`, `state/`, and `outputs/`; no private build inputs belong in this source repository.

```powershell
powershell -ExecutionPolicy Bypass -File launcher/build.ps1
powershell -ExecutionPolicy Bypass -File launcher/test.ps1
python launcher/stage_release.py --from-release outputs/releases/0.2.2 --output state/launcher-staging/0.2.3
# Prepare and test changes inside that staging directory, preserving its other cumulative patches.
python launcher/release.py --version 0.2.3 --client-dir state/launcher-staging/0.2.3 --notes "Describe the tested changes."
python launcher/validate_release.py outputs/releases/0.2.3 --repo gambino87/lineage-2-prominence-client-updates
```

`channel.json` supplies the default repository, title, and server address. Override these with `--repo`, `--title`, or `--host` when necessary. The default server address remains `127.0.0.1` for local testing.

The release builder requires an isolated `--client-dir`; it refuses the live client folder. Staging is a cumulative overlay on the pinned original ZIP (SHA-256 from downloads/client-manifest.json), not just the changes since the previous release. Start with `stage_release.py` to retain all previous patches. To edit an unmodified file, copy that file from the original ZIP into staging first. Files omitted from staging use the original ZIP; omission does not delete a file.

The builder includes only client directories, excludes logs, generated IWO files, shader caches, and known backup files, and seeds INIs from the base archive rather than the developer's preferences. No server files, credentials, or signing keys are packaged. Check state/launcher/release-VERSION.json before publication. Building writes a candidate `launcher.json` inside the release folder and leaves the active launcher's configuration alone.

Outputs: outputs/releases/VERSION contains manifest.json, its RSA signature, one compressed asset per changed file, and Launcher.zip. This is a cumulative release: users can skip versions. Asset URLs are pinned to the signed release's tag, even if a new latest release appears during installation. GitHub Releases supports up to 1000 assets and less than 2 GiB per asset; builder checks those limits.

The dedicated **public** GitHub repository tracks launcher/update-tool source and `client-history/`. Each history entry contains the exact signed manifest, release notes, changed client paths, checksums, and patch links. Patch archives and Launcher.zip belong in Releases. Public releases need no tester login. Do not upload the project root or embed GitHub access tokens in launcher settings.

With the existing Git Credential Manager login (or a `GH_TOKEN` / `GITHUB_TOKEN` environment variable), upload a draft. GitHub CLI is not required:

```powershell
powershell -ExecutionPolicy Bypass -File launcher/publish.ps1 -Version 0.2.3
```

Review the draft and publish it on GitHub, or run the same command with `-Publish` when publication is authorized. The publisher validates the signature and every archive before uploading, verifies GitHub's uploaded asset hashes, and resumes only matching drafts. It refuses to modify published releases. Publishing changes what testers receive on their next launcher check; it does not update an already running client. Authentication stays in memory and is sent only to GitHub API hosts.

Signing keys are generated once in **state/launcher/signing-private.xml** and signing-public.xml. Back up the private key securely; never upload it. The public key is pinned in launcher.json. Replacing it requires distributing updated launcher configuration through a trusted channel. Per-file SHA-256 hashes and archive hashes are covered by the signed manifest.

After publication, the publisher verifies public manifests and automatically commits and pushes only the generated history files on `main`. It leaves other source edits unstaged. Source edits should be committed separately when ready. If a release is published directly in GitHub's UI or history synchronization fails, run `python launcher/sync_history.py --push`; this records existing releases without republishing them. Run without `--push` to generate local history for inspection.

## Installation safety and recovery

All downloads and extracted content are verified in .launcher before replacing live files. No files are applied while L2 is running. A journal and backups allow rollback after a write failure and recovery on the next Update / Repair after a crash. Unsupported paths, reparse points, unsigned/tampered manifests, and non-HTTPS remote downloads are rejected. Downloads interrupted mid-file restart; completed verified files are cached.

Personal INIs are preserved even during Repair; repair does not reset user preferences. Server-side job hotbars are unaffected. Unknown extra files are left alone. Deletion migrations and binary deltas are not implemented in this first version. Custom interface tooltip labels are distributed as part of managed DAT files; only personal INI settings are preserved automatically.

The launcher itself is manually replaced when a new Launcher.zip is distributed. Existing installations automatically receive **client** updates, not launcher executable updates. A server/client minimum-version handshake is not yet implemented. The Windows executable is not Authenticode-signed.

## Local development feed

Use `--local` explicitly to generate a file:// feed with AllowLocalFeed=true. Without that flag, builds use the GitHub repository in channel.json. A local build selects this workspace's client folder in its candidate configuration; it is not a portable tester distribution. Production settings require HTTPS and set AllowLocalFeed=false. Do not distribute a local feed ZIP to testers.

## Validation

test.ps1 uses isolated fixtures to test real install/update/repair, failed writes, rollback, crash recovery, signatures, path rejection, settings preservation and connection-module pinning. tests/RealRelease.cs installs and verifies the actual complete client in a separate directory without launching it. It must never point at an existing client directory.

`python -m unittest discover -s launcher/tests -p "test_*.py" -v` checks client-history comparisons, the first GitHub release against the previously validated client payload, and publication safeguards. `tests/RemoteRelease.cs` exercises real HTTPS patch downloads against the existing disposable `state/launcher-real-install` fixture, deliberately replacing its managed patch files and verifying repair plus INI preservation. It never targets the live client or launches the game.
