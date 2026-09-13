# Resumed remote management campaign

The user resumed the campaign after the four-area completion phase. Deliver the
following areas in order, using independent PRs against
`sebros/remote-tenant-cli-plan` in `sebastienros/OrchardCore`. Merge only after
local verification and CI succeed; refresh the base before the next PR.

| Area | Scope | State |
| --- | --- | --- |
| SMTP | Typed tenant SMTP and default email-provider settings, redacted secrets, delivery testing, shared admin validation and mutations | Merged PR #38 (`b7be36756`); all CI green |
| User/security settings | Existing registration, password-reset, email-change and MFA policy settings; custom user settings; supported tenant OpenID server/client/validation settings with explicit ownership | Policies merged PR #39; custom user settings merged PR #40; OpenID merged PR #41 (`c5b7b0582`); all CI green |
| Deployment completion | Remaining explicit step adapters, initiating-user authorization in background exports, remote clients/instances/targets, content export entry-point parity | Selectors and execution-principal merged PR #42 (`8d0ef4359`); remote destinations locally verified on merged base; content export parity in final verification |

Count progress against this scope, not the completed earlier campaign. Split each
area into reviewable independent PRs where needed. Every slice includes canonical
docs, Pomi skill guidance, HTTP/Pomi/MCP discovery and permission checks, tests of
existing callers, and tenant isolation. Preserve disabled publishing CI jobs.

Do not include admin templates/menu/dashboard CRUD, other email/SMS/cloud
providers, new authentication recovery protocols, or the unrelated deferred
backlog. Source audits must record unsupported host-owned settings explicitly.

The deployment selector checkpoint at `3d1b48f8f` is unfinished work to review,
not a ready-to-merge change. In particular, background custom-settings exports
must enforce the initiating user's existing permissions without depending on an
ambient HTTP request or bypassing authorization.
