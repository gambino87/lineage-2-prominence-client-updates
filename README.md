# Lineage 2 Prominence Client Updates

Distribution and staging repository for the custom Interlude launcher and signed client updates.

## Install or update

Download [Launcher.zip](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/latest/download/Launcher.zip), extract it, and open Interlude Launcher.exe. Close Lineage II, then click **Install / Update** to apply the release.

The launcher checks this repository's latest published release, verifies its signed manifest, and downloads only managed files that need updating. Existing INI settings are preserved. Future client releases use the same feed; users do not need a GitHub account.

The original supported client is downloaded separately by the launcher when needed. The default server address is `127.0.0.1` for local testing. Server connectivity is configured separately.

## Release workflow

1. Prepare and test client changes in an isolated staging directory.
2. Build and validate a cumulative signed release locally, using all previous patches plus the new staged changes.
3. Upload its manifest, signature, patch archives, and Launcher.zip to a **draft release**.
4. Review the draft, then publish it when ready for testers.
5. Users apply the update themselves through the launcher.

Draft releases are for maintainer review; they are not available through the public updater feed. Published releases are immutable: corrections use a new version.

This repository stores distribution documentation. Release assets belong in GitHub Releases, not Git history. A source commit alone does not publish a client update. The full base client, server files, account data, local configuration, and private signing keys must never be committed here.

The first public updater release is [0.2.0](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/0.2.0): 1,504 managed files and 18 cumulative patches, matching the previously validated local client package. The signed update feed is [manifest.json](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/latest/download/manifest.json).

If you have an older local-feed launcher, replace its launcher files with the latest Launcher.zip and restart it, then select your existing client folder. The workspace launcher has already been configured for this GitHub feed; restart any instance that was open during that change. Applying client updates remains a manual **Install / Update** action.
