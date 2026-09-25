# Client 0.1.10

# Lineage 2 Prominence — 0.1.10

This update expands armor progression and crafting, revises monster balance, improves shops and broker browsing, and rebuilds floating combat feedback.

## Armor and crafting

- Added 11 complete T0–T3 armor sets, comprising 55 pieces, plus six no-grade shields. Set bonuses require all five matching armor pieces.
- Heavy sets provide HP bonuses; light sets provide DEX and critical-rating bonuses; robe sets provide M. Def bonuses. Matching shields add shield-defense bonuses to heavy sets, while the matching Devotion and Magicked shield combinations reduce incoming ranged physical damage.
- Armor and shield level requirements are now displayed in tooltips: T0 at level 1, T1 at level 3, T2 at level 10, and T3 at level 15.
- Completed T1–T3 armor recipe coverage and connected the new pieces to Humble Beginnings, Gaining Momentum, Restored Relics, key-material shops, and recycling.
- Existing learned recipe IDs are retained and now produce the corresponding new equipment. Previously crafted legacy equipment is not automatically converted.
- T0 light and robe sets are available from armor merchants. Starter shops no longer offer the superseded non-set starter armor.
- Inventory sorting keeps matching set pieces together, followed by their matching shield.
- Updated matching artwork for Wood Plated, Bone Plated, Hard Leather, Bone Scaled, Magicked, and Bronze pieces, including selective armor and shield shine.
- Recipe Information now shows the crafted item's full tooltip, including stats and set bonuses.
- Recycling lists display the equipment being consumed rather than the first returned material. Fixed crashes when opening weapon recycling; dual swords and dual daggers remain excluded.

## Blacksmiths, shops, and the broker

- Starting-village blacksmiths offer recipe and material exchanges directly from their first dialogue page. Each exchange combines that blacksmith's apprenticeship tiers and can be used without an active quest; existing currencies, prices, regional material taxes, and NPC restrictions remain.
- Exchange windows refresh held ingredients and available quantities after inventory changes without clearing the selected offer or entered quantity.
- Broker offers use the native exchange window, with a final purchase review before settlement. Searches remain restricted to the shop's allowed merchandise and capped at 100 offers.
- Reclaimed broker listings no longer clutter the account listing view.

## Monsters and rewards

- Standardized ordinary hunting monsters around level-based stat and XP baselines. Individual attacks, abilities, AI, and appearances remain distinct; special encounters are excluded from ordinary-monster standardization.
- Removed legacy permanent monster trait bonuses and penalties and their corresponding target icons. Level, monster type, and descriptive raid labels remain visible.
- Reworked WANTED monsters as tougher encounters with increased rewards and a dedicated target reticle.
- Added numbered target-window level badges for monsters level 1–80. Their tooltips explain the current level difference and its damage and reward modifiers.
- Updated level-difference rules at all levels. Higher-level monsters become substantially harder, without an extra level-offset reward bonus. Lower-level monsters progressively lose XP and Adena eligibility; monsters six or more levels below award no XP, and those ten or more below award no Adena. Item and spoil penalties begin eight levels below; ordinary quest chance has no level-offset penalty.
- Revised level-based Adena amounts and rebalanced existing item, spoil, and apprenticeship-token reward budgets accordingly. Existing item identities are preserved.
- Removed the retired Kaboo Chief quest monsters and their associated warhound spawns.

## Combat and controls

- Threatening Stance now grants 5% P. Def and M. Def alongside its existing threat-generation bonus.
- Adjusted melee parry return damage to use a consistent defense reference. Ranged parries still block without returning damage.
- Holding the left mouse button pans the camera without steering the character. Short clicks retain movement, targeting, and interaction; right-mouse steering and two-button forward movement remain available.
- Refined click tolerance and camera-heading changes when releasing and re-pressing movement keys.
- Movement input during a cast clears a previously queued auto-attack, preventing it from immediately resuming when the cast ends.
- Improved doorway traversal and regenerated affected geodata, including the Orc Village and Elven Village shop areas.

## Combat text and notifications

- Monster damage uses one shared column with up to five entries. New entries push older ones smoothly upward; entries expire on a timer, and overflow fades early.
- Block, Evade, Parry, and Resisted share the monster damage queue. Improved reporting for AoE hits, untargeted enemies, and moving or newly defeated targets.
- Entries shrink into their resting size, then hold that size while fading. Critical hits remain twice the normal resting size. All damage lettering is upright, with critical bolding retained.
- Combat text uses 75% opacity with a linear fade. Monster-column spacing remains more consistent as the camera zoom changes.
- Player notifications use three separate downward-scrolling columns: received combat below the player, mastery gains to the left, and XP/SP to the right. Their shrink-in animation is centered horizontally and vertically.
- Skill and passive rank gains show an icon, name, and rank. These cards share an orderly queue with item-pickup notifications.
- Reduced repeated combat-text processing during simultaneous hits and unnecessary updates for distant, unseen permanent ambient effects.

