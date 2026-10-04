# Client 0.1.15

Update 0.1.15

Changes since live 0.1.14

Combat and progression
- Weapons in the overhaul catalog now follow the approved one-handed sword auto-attack DPS progression at each weapon level. Weapon families retain their own speed, accuracy, critical behavior and attack cycles; magic weapon P.Atk remains half its M.Atk.
- Magic auto-attacks use a linear M.Atk-versus-M.Def damage formula. Human and Orc Mystics now have B-rated Magic Weapon training, matching the other base Mystics.
- All caster classes retain Spellcraft, Magician's Movement and Mana Recovery after class transfers. Spellcraft also grants 10% more M.Atk while wearing robes.
- Physical outgoing damage against monsters now uses the equipped weapon family's training for the combat level comparison. Incoming monster damage uses the training of the armor being worn. Reward level comparisons remain tied to character level.
- Block, Dodge and Parry training use the same class-normalized chance model.
- Power Strike, Mortal Blow, Power Shot and Iron Fist use one auto-attack cycle for their action, without a cast bar. Their reuse follows attack cycles, and queued uses resume normal attacks without the extra spell recovery pause.
- Rank-one damage is 2.5 normal attacks for Power Strike and Iron Fist, 4 for Mortal Blow, and 1.5 for Power Shot. Later ranks retain 3, 6 and 2.5 respectively. Power Strike and Iron Fist share reuse.
- Power Shot's reuse includes bow firing and restring timing, allowing three normal bow attacks between uses. Power Shot itself adds no restring delay.
- Lightning Bolt, Ice Bolt and Wind Bolt are learned at level 5 and each have one rank dealing 2.25 times magic-auto damage. They have a four-second base cast and reuse designed around two intervening magic auto-attacks. Ice Bolt no longer slows.
- Curse Weakness and Wind Shackle now scale their damage over time with M.Atk and M.Def, with 300 total power over 30 seconds. Curse Poison and Venom use 400 total power over 30 seconds. Existing debuffs, landing, cleansing and diminishing returns remain.
- Wide Guard now prevents parrying and reduces Accuracy by 3.75 while active, retaining its existing cleave and damage penalty.

First-transfer abilities
- Human Knight, Elven Knight and Palus Knight gain Shield Slam: a successful shield block opens a six-second opportunity. A landed Slam deals normal auto-attack damage, interrupts casting and attempts a stun. It costs no MP and has a 30-second cooldown.
- These Knights also gain Pull at level 20: Inescapable Justice for Human Knight and Divine Lure for Elven and Palus Knight. A clear-path target within 700 units is stumbled, struck and drawn close with themed chain effects and sound. Bosses are immune; reuse is shared and lasts 30 seconds.
- Aegis replaces High Guard for these Knights, allowing normal shield blocks from every direction.
- Knight's Oath replaces Low Guard, passively granting 10% P.Def, 10% M.Def and 30% generated threat.
- Vanguard replaces Wide Guard, allowing full-damage melee auto-attacks against up to five nearby targets without the guard's Accuracy or parry penalties.
- Warrior and Raider gain Leaping Strike at level 20: a 500-unit gap closer dealing one normal attack, with no MP cost and a fixed 15-second cooldown. Added its leap motion, ground smoke and impact effects and sounds.

Monsters
- Monsters at level 5 and above gain 50 template HP before existing special HP multipliers.
- Restored the shared level-based XP baseline by removing accumulated blanket XP increases. Leader, Minion and WANTED reward multipliers remain.
- All level 1-20 monsters now have matching unbuffed effective P.Def and M.Def. Temporary buffs and debuffs still modify their respective defenses.

Interface and equipment
- Starter skill tooltips use compact name/rank, skill-type and cast/MP headers, followed by concise description, power/effect and reuse/school lines. Current speed-derived timing numbers are yellow. Fixed times retain their normal color.
- Corrected compact tooltip selection and preserved timing snapshots when tooltip windows are recreated.
- Added a collapsible Combat Skills group to the Passive tab and updated the Blunt mastery icon.
- Inventory sorting groups weapons by type and required level while keeping matching armor sets together.
- Armor merchants with separate starter-armor menus now offer one Buy Armor option containing their available armor and shields.
- Equipment hotbar shortcuts now toggle equipped items off and back on. Pending swaps retain combat timing and equipment restrictions.
- Improved final keyboard-facing synchronization, including signed heading reports and shield-facing checks across the full turn boundary.
- Updated Triple Sonic Slash motions for male character bodies and Burning Fist's charge effect to cover both hands.

Close Lineage II and use Update in the public launcher when this release becomes available.


Compared with 0.1.14.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/0.1.15) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `animations/DarkElf.ukx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/26c15da1101f85f6a6e795ca27d16974c482feba463a8aaf8a45bc224718041e.zip) |
| `animations/Dwarf.ukx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/9393f68fc41236b4c6c17d78982649f526a76ad20e843ee8f23eb41169c7b0a3.zip) |
| `animations/Elf.ukx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/96ff1af29784a6544e69adb3f055e28cdac4eef5b0f7ede60a578352408e741a.zip) |
| `animations/Fighter.ukx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/8a35c6879ef1964f04032396229c55427aaa5da1b662c4017ff8808af4efbc02.zip) |
| `animations/Magic.ukx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/419be050aff1e894f213ef2e86acdeedafbedf726adf2f8fdd2b56bf78a4ff27.zip) |
| `animations/Orc.ukx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/5cab68e632af9ca9d26b2d65fe1310911640805bafb2569910fd7967cbf1ce47.zip) |
| `animations/Shaman.ukx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/ca976c8b3880fee185704c095becdd3f132d548aa31ca83715c4b2e97712ad95.zip) |
| `animations/Skill.usk` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/81ba731edd8d9419efc4d674a9ca317428e59501f5bd59cc63df765eb6ec5e81.zip) |
| `staticmeshes/LineageEffectsStaticmeshes.usx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/4b46f47087f655497aa7f979bf3ff3a2515b15436687e1dad932b40fb6414cd0.zip) |
| `system/engine.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/4b0e0470555a1792c170e11013fc5737c56316bf814e69f516fc5d3451b96757.zip) |
| `system/interface.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/59235bf6540c23e93fee0de0d19ecc472304e1173ef2397a0027de1e799998b1.zip) |
| `system/interface.xdat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/f5911d6dc60a3867b08ae7345fc14c93ddbee292237d22d40e6e2fa968efc363.zip) |
| `system/lineageeffect.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/8c91d87d366a1fb023014d3678a469325089144f5670e399368f9c1ee2a51a51.zip) |
| `system/skillgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/2a5b05a2df1de93917605c557448ecf62fb30f9bb67f5b1e4d31a874b1cefca8.zip) |
| `system/skillname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/9ebd265b5f1e8e5ba65ced8bf8eb9c943c2deb138cebb7ddf560519f5aa0f877.zip) |
| `system/skillsoundgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/567092a2c6d4347915541ba41a79f92b5a69ce31f8bf7aefff033b65ccfc390d.zip) |
| `system/weapongrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/07647b9313d6b785a7f52d76d5bad1ef6c820500c1cc51e5b07a7e50271b16e2.zip) |
| `systextures/ProminencePullChains.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/b224e5735dcdf6f5850f16306c972c3cc2bf6f743a7334676b1e0998457b2f5b.zip) |
| `systextures/ProminenceShieldSlam.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.15/79b66717109ec5206d546eff8942fce86e45f4c218968a613727aed918e03abe.zip) |
