# Deployment selection adapters: checkpoint

The next selector group is retained on `codex/deployment-selection-adapters`,
based independently on merged PR #31 (`83bdcb470`). It is not ready for a PR.

Implemented locally: explicit named-selection contracts for custom site settings
and custom user settings. Both existing admin UpdateAsync methods now call the
same normalization helper as their adapters. Available names come from the
existing settings services, including their stereotype filtering. Strict test
build passes without warnings/errors; all six focused tests pass.

Before completing this slice, verify queued custom-settings exports. The existing
CustomSettingsDeploymentSource authorizes each selected type through
CustomSettingsService.CanUserCreateSettingsAsync, which reads the HTTP principal.
The background operation executor does not currently carry an execution principal.
This needs a live before/after check and a deliberate shared execution-identity
contract; do not bypass the source's permissions merely to make the export pass.
Custom-user-settings export has a separate existing per-type permission TODO that
also needs an explicit disposition. Translation selectors and runtime/transport,
canonical documentation, packaging, and PR gates remain unfinished.

## Priority adjustment

The opening campaign followed the important composition/settings/identity/index
work. After delivering core artifact export/import, continuing every deployment
factory adapter displaced higher-value gaps. Complete the query PR already in CI,
then prioritize tenant rate limits, audit/task administration, media administration,
and robots/sitemap management. Return to this selector checkpoint afterward. This
is a scheduling change, not removal from the agreed non-deferred scope.