## Updating

Close Lineage II, use **Update launcher** if offered, then **Update** in the public launcher. Live accounts, characters, and inventories are preserved.


Compared with 0.1.9.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/0.1.10) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `system/armorgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/3823f1e3e9005f6fdc6668175c46c3729ba8fc15aa396a01e54dfe9e6f27eee3.zip) |
| `system/engine.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/6b291caeb894e46fcde7e762a4bb05855a0b8a16e48a3f1baddba71ffd17644b.zip) |
| `system/etcitemgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/f5769a88682115310e07a22a6d037ac490246ecfefa5ef1c592fd2d1a798b267.zip) |
| `system/interface.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/2532b8f2a689890b4b36471cdd2ee4e0c8a374427ce55d092d1e1d4ae7d33d6e.zip) |
| `system/itemname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/929a1a6c777ca542a12e1b33e6f7ab612832b879f12c4fa21b5d7954b2acdc31.zip) |
| `system/npcgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/7045871064ed1ec570d0cacdd952db29f41c61355ad654cb594a8a51294f533f.zip) |
| `system/nwindow.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/bea24893ad16d9ce7786b8b56b37f0969d9f28bfee8895d2b23eb86539f35cb8.zip) |
| `system/recipe-c.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/7c81de58ad4555ecc6f10ed4617c93b91b18af83fbc22583ca0d8b3a18648a6e.zip) |
| `system/skillgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/c44e1744c95d739ceaf58a36d198fba777a7b84b47c544f0452b24e9f3c4f14d.zip) |
| `system/skillname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/d48b759e6261d8d90aabe4c264d459e9dd21539340366b9a595078e49b1f7bca.zip) |
| `system/weapongrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/eaf5ca716d7bfa33eec2c6b2b1fe4ac99e29fdafab41fa590bd455e741b71206.zip) |
| `systextures/fdarkelf.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/bcc99bd7f9080d7e56eeb3778365c6cae03058eab71584d8c71a46cd809fc75e.zip) |
| `systextures/fdwarf.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/d6973369c34a1956b202476b629d9b48fc80d15c835e471139c3f2f9b31bf825.zip) |
| `systextures/felf.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/8d0c281d0df314e53fe569493b3abf05db809eba172529a303bef3001a32bc6d.zip) |
| `systextures/FFighter.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/bd6b392143054d1aaa64c86a778436f3a8282de1de06a5ec535c0a362dbe46e5.zip) |
| `systextures/FMagic.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/7639ffe5b736e800d3a91c2509b9c3fd93eee074531bc3858321e0e4c44fa3b9.zip) |
| `systextures/FOrc.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/272eff6eda74bed537c421dcc145bec9ee98dcd61133f1bb49d33c99a58581be.zip) |
| `systextures/FShaman.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/49efeb2b58f6d0170fbe9b2bc9637f7380e19284e21ea4a5c10dd3d24c1e78b6.zip) |
| `systextures/L2UI_CH3.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/b9284eb4faebc8eccb03e2a6a0b93042c76eb7297edf4ee5abf89af8689466bb.zip) |
| `systextures/LineageWeaponsTex.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/060acdaba6d8ee3476f341cca2986bb1e4f41c1cf00debc11d36c7a9d19fb7ff.zip) |
| `systextures/mdarkelf.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/b89ffb1aea258db2f193518ef4ab884cc03bf359d6a67f32f7988c5e565f849d.zip) |
| `systextures/mdwarf.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/0c2481f3e7d7a101f9339fa9809a693d5291087916f9693c9c9d1232d8b19318.zip) |
| `systextures/melf.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/37f5205988b7b89632c7eff9db23c12d4f0e05189cacada5090623d983f9ba6d.zip) |
| `systextures/MFighter.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/15ead9bab24c3801f2280125ac6aa5d44c5d4d4cda8bef437d89f52fae382d6d.zip) |
| `systextures/MMagic.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/eec4fa21655f957e1054a632fdb37bce1f76c9e5380b7a8129118350c824ec07.zip) |
| `systextures/MOrc.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/fa006958f2badca3551ab059f579f1a4fcec826e5805da25e414425052bb81b0.zip) |
| `systextures/MShaman.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/e2289f104dd43fd71d4231d64e0e436ee9c783615935af421f1dd825e074583e.zip) |
| `systextures/ProminenceLevelIcons.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/76b2ec0f0b5c8438e6a983938b61eab2963ce6c794793bb4aee13b0964f62e93.zip) |
| `systextures/ProminenceWantedIcons.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.10/2f097b9f5497859177cb89c351be1dc5175fcaca6d98fd775604c9d7f1259b63.zip) |
