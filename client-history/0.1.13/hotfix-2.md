# Hotfix #2

Hotfix #2 - Monster Adena payouts
- Halved actual Adena amounts dropped by all monsters, at every level, including bosses, WANTED monsters, leaders and minions.
- The 70% base drop chance and level-difference modifiers are unchanged.
- Monster Adena values used for item, spoil, T4 key and quest-token budgets are unchanged. XP and SP are unchanged.
- Server-only hotfix; no client update required.

Deployment: GameServer.jar only. Its only gameplay class change is MonsterAdenaBaseline. Existing live data, configuration and player database are preserved. No client binary or signed manifest changes.
