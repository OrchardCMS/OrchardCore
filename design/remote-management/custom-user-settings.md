# Custom user settings management

Adds feature-owned types/schema/show/update operations for embedded user settings.
Uses the existing CustomUserSettingsService in both the admin display driver and
the API; the original driver constructor remains compatible. Shared safe-envelope
validation/schema code is extracted from CustomSettingsManagementService into
EmbeddedContentItemApi and used by both site and user settings.

The API requires remote access, the dynamic type permission, and resource-based
ViewUsers/EditUsers permission. Writes require HTTPS. Content handlers validate a
detached item before UserManager persists the owner; failed updates cancel the
session, and successful updates remove the embedded item from standalone content
tracking. Identity/security fields and unrelated user properties are not writable.

Verification includes real YesSql/Identity persistence, partial updates, denied
user access, rejected account fields, safe read envelopes and no standalone
content row, plus existing custom-site-settings tests after the extraction.
All twenty focused tests pass. Live HTTPS verification passes through HTTP, Pomi
and MCP, including discovery, readback, rejected identity fields, equivalent
retries and a child tenant with independent feature and settings state. The full strict build passed, and the complete server suite passed (3,653 passed,
one skipped) on the independent SMTP base. Documentation and CI verification
remain required before merge.
