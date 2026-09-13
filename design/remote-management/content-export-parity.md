# Content export parity

The Download feature gains an OAuth/Pomi/MCP JSON export command sharing the existing
admin version selection, Export + per-item EditContent checks and serialization.
The export-to-target feature gains an explicit content-ID/latest selector, consumed
by the existing deployment source for queued archives and direct remote targets.
The source keeps admin form selection for legacy plans, checks every selected item,
and omits local document IDs from recipes. Missing/denied items fail the job.

Local validation:

- Five tests cover admin/queued version and serialization parity, legacy admin form
  selection, denied items, and validation without mutation.
- Full combined solution build: zero warnings/errors. Server suite: 3,701 passed,
  one skipped. CLI: 298 passed. MCP: 78 passed.
- HTTPS HTTP/Pomi/MCP published/latest exports and queued archives pass, including
  denied identities and content/artifact isolation in a separate tenant.
- A selected draft is sent through the shared remote destination service and imported
  into another tenant as an unpublished latest version. An operator with Export and
  ExportRemoteInstances but without item-edit permission cannot send it; the target
  remains unchanged after that denied attempt.

The final content PR targets the merged base after the remote destination PR. No
stacked PR is required. Provider-specific credential exporters and legacy cloud-index
aliases remain outside this resumed scope, as recorded in the coverage inventory.
