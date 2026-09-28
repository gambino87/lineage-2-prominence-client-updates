# Client 0.1.13

Update 0.1.13

Party play and combat display
- Parties now use random item distribution, including Sweeper rewards. Each unit in a stack is distributed independently; Adena remains evenly shared.
- Your character appears first in the party panel. Party headers show the leader crown, level, name, target-relative threat and DPS.
- Added grey SP bars to player and party panels and a 30-second rolling DPS display. Improved party spacing, backgrounds and threat-indicator stability.

Combat and stances
- Mystic weapon auto-attacks restore 5 MP on successful hits. Their attack power now uses twice the square root of weapon M.Atk.
- Lightning Bolt, Wind Bolt, Ice Bolt and Flame of Entropy have increased power. Curse Poison and Venom have a one-second base cast time.
- Curse Weakness and Wind Shackle now also deal 3 damage per second while active.
- Bow physical damage gains a distance bonus: 10% beyond 100 units, 20% beyond 300, and 25% beyond 500.
- Power Shot costs no MP and has a 20-second cooldown. Removed bow accuracy penalties.
- Mage Stance grants 50% M.Atk with 10% longer casting times. Cleric Stance grants 10% casting speed and 20% healing output.
- Fighter stances are now named Low Guard and High Guard. Low Guard grants 10% P.Def and M.Def alongside its threat bonus.
- Wide Guard allows melee auto-attacks to hit up to four additional enemies, with a 33% outgoing damage reduction.
- Warlock Stance makes Lightning, Wind and Ice Bolt hit up to four nearby additional targets at 80% damage, with matching impact effects.
- All six stances and guards switch immediately without cancelling current actions and share a three-second switching cooldown.
- Heal rank 2 is available at level 15.

Monsters and encounters
- Elven Ruins Leaders and Minions have distinct target badges. Corrected hallway monster level labels.
- Leaders summon four reinforcements at half health. Reinforcements wait three seconds before engaging.
- A surviving leader casts Haste II after its original four followers die, and Berserker Spirit II after all four reinforcements die.
- Improved leader casting and Relic Werewolf weapon appearance, Disarm behavior and animations.
- WANTED monsters now use 10x HP and 20x reward values, retaining their defense bonuses.

Crafting and equipment
- Aligned all T1-T3 single-weapon recipes, village key shops and quest exchanges with the current weapon tiers. Added missing T3 crafting coverage, including Falchion and Zweihander.
- Added nine T4 weapon recipes through Restored Relics. Their five required keys drop from Elven Ruins Leaders and Minions in a separate key reward pool.
- Jewelry names and M.Def now follow the Magicked, Knowledge, Wisdom and Blue Diamond tiers. Recipe and key names match, and T3 jewelry keys are sold in Elven Village.
- Existing learned recipes and owned jewelry remain valid. Recipe exchanges are ordered by tier and equipment category.
- Bronze Shield now has matching weathered bronze artwork.

Fixes
- Fixed an armor tooltip memory-corruption crash and added armor M.Def display.
- Fixed party-panel target handling, repeated separators and overlapping information.
- Corrected damaged punctuation in launcher patch notes.



Compared with 0.1.12.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/0.1.13) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `system/armorgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/a5d238503c0f282b7274c63005c37b1a32bbdd985c3462cd062015e03b1f5878.zip) |
| `system/engine.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/ddc31d4d74cc9bd4354569c61300b7624b6aca27bb23f041cf97cc065d4246b2.zip) |
| `system/etcitemgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/cac28c0f6b62fe3200e699fb7e4c05ff9dd3d39b58ae7009a8d31e5be18be944.zip) |
| `system/interface.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/dcc92a41a6f53c8f29e68650a0f2bffd2cb4737902bb7bb677850662c8b1a639.zip) |
| `system/interface.xdat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/c0ec372f2a4b82daae59f7f1e50c06fc9343ff6213e3b3cd8438a9fe8e6f78af.zip) |
| `system/itemname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/abd083524dd229c213e1ae5a4239ddba96e263a35dd972c654ae2bc65148b4cf.zip) |
| `system/LineageMonster.int` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/c63458c7eac210bd7277985a95851e2889a284d6fd0128e4bae81b478dcf07d0.zip) |
| `system/npcgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/eefaf300e37bad359edbd4dd9742413a3755af6cda64c990536340af9cf30f39.zip) |
| `system/recipe-c.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/f22f15f8ac7030350986a96c4b069858143b1d679b6f97f2423986e7d7f7757a.zip) |
| `system/skillgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/90e84d04a91e45e713ac19e54967887aa00694d15015c762b2ecfe2a3a8b4ef5.zip) |
| `system/skillname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/660071b7894204011894e83856efc2dc741f5932b9b50d67349e57093b14a0a8.zip) |
| `system/weapongrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/8284dd77b197b76583249b7ef66bfd785880589269a48ce40b7a9b0c9456b00e.zip) |
| `systextures/L2UI_CH3.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/5f941e295de9b2770828329736d86749ed923628f27cf72139ef9677aad8e382.zip) |
| `systextures/LineageWeaponsTex.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/278c06b63b0c1ac2d0aceb09bf0c2b2f72d1a3156ff28cadbd4badf8e18222c8.zip) |
| `systextures/ProminenceRoleIcons.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/905f4e267f9be529210f075510d106180c0d75a0d7d2f140cf49c5ad7f192a1f.zip) |
| `systextures/ProminenceSPFrame.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/7e4ed4bba8518d668b896cfd98c003295e7e071dc37eed5f723dde98d10db080.zip) |
| `systextures/ProminenceSPGauge.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/3430f052502293ab2262eac74eb36557539e459c31d512e3c1059e11d10bc9ab.zip) |
| `systextures/ProminenceThreatIcons.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.13/81874dbcbe4b8ea7c884bd71b10b015a8470ab02b726c5f67a62864c42339449.zip) |
