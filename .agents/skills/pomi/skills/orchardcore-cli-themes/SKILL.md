---
name: orchardcore-cli-themes
description: Lists, enables, and selects Orchard Core site and admin themes through `pomi`. Use when discovering installed themes, activating a theme and its base themes, setting the current frontend or admin theme, or verifying tenant theme state before applying templates and static styling.
---

# Pomi CLI Themes

Before issuing commands, read the [shared context, authentication, and output rules](../orchardcore-cli/references/shared-rules.md).
For first-time access or login problems, follow [authentication and contexts](../orchardcore-cli/references/authentication.md).
These rules apply even when this specialist is selected directly.

Manage themes in the target tenant context. The authenticated identity requires
`AccessRemoteManagement` and `ApplyTheme`.

## Workflow

```bash
pomi --context site themes list --take 200
pomi --context site themes enable TheTheme
pomi --context site themes set-current TheTheme
pomi --context site themes list --current true
```

`set-current` detects whether the manifest declares a site or admin theme,
selects it in the corresponding setting, and enables it and its base themes
when necessary.

Filter discovery:

```bash
pomi themes list --search Agency
pomi themes list --admin false --enabled true
pomi themes list --admin true
```

The list includes only themes allowed by the active feature profile and omits
hidden, always-enabled, dependency-only, and non-theme features.
Each result includes `id`, `name`, `description`, `isAdmin`, `isEnabled`, and
`isCurrent`.

## Reliability

- Theme IDs match case-insensitively for lookup; use the canonical returned ID
  in automation.
- Repeating `enable` for an enabled theme performs no additional mutation.
- Repeating `set-current` for the selected theme preserves the same setting.
- A missing or non-manageable theme returns `404`.
- Refresh discovery only when theme enablement changes the available API
  surface: `pomi api refresh --force`.

After selecting a site theme, use [orchardcore-cli-media](../orchardcore-cli-media/SKILL.md) for tenant CSS/assets
and [orchardcore-cli-templates](../orchardcore-cli-templates/SKILL.md) for custom Liquid shape overrides. Verify the
public route because selecting a theme does not prove its resources or content
templates render correctly.

Official references (live tenant schemas take precedence):
[themes API](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/api/themes/README.md) and
[Themes module](https://github.com/OrchardCMS/OrchardCore/blob/main/src/docs/reference/modules/Themes/README.md).
