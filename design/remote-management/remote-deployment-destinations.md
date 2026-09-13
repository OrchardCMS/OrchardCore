# Remote deployment management

Shared admin/API validation, explicit client/instance contracts, credential-free
readback and secret-safe CLI inputs. Target listing exposes only names/IDs and
send uses the same archive service and transport as the admin action.

The transport disables redirects and requires HTTPS except loopback. The receiver
uses existing bounded package staging and constant-time protected key comparison.
A receiver exception now returns HTTP 500 instead of a misleading success response;
imports and sends are not automatically retried.

Initial local evidence: 16 transport/admin archive tests passed; receiver exception
regression passes and verifies staging cleanup. Live HTTP/Pomi/MCP client/instance
CRUD, key preservation/redaction, tenant isolation and actual recipe delivery pass.
Final integrated full solution build has zero warnings/errors; full server suite has 3,681 passed and one skipped. CLI 298 and MCP 78 pass. Fresh HTTPS source/target probes verify separate management/export permissions and successful recipe delivery.
