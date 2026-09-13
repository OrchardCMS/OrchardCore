# SMTP and email management

## Contract

The Email feature contributes `settings sections show/schema/update email` and
`email test`. SMTP contributes `settings sections show/schema/update smtp`.
All operations require AccessRemoteManagement and ManageEmailSettings; SMTP
writes additionally require HTTPS. Feature-owned typed providers reuse existing
settings endpoints, including MCP tool projection.

SMTP exposes tenant settings only. The Default SMTP configuration provider and
pickup directory base remain host-owned. Passwords are write-only, protected with
the existing data-protection purpose, and never returned as ciphertext. Omission
retains the password; clearPassword removes it. Equivalent password updates retain
the existing protected value and do not invalidate options. Invalid input is
validated before mutations; unknown properties and numeric enums are rejected.

The email section lists enabled technical provider names and selects the default.
SMTP's technical name is `SMTP`. The test operation sends one bounded plain-text
message using IEmailService, exactly the service used by the admin test form.
Provider errors are not echoed into the API. HTTP 204 represents provider
acceptance, not receipt by the destination inbox. Retries can send duplicate mail.

## Existing caller reuse

SmtpSettingsDisplayDriver and the SMTP section share SmtpSettingsEditor, including
sender/path validation, credential encryption, default-provider transitions and
post-commit options invalidation. Invalid values no longer mutate the admin editor's
settings object before ModelState validation. The new admin regression fails on
the unchanged base editor (the host becomes `changed-host` despite an invalid sender)
and passes with the shared editor. Password fields no longer receive
protected ciphertext when rendering the admin form.

EmailSettingsDisplayDriver and the email section share EmailSettingsEditor for
provider availability and default selection changes. Delivery goes through the
existing IEmailService and its validation/events/provider pipeline; there is no
second SMTP transport implementation.

## Validation

- Strict full-solution, test-project and web-host builds pass with zero warnings/errors.
- Fifteen focused tests cover invalid patch preservation, secrets, unchanged retries,
  enabling/disabling defaults, denied delivery, error redaction, the existing admin
  editor and the real signal-backed SMTP options refresh without tenant reload.
- Full server suite: 3,652 passed, one skipped, zero failures. CLI 298 and
  remote-management/MCP 78 regressions pass.
- smtp-smoke.py verifies HTTPS settings, permissions, MCP readback, invalid-patch
  preservation, two tenant-specific pickup files/settings, and a real MailKit
  authenticated connection to a loopback SMTP sink. No external mail is sent.
- Strict documentation builds and package checks accompany the PR.

The full solution initially failed on the unchanged base because Testcontainers
brought in vulnerable SSH.NET 2025.1.0. Independent prerequisite PR #37 merged as `a8f5aa242` and updates
that test-only dependency to 2026.0.0. SMTP integrates the merged base and passes
the strict full-solution build. No vulnerability auditing is suppressed.
