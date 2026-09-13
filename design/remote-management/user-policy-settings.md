# User policy settings

The user/security tranche begins with eleven typed site-policy sections:
user-login, user-registration, user-password-reset, user-change-email,
user-external-login, user-external-registration, user-two-factor,
user-role-two-factor, user-authenticator-app, user-email-authenticator and
user-sms-authenticator. Each follows the feature that owns the existing admin
editor and requires ManageUsers plus remote-management access. Writes use HTTPS.

UserPolicySectionProvider uses explicit typed field delegates, not arbitrary CLR
property discovery. Unknown fields and invalid types/ranges are rejected before
mutations. Nullable text and role arrays have documented resets. Equivalent
updates skip persistence; registration/external-login option changes use existing
post-commit options signals.

UserPolicySettingsEditor supplies detached copies and shared mutations for all
eleven existing admin editors. MFA count validation and Liquid-template validation
preserve invalid-update state. Role selection uses existing assignable-role
discovery, rejects missing roles when enabled, and allows disabling with stale
inactive selections. Authenticator codes follow the existing six-digit limit.

External authentication and MFA core are dependency-only features: activate them
through an authentication provider or MFA method. The smoke test enables an
unconfigured GitHub provider solely to activate the existing external policy;
it makes no external authentication request and does not add provider credentials.

Local evidence: sixteen focused tests, strict module/web/test builds,
eleven HTTP/Pomi/MCP settings workflows, permission denials, invalid-value
preservation, equivalent retries, independent child-tenant policies, and the real
ForgotPassword route changing between HTTP 200 and 404. Both invalid-update regressions fail on the unchanged admin editors and pass
with shared validation. Real options-monitor checks cover both the previous update
path and the new management sections without a tenant reload. The complete server suite passed (3,666 passed, one skipped), as did all 298 CLI
and 78 remote-management/MCP tests. The final single-worker full solution build
passed with warnings treated as errors; all sixteen focused tests and the live
HTTPS smoke test passed again on the final source. CI is required before merging.

This slice does not expose passwords, authenticator keys or recovery codes, enroll
users, or perform password recovery. Custom user settings and OpenID configuration
are subsequent independently reviewed slices. Host password-complexity/lockout and
cookie settings retain their configuration ownership.
