---
name: orchardcore-cli
description: Installs or updates the `pomi` CLI and uses it to initialize local Orchard CMS sites or manage remote tenants. Use for creating the initial website or a new SaaS host with pomi install, contexts, authentication, discovery, tenant setup, enabling Remote Management, compatibility checks, direct GraphQL, dynamic help, and coordinating module-specific management tasks.
---

# Pomi CLI

Use Pomi to create local Orchard CMS applications or manage an existing tenant.
Read the [shared context, authentication, and output rules](references/shared-rules.md)
before issuing commands. Specialists link to the same rules and can be used directly.
Load only the procedure or specialist needed for the user's task.

## Start a new website with Pomi

For a new Orchard application, including the initial **SaaS host**, start with
`pomi install <directory>`. It creates the project and initializes the Default
tenant without a context or login. Read [local installation](references/installation.md)
before creating files. For a requested SaaS host, pass `--recipe-name SaaS`;
otherwise prefer `--recipe-name Blank` unless the user selects another recipe.
Use `pomi tenants install` only to add a tenant to an **existing, running host**.
For automated building after setup, add `--enable-remote-management` to either
install command and use the returned `context`; it authenticates using stored
application credentials without `pomi login` or manual context creation. Omit
the option for setup-only work that should follow the recipe without additional
remote management. In both modes, hand over the administrator credentials in a
[private file](references/setup-password.md#credential-handoff).

If Pomi is unavailable, follow [CLI installation](references/cli-installation.md)
first. Do not substitute manual `dotnet new`, project scaffolding, or hand-written
Auto Setup configuration for `pomi install` unless the user explicitly requests
that approach. Report an installation blocker rather than silently bypassing Pomi.

## Choose the workflow

| Task | Read |
| --- | --- |
| Install or update the Pomi executable | [CLI installation](references/cli-installation.md) |
| Create a standalone application or the initial SaaS host (`pomi install`) | [Local installation](references/installation.md) |
| Create/setup a tenant in an existing server (`pomi tenants install`, create, setup) | [Tenant installation and Remote Management setup](references/tenants.md) |
| Connect, log in, approve a device code, or manage saved contexts | [Authentication and contexts](references/authentication.md) |
| Diagnose missing commands, cache freshness, permissions, or output | [Shared operating rules](references/shared-rules.md) |
| Define types, parts, fields, and content models | [Content definitions](../orchardcore-cli-content-definitions/SKILL.md) |
| Author, localize, publish, or restore content versions | [Content items](../orchardcore-cli-content-items/SKILL.md) |
| Upload images/files, obtain URLs, or manage custom CSS/JavaScript | [Media](../orchardcore-cli-media/SKILL.md) |
| Create Liquid shape overrides and verify rendering | [Templates](../orchardcore-cli-templates/SKILL.md) |
| Manage named Liquid shortcode snippets | [Shortcode templates](../orchardcore-cli-templates/SKILL.md#shortcode-templates) |
| Manage shape placement and filtering | [Shape placements](../orchardcore-cli-templates/SKILL.md#shape-placements) |
| Configure language-switch cookies and fallback | [Content culture picker](../orchardcore-cli-settings/SKILL.md#content-culture-picker) |
| Configure available layer zones | [Layer zones](../orchardcore-cli-settings/SKILL.md#layer-zones) |
| Attach and move existing widgets in layers | [Widget placement](../orchardcore-cli-templates/SKILL.md#widget-placement-in-layers) |
| Define conditional layers and verify widget visibility | [Conditional layers](../orchardcore-cli-templates/SKILL.md#conditional-layers) |
| Select installed site/admin themes | [Themes](../orchardcore-cli-themes/SKILL.md) |
| Create navigation with official menu content and shapes | [Menus](../orchardcore-cli-menus/SKILL.md) |
| Execute GraphQL documents or inspect its schema | [GraphQL](../orchardcore-cli-graphql/SKILL.md) |
| Manage URL rewrite/redirect rules and their order | [URL rewrite rules](../orchardcore-cli-settings/SKILL.md#url-rewrite-rules) |
| Manage Site Settings, typed module sections, Custom Settings, and cultures | [Settings](../orchardcore-cli-settings/SKILL.md) |
| Manage OpenID applications and scopes | [OpenID management](../orchardcore-cli-automation/SKILL.md#openid-applications-and-scope-management) |
| Manage features, recipes, queries, workflows, users, and roles | [Administration](../orchardcore-cli-automation/SKILL.md) |

## Coordinate a site build

For a complete site build, first choose new-host installation or existing-host
tenant provisioning above, then enable the required features,
design definitions, upload Media assets, author a draft, create templates, render
and refine, then publish when requested and verify public routes. For an existing
site, start at the relevant step and preserve its content model and configuration.

The [content definitions specialist](../orchardcore-cli-content-definitions/SKILL.md)
covers editable Flow/Bag composition. Keep presentation in templates and Media
assets; theme files are deployed with the application. Pomi has no static-file
upload API. Follow the user's chosen scope rather than treating a single-resource
request as permission to rebuild the site.
