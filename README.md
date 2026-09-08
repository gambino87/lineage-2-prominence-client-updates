# Lineage 2 Prominence Client Updates

Distribution and staging repository for the custom Interlude launcher and signed client updates.

## Install or update

Download Launcher.zip from the latest published release, extract it, and open Interlude Launcher.exe. Close Lineage II, then click **Install / Update** to apply the release.

The original supported client is downloaded separately by the launcher when needed. Server connectivity is configured separately.

## Release workflow

1. Prepare and test client changes in an isolated staging directory.
2. Build a cumulative signed release locally.
3. Upload its manifest, signature, patch archives, and Launcher.zip to a **draft release**.
4. Review the draft, then publish it when ready for testers.
5. Users apply the update themselves through the launcher.

Draft releases are for maintainer review; they are not available through the public updater feed. Published releases are immutable: corrections use a new version.

This repository stores distribution documentation. Release assets belong in GitHub Releases, not Git history. The full base client, server files, account data, local configuration, and private signing keys must never be committed here.

The GitHub repository is ready; the remote update feed awaits its first published release. Until the first remote release is published, the existing local feed remains in use.
