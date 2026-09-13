# Deployment selector and execution authorization slice

Adds 26 explicit contracts: custom settings/user settings, translations, site properties,
six core/legacy Lucene index selectors and sixteen configuration-free export steps.
The admin editors and export sources share normalization, categories and site-property
selection. No reflection-based serialization of arbitrary step state is introduced.

Queued exports capture the initiating identity without tokens and restore OAuth role
semantics. Custom settings and custom-user settings enforce their existing resource
permissions. AllUsers requires ManageUsers because its existing recipe records contain
credential material. Denial fails the operation instead of omitting data silently.

Local validation before integration onto the newest merged base:

- Unchanged queued export omitted authorized custom settings; the same probe passes
  after the execution-principal fix. A subsequent live export includes user records.
- All 26 contracts pass HTTPS HTTP/Pomi/MCP discovery, configuration, retries, invalid
  updates and denied access checks.
- Strict full solution build: zero warnings/errors.
- Server suite: 3,681 passed, one skipped. CLI: 298 passed. MCP: 78 passed.
- Cross-tenant queued settings/content/media export/import verification is in progress.

Intentional boundaries: credential-bearing external identity provider exporters and
legacy cloud-index aliases still require explicit contracts. Modern index-profile
selectors cover provider-independent selection. Remote destinations and form-based
content export entry points are the next independent deployment slices.
