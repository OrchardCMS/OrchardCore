# Content export parity

The Download feature gains an OAuth/Pomi/MCP JSON export command sharing the existing
admin version selection, Export + per-item EditContent checks and serialization.
The export-to-target feature gains an explicit content-ID/latest selector, consumed
by the existing deployment source for queued archives and direct remote targets.
The source keeps admin form selection for legacy plans, checks every selected item,
and omits local document IDs from recipes. Missing/denied items fail the job.

Validation pending: shared admin/source version and serialization tests, invalid
selection/no-mutation tests, real published/latest HTTP/Pomi/MCP export, queued
selection with authorization failure, final integrated build and CI.
