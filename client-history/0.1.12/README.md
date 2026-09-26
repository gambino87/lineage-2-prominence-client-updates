# Client 0.1.12

# Lineage 2 Prominence â€” 0.1.10

This update expands armor progression and crafting, revises monster balance, improves shops and broker browsing, and rebuilds floating combat feedback.

## Armor and crafting

- Added 11 complete T0â€“T3 armor sets, comprising 55 pieces, plus six no-grade shields. Set bonuses require all five matching armor pieces.
- Heavy sets provide HP bonuses; light sets provide DEX and critical-rating bonuses; robe sets provide M. Def bonuses. Matching shields add shield-defense bonuses to heavy sets, while the matching Devotion and Magicked shield combinations reduce incoming ranged physical damage.
- Armor and shield level requirements are now displayed in tooltips: T0 at level 1, T1 at level 3, T2 at level 10, and T3 at level 15.
- Completed T1â€“T3 armor recipe coverage and connected the new pieces to Humble Beginnings, Gaining Momentum, Restored Relics, key-material shops, and recycling.
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
- Added numbered target-window level badges for monsters level 1â€“80. Their tooltips explain the current level difference and its damage and reward modifiers.
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

## Hotfix â€” 0.1.11

- Restored MP bonuses on robe chest and leg pieces: starter robes total 29 MP, T0 39 MP, T1 60 MP, T2 92 MP, and T3 140 MP.
- Corrected starter armor defense so Apprentice and Squire armor sit below T0 equipment. T0â€“T3 defense is unchanged.
- Spoil Success now appears in the shared monster damage column at the same size as Block, Evade, Parry, and Resisted, with matching shrink-in, stacking, and fading. Spoil chance is unchanged.

## Hotfix #2 â€” 0.1.12

- Increased P. Atk for the 25 bows in the weapon progression catalog to account for their existing restring time. Weapon power ratings and attack/restring timing are unchanged.
- Overhit now joins the monster damage column at the standard combat-word size, with matching stacking, entrance animation, and fading.
- Simplified the launcher layout: removed the server address field, moved server reachability beside the ready-to-play status, and made more room for patch notes.

- Restored missing spoil materials on eight Keltir variants, using their intended spoil-value budgets.


Compared with 0.1.11.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/0.1.12) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `system/interface.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.12/1c34fd05df5421ece8869c20823ebe5a0a57e7530fb4937071a424a959adc707.zip) |
| `system/weapongrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.12/ce4a60406d7e3491b7cb0c857ae6d51ce65f82a594a04d1215d0799f0f1bac3c.zip) |
