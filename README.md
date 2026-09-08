# Lineage 2 Prominence Client Updates

Source and client release history for the custom Interlude launcher and signed client updates.

## Tracked files

- [launcher/](launcher/): the launcher application, release builder, publisher, and tests.
- [client-history/](client-history/README.md): signed manifests, release notes, file changes, checksums, and patch download links for every published client release.

The current workspace's `launcher/` path points to this checkout's `launcher/` directory. Editing either location changes the same source files, so edits appear in Git immediately. Build outputs, staged game files, server files, and signing keys stay outside this repository.

## Install or update

Download [Launcher.zip](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/latest/download/Launcher.zip), extract it, and open Interlude Launcher.exe. Close Lineage II, then click **Install** or **Update** when available to apply the release.

The launcher checks this repository's latest published release, verifies its signed manifest, and downloads only managed files that need updating. Existing INI settings are preserved. Future client releases use the same feed; users do not need a GitHub account.

The original supported client is downloaded separately by the launcher when needed. The default server address is `127.0.0.1` for local testing. Server connectivity is configured separately.

## Release workflow

1. Prepare and test client changes in an isolated staging directory.
2. Build and validate a cumulative signed release locally, using all previous patches plus the new staged changes.
3. Upload its manifest, signature, patch archives, and Launcher.zip to a **draft release**.
4. Review the draft, then publish it when ready for testers. The publisher records and pushes the signed client history automatically.
5. Users apply the update themselves through the launcher.

Draft releases are for maintainer review; they are not available through the public updater feed. Published releases are immutable: corrections use a new version.

Source changes appear in Git as soon as they are edited; commit and push them when ready. Published client changes appear under `client-history/` automatically. The game binaries and downloadable launcher package remain in GitHub Releases. A source commit alone does not publish a client update. The full base client, server files, account data, local configuration, and private signing keys must never be committed here.

See [client release history](client-history/README.md) for the latest recorded changes. Releases 0.2.1 and 0.2.2 added the skill-cap trial quest journal and corrected Harrys' Kat's quest location. The signed update feed is [manifest.json](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/latest/download/manifest.json).

If you have an older local-feed launcher, replace its launcher files with the latest Launcher.zip and restart it, then select your existing client folder. The workspace launcher has already been configured for this GitHub feed; restart any instance that was open during that change. Applying client updates remains a manual **Install** or **Update** when available action.

Maintainer commands and prerequisites: [launcher guide](launcher/README.md). If a release is published directly on GitHub, or a history push is interrupted, run `python launcher/sync_history.py --push` to synchronize its records.
