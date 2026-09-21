# Client 0.1.5

Movement and controls
- WASD taps turn in place; holding longer than 0.125 seconds moves normally.
- Tapping a direction during click-to-move or autorun brakes before turning. Autorun is cancelled.
- Improved the transition from braking to turning.
- Attack and skill commands take priority over held movement, with still-held directions resuming after the action.
- Autorun respects casting and attack locks, and the Attack command cancels autorun.
- In-range auto-attacks turn toward the target without stepping forward; turning time scales with angle, up to 0.5 seconds for a full reversal.
- Improved stopping when beginning a spell.

Close Lineage II and click Update in the launcher.

Compared with 0.1.4.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/0.1.5) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `system/engine.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.5/e84d7d7226c238c780c74b0965165850d9ceb3332fddb39fe8a1dd979b5ca8fc.zip) |
| `system/nwindow.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.5/ef736fbff40e64f62e8ebcc692d0feda22c1bfadb1f5e91e08f499645007b940.zip) |
