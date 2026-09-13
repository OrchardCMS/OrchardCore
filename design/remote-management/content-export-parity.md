# Content export parity

The Download feature gains an OAuth/Pomi/MCP JSON export command sharing the existing
admin version selection, Export + per-item EditContent checks and serialization.
The export-to-target feature gains an explicit content-ID/latest selector, consumed
by the existing deployment source for queued archives and direct remote targets.
The source keeps admin form selection for legacy plans, checks every selected item,
and omits local document IDs from recipes. Missing/denied items fail the job.

Local validation: five shared admin/source version, serialization and selection tests pass; full solution build has zero warnings/errors, and the server suite passes 3,689 tests with one skip (before the added fifth focused test). Published/latest HTTP/Pomi/MCP exports and queued selection with per-item denial pass. Final merged-base verification and CI remain.
