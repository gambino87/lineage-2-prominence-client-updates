# Client alpha-0.0.13

Lineage 2 Prominence - alpha-0.0.13

Combat and threat
- Monsters keep their current valid target until another attacker reaches at least 130% of that target's threat. Explicit taunts and scripted targeting still apply.
- Your selected monster shows a threat circle beside its overhead name: green below 70%, yellow from 70-100%, orange above 100% but below 130%, and red at 130% or while attacking you.
- Damaging an immobilized monster from within 100 range raises your threat to at least 131% of its current target's threat. Applies to physical attacks, magical weapon attacks, damaging spells and damage over time. Misses, resisted hits and non-damaging spells do not trigger this rule.
- Melee parries now reduce incoming damage by 30%, down from 50%.
- Dual swords and dual daggers grant +4.75 parry rating while equipped. Dual daggers use a x1.5 critical-rate multiplier.
- Reflected parry damage now reads: Your parry reflected n damage.
- Other players' parry sounds now originate from their character with distance-based volume.

Disarm
- All Fighter jobs, including class transfers, automatically learn Disarm at level 10.
- Power 20, cost 20 MP, range 50, base cast 1.8 seconds and base reuse 30 seconds. Cast and reuse scale with attack speed.
- Supports Hand-to-Hand, Sword, Dagger, Blunt, Polearm, Dual Swords and Dual Daggers. Uses normal physical accuracy and damage rules, with weapon practice on successful damaging monster hits.
- Disarm Effect 1 lasts 10 seconds before diminishing returns. Repeated applications shorten to roughly 6.7, 5 and 3.3 seconds, then immunity; the chain resets 30 seconds after removal or expiration.
- Armed monsters have their base P.Atk. reduced to its square root. Unarmed monsters cannot be disarmed.
- Players temporarily unequip their weapon and cannot change weapon-hand equipment until the effect ends, when the same weapon is restored. Successful weapon removal interrupts casting.
- Blocking or parrying always prevents Disarm Effect 1; the damaging hit still follows normal defense rules.
- Uses Power Strike's animations and sounds, with the Seal of Binding icon.

Spells
- Curse Poison now belongs to Enfeeblement and uses Enfeeblement for accuracy and school practice.
- Orc Mystics automatically learn Venom at level 8 in place of Curse Poison. It retains Poison Effect 1: 5 damage each second for 30 seconds, 15 MP, 600 range and 10-second reuse. Venom has a 2-second base cast, its original animations and effects, Enfeeblement school and Dark aspect.
- Orc Mystics automatically learn Soul Cry rank 2 at level 13. Fire proc power increases to 2, with 2 MP consumed per auto-attack. Rank 1 remains power 1 and 2 MP per attack.
- Orc Mystics automatically learn the new Dreaming Spirit at level 14: 5-second base cast, 30-second reuse, 30 MP, Enfeeblement school and Fire aspect. Sleep Effect 1 lasts 15 seconds with diminishing returns; damage can wake the target. Preserves the original Dreaming Spirit animations and effects.
- Ice Bolt ranks 1-3 now all apply Slow Effect 1, reducing movement speed by 10%. Ranks 4-5 retain Effect 2.
- Reduced Earth magical weapons' Entangle casting sound volume by 50%.

Interface
- Floating damage numbers are opaque with dark edging. Parry text also has dark edging like Block and Evade.
- Incoming damage and healing/resource notifications are 50% larger. XP and SP remain at their original sizes. Damage colors, italic proc text and bold critical emphasis are preserved.
- Weapon tooltips replace Soulshot and Spiritshot lines with Crit., Acc. and Parry below Atk. Spd. Critical-rate multipliers use x notation; zero rating adjustments show +0. Magical weapons omit Acc.
- Character-window HP and MP now show 100% regeneration per 3-second tick: current / maximum (+n/tic). Values include regeneration stat bonuses before posture multipliers and Mend/Meditate conversion, rounded to one decimal place.

Quests and NPCs
- Renamed Decrepit Giant's Servitor to Giant's Servitor and changed its appearance to the Nonexistent Man model. Updated the related dialogue and journal references.
- Corrected The Wolf Needs No Blade hunting marker to an Evil Eye area near Orc Village. Completion still directs you back to Karukia.

To apply: close Lineage II and click Update in the launcher before reconnecting.


Compared with alpha-0.0.12.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/alpha-0.0.13) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `system/engine.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/997cef0c14780ce9c4aeee7811c6488f7eaa778aa2da4f269309747da669a796.zip) |
| `system/interface.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/4ebfbdf5bd71803229a34b7bd0ed528bd6909d101947e24e96f946cdc6737524.zip) |
| `system/interface.xdat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/5c378ee414871a62c8385d5d2c6c0867c52fe17ae7a32dd73766a89083350ff0.zip) |
| `system/npcgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/1a20a2ae8deadae17da4d9236d7b11faba9163ffa2d9d3feea65278ae161508b.zip) |
| `system/npcname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/b6b981d3e42414fce19461e116168758a9d201b8743da8b963fb529ddb098c58.zip) |
| `system/nwindow.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/1ecdadedcdf6b5345e32378935bece5e21d2239a8535f13188a03535e7069e64.zip) |
| `system/questname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/47801bf4edca8cdc8de2609d0cbdcdb1a11b542e65efd5aec63751a0e7b1c204.zip) |
| `system/skillgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/4d7b759c6e83397b0cab5a09b87be830ecbe1dd81fc23afd08facb3744a65b28.zip) |
| `system/skillname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/7020316144e134f51ad096c4456bb93b9ce7d687aa01556c018c6bc12ef25077.zip) |
| `system/skillsoundgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/f16dc27da5e0188247d02e62e42a91d73cc2a1fc0bfdbd44fff72132b3052d21.zip) |
| `system/systemmsg-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/31a3f56598a94b938677d0cedb5778fa55820d1113ab7f20e75f27973dcdde65.zip) |
| `systextures/L2UI_CH3.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/alpha-0.0.13/88ee8f1068d851925417c6597d5bd76e863a4c42c171d4e8dbc4be5a9b165811.zip) |
