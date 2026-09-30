# Client 0.1.14

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
- Mage Stance grants 50% M.Atk; Hotfix #3 changes its penalty to 10% reduced casting speed. Cleric Stance grants 10% casting speed and 20% healing output.
- Fighter stances are now named Low Guard and High Guard. Low Guard grants 10% P.Def and M.Def alongside its threat bonus.
- Wide Guard allows melee auto-attacks to hit up to four additional enemies, with a 33% outgoing damage reduction.
- Warlock Stance makes Lightning, Wind and Ice Bolt hit up to four nearby additional targets at 80% damage, with matching impact effects.
- All six stances and guards share a three-second switching cooldown. Fighter guards switch immediately; caster stances queue behind the current action (updated in Hotfix #3).
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


Hotfix #1 — Armor P.Def baseline
- Fixed low-level armor lowering P.Def when equipped into empty slots.
- Empty head, glove and boot slots now provide zero P.Def.
- Empty chest and leg slots match starting gear: 14/9 P.Def for fighters and 10/7 for mystics.
- Applied consistently across class progressions. Armor item stats are unchanged.
- Server-only hotfix; no client update required.

Hotfix #2 - Monster Adena payouts
- Halved actual Adena amounts dropped by all monsters, at every level, including bosses, WANTED monsters, leaders and minions.
- The 70% base drop chance and level-difference modifiers are unchanged.
- Monster Adena values used for item, spoil, T4 key and quest-token budgets are unchanged. XP and SP are unchanged.
- Server-only hotfix; no client update required.

Hotfix #3 — release 0.1.14
- Skill reuse no longer scales with casting speed or attack speed. Explicit reuse modifiers still apply.
- Mage, Cleric and Warlock Stance now queue behind the current spell or attack, including switching off, while retaining their shared three-second cooldown.
- Manually queued spells take priority over automatic attacks. If a queued spell cannot be cast, normal attacks can resume.
- Restored Ice Bolt’s original casting animation with Ice Dagger’s projectile and impact at every rank. An ice dagger now forms during casting before launch.
- Cleric Stance now reduces M.Atk by 50%, retaining its casting-speed and healing bonuses.
- Mage Stance now reduces casting speed by 10% instead of adding a separate cast-time penalty. Its M.Atk bonus remains 50%.
- Fully resisted hostile spells now show a blue hexagonal shield, sized and centered to fit the target.


Compared with 0.1.13.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/0.1.14) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `animations/Skill.usk` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.14/d9c5ebc5962a4acda9c0b8cb0a8037185953e84a0df3fd06397e6ee8a031fca8.zip) |
| `staticmeshes/LineageEffectsStaticmeshes.usx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.14/a58e134fd69a196537e892b14739db126f94056096fed1bc9199b41bb47a4871.zip) |
| `system/engine.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.14/31a9e322ca9b78c6c7958f73a510057881326305d13fa3a6ef5ef15091797eeb.zip) |
| `system/lineageeffect.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.14/a99ee13291ab63c8c8012bb649c3b6c6e271fb94c593eb62599019cc7aeecbdf.zip) |
| `system/skillgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.14/0f10d673d7ac8af68f0ca4ff69ba6ff4db73d1437e9e3deacb1966efdf2c6340.zip) |
| `system/skillname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.14/b255d5d7fc3419c50403d83be4fe2e6af67d85c3e6f88c28e42b23304fb58057.zip) |
| `systextures/LineageEffectsTextures.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.14/420015f7c65d9b3b8cc3f697840089325fc4e51b32ca788833af96272f9bc325.zip) |
