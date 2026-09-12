# Client alpha-0.0.10

Lineage 2 Prominence - alpha-0.0.10

Soul Cry
- Soul Cry now adds Fire magic damage to successful auto-attacks instead of increasing P. Atk.
- Each successful hit attempts a power-1 Fire proc based on your M. Atk. The target can resist; accuracy uses Enhancement.
- Works with physical and magical weapons. Costs 2 MP per auto-attack cycle, including misses and resists, with no MP drain while idle. Dual strikes and additional polearm targets do not multiply the MP cost.
- When you cannot pay the cost, Soul Cry turns off and your normal attack continues. An attack that spends your last 2 MP can still trigger its paid proc.
- Successful procs play Fire Bolt's target impact and hit sound.

Poison and spell descriptions
- Poison Effect 1 now deals 5 damage every second for 30 seconds, replacing 15 damage every 3 seconds. Total damage remains 150 before resistance.
- Poison's target visual now repeats every second.
- Updated Flame of Entropy and Curse Poison descriptions to explain their effects, timing, schools and elements.

Floating combat text
- Soul Cry procs, parry retaliation and damage-over-time ticks now display damage numbers on the correct target.
- Damage colors: Physical - warm white; Magical - light purple; Holy - white; Dark - magenta; Fire - orange-red; Wind - blue-green; Water - light blue; Earth - light green; Direct/unaspected - grey.
- Procs, damage over time and parry retaliation use italics while retaining their damage color. Parry retaliation is Direct damage.
- Critical hits use bold emphasis instead of a separate critical color. Non-critical numbers are now half-size, with tighter digit spacing.
- Large critical digits have been redrawn at higher resolution for sharper edges.
- Numbers use six positions and do not repeat the previous position for the same target. Each number retains its selected offset throughout its animation.
- Incoming damage follows the same color and emphasis rules while keeping its usual location.
- Fixed a combat-text client crash affecting attacks, including magical-weapon attacks on town NPCs.

Casting UI
- The skill cast bar now uses the server's cast duration instead of a separate client estimate, aligning its timing with the overhead bar.
- Removed the UI bar's independent minimum duration and end delay.
- Instant hit effects and other characters' casts no longer reset your active cast bar.

Launcher 1.1.14
- Play is disabled while a launcher or client update is required. The launcher checks again before starting the game.
- Required update buttons gently pulse. All disabled buttons use matching grey styling.
- Update launcher stays disabled when the launcher is current. Client-only releases no longer trigger a false launcher update.
- Replaced the base-client download link with Share with a friend! The button copies the public launcher download link and displays Copied Link for 3 seconds.

Known issue
- Auto-attacks do not consistently resume after physical skills such as Iron Punch and Power Strike. This remains under investigation.

To apply: close Lineage II, click Update launcher, then click Update before reconnecting.


Compared with alpha-0.0.9.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/alpha-0.0.10) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `animations/Skill.usk` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.10/2e9021cacc93fe0f055fc339f3011020c444c7cf466a78a0045d509cbcabd18c.zip) |
| `system/engine.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.10/579cd9822f57cf1ec760720ee77fa5e61531bc8a6baa97d7995cba627660faea.zip) |
| `system/interface.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.10/185e7d289bde1fd3312867ab26ab86d3c885d753253073905b6728d890110568.zip) |
| `system/nwindow.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.10/dec1630c7fb4d9bdb35fe7d5c8326ee7dde65587f73645daa919252c9d398eca.zip) |
| `system/skillgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.10/f00f6c894b5229bea168b408c33824eecc5d3680b0828262677c695a1d198177.zip) |
| `system/skillname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.10/d60d657b3d32ec63f20d2c4debafb61642974c27b5251e2d9a12f4c93538413a.zip) |
| `system/skillsoundgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.10/7cbc4e09686aed8760309cee9819f86558c92797affe8ad1008c2111375f630d.zip) |
| `systextures/L2UI_CH3.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.10/a3848cb71b6fcb0ba09da3c947822914b35fc3a0c48e4bc1983a19b7c00bb9d8.zip) |
