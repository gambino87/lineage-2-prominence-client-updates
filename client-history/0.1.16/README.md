# Client 0.1.16

Jobs and progression
- Added the Character Jobs drawer with saved levels, availability and normal town switching.
- Parent jobs retire from the selectable list after the first successful switch into a promoted child. Unlocking a child alone does not retire the parent; saved progress and hotbars are preserved.
- Eligible descendants can finish their retired parent's remaining promotion quests and fighter guild lessons.
- All nine base jobs now cap at level 19. First promotions qualify at 19 with the required quest and start at level 20. Later progression thresholds are unchanged.
- Trainer skill acquisition and enchantment are disabled. Automatic progression, job, practice, equipment and crafting unlocks remain available.

Interface and controls
- Added Combat Skills drawers showing weapon, defense and magic training, ranks, caps, training status and class ratings.
- Added the in-game Hotkeys editor with key capture, conflict swaps, Apply/Cancel and persistent bindings. Fixed empty saved rows, Ctrl+Shift handling and button layout.
- Improved fullscreen and Alt+Tab reliability with borderless desktop resolution. Fixed cursor alignment and brief Skills-window flashing.
- Target panels show passive or sight-aggression badges. Level tooltips pair damage and hit information for physical weapons, equipped magic weapons and all six magic schools, including physical front/side/rear chances. Hovered values remain stable until reopened.
- Improved inventory-opening performance, fixed missing scenery and characters when crossing regions with Ultra terrain, and made zero music volume remain silent at startup.

Combat, monsters and equipment
- Players receive a flat +20 P.Def after the normal equipment, training, buff and debuff calculation.
- Magical outgoing damage uses the relevant school training; magic weapon auto-attacks use their own training. Target tooltips reflect the matching rules.
- Increased Soul Cry fire damage for early Orc Mystic combat and moved it to the shared linear magic formula. Its 2 MP cost per auto-attack, proc rules and timing are preserved.
- Full Shirt, Leather, Bone Scaled and Hard Leather sets add 1 Accuracy alongside their existing bonuses.
- Fixed queued attacks and skills after reaching a target while preserving normal range checks and smooth approach turns.
- Aggressive monsters acquire new targets within a 120-degree forward sight cone or a 40-unit close radius, with line of sight required. Retaliation, assistance and existing combat hate remain active.
- Reduced physical attack by 1 for all 37 ordinary monster definitions at levels 1–3.
- The first Elven starting field now has four Young Red Keltirs and three Red Keltirs, replacing its two Elder Red Keltirs while preserving population and respawn timing.

Quests, crafting and terrain
- Added eleven village guild lessons for Power Strike, Mortal Blow and Power Shot. Unlocks are shared across eligible jobs; additional village variants grant their defined XP reward.
- Guild lessons use separate village contacts and local level 4–5 targets, with updated dialogue and journal routes.
- All 120 quest recipe scrolls sell for 5 Adena per token spent. A two-token recipe sells for 10 Adena; exchange costs and crafting requirements are unchanged.
- Corrected Talking Island fighter-guild doorway collision, Orc-area floor clearance, the Dark Elf starting route and unsupported lower floors in the Elven region.

Effects and staff tools
- Improved Iron Fist heat distortion and magic-resist shield orientation, haze and target-size scaling.
- Added a smaller outward-growing shockwave to critical impact feedback and fixed its loading crash.
- Improved the existing staff-monitoring bridge's transaction capture and bounded nearby-actor information. Website deployment and live monitoring settings are unchanged by this server package.


Compared with 0.1.15.

[Published release](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/tag/0.1.16) · [Signed manifest](manifest.json) · [Checksums and changes](changes.json)

| Client file | Change | Download |
| --- | --- | --- |
| `staticmeshes/LineageEffectsStaticmeshes.usx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/d711b1a7f6a6666d1f7b86602054df6776c5e233db1f3f30ad23c6f7668f1ba2.zip) |
| `system/D3DDrv.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/2892877832733e1e69a99c1be42f824720d9d8e39ae9154d15bb5a0f3f44b359.zip) |
| `system/engine.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/3d308da48eaf00fc9ecdc6e6926e841257db87609944a49e93237ad9be2158ff.zip) |
| `system/interface.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/1be216800a0a59061b6995fb4aad6fbbca3aa7ba81e86daa5f0f513aefa66fd7.zip) |
| `system/interface.xdat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/43786a8412f4ecbc283de6d1925b0108ff2c4ed996a9bff8ab4e790a075714a6.zip) |
| `system/itemname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/48e8359a37bbda16bea51cbdaecf41d863f07c22b295af12bfc9475130a83b76.zip) |
| `system/lineageeffect.u` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/9a841caaae7ecf01aa479c9576c02f434cb16fd5c6b30cc54e0c0c9646db5fed.zip) |
| `system/npcgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/6e3f6e850867bab592b1e26708cb43f7f734db76f70cb7e9ec040c8c6abc8989.zip) |
| `system/nwindow.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/c7930d451d8e094619432033a6095d3a007e08cea4fc21b9ba6170d5e548edf4.zip) |
| `system/PHeat.dll` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/5c5d26e67b86c236af1af46533ea25b09a735d3765c80c97bb326f16550ee6a6.zip) |
| `system/PKeys.dll` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/e03e7e7112d1fe66b01c50a9baea032b24f0d02fccffee916dcb12806a393e3f.zip) |
| `system/PKeysPersist.exe` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/875457523b1dc5c2b6e2aea0bf8ee4fc8563dc5fba694af4ab8414f689d1dfa3.zip) |
| `system/questname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/f3368574eea253985de9a06815000d5082b9ac6774c5085b7ced99934cf689cb.zip) |
| `system/skillgrp.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/62857f73d66e6f0cb38943eacf28ebcf2e9292c2242995891bdfe5b96c91b087.zip) |
| `system/skillname-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/b929b64c1e678fb36b022b900bc5e907092c2787d0d05d3d7f6b2f794c9faec8.zip) |
| `system/sysstring-e.dat` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/d2318665f90d0ad4b8121c161b748284645e351e31112e8a7206cce5a4c3de1f.zip) |
| `system/WinDrv.dll` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/dcb59caf49472fb330e88b213875a742a49271dbf419f7f18c8fd2e2ccac4c5e.zip) |
| `systextures/LineageEffectsTextures.utx` | Updated | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/9d008898f608b5a4c6179b45e371ff6d7636c1f0d7725599d5a121fbca127d41.zip) |
| `systextures/ProminenceAggroIcons.utx` | Added | [Patch](https://github.com/gambino87/lineage-2-prominence-client-updates/releases/download/0.1.16/506ff9b7b1237aa9d6e22e5a40688da03cd0147ab0a252052a57606cf340d8eb.zip) |
