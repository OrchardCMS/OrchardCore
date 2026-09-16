# Use Pomi with an agent

<img src="assets/logo.png#only-light" alt="Pomi" width="160" />
<img src="assets/logo-dark.png#only-dark" alt="Pomi" width="160" />

Give your coding agent the complete Pomi skill package to help it install an
Orchard CMS site, connect to a tenant, and manage content, media, settings, and
other enabled features. Install the **Pomi plugin from the Orchard Core Git
marketplace**: your agent fetches it, so no manual download or package build is
required. The website also provides versioned ZIPs and browsable instructions.
Opening a public skill URL does **not** register it with an agent.

The plugin also includes **Pomi, a web and content designer** for Copilot CLI.
It builds standalone websites or SaaS tenants, combines visual design with
structured content modeling, and aims for a site editors can maintain without
editing markup. Its agent profile uses the same ten bundled skills.

## Install Pomi and connect

[Install Pomi and authenticate](../guides/remote-management/README.md) on the
machine where the agent executes commands. Native remote management needs no
.NET runtime; local site installation needs the matching .NET SDK. A cloud
agent needs its own executable, network access, and authorized credentials;
installing skills on your laptop does not transfer them to a cloud workspace.

Verify the executable and select the intended tenant before giving the agent
work:

```bash
pomi --version
pomi context list
pomi --context my-site api compatibility
```

Use the [authentication reference](skills/orchardcore-cli/references/authentication.md)
for first-time login, device authorization, or client credentials. Do not paste
tokens or passwords into an agent prompt.

## Install in your agent

Marketplace installation keeps all ten skills and their references together.
For a manual skills installation, preserve **all ten sibling directories**;
individual specialists link to shared references in the main skill. Do not copy
just `SKILL.md` or flatten the layout. Download examples use macOS/Linux paths;
on Windows, use **Extract All** and equivalent absolute folder paths.

### Codex: install from the repository

Register the marketplace on the Pomi development branch, then install its plugin:

```bash
codex plugin marketplace add OrchardCMS/OrchardCore \
  --ref main
codex plugin add pomi@orchardcore
```

The first command registers the catalog; the second installs and enables Pomi.
Start a new Codex task and ask it to use `orchardcore-cli`, or select a specialist
such as `orchardcore-cli-content-items`. The plugin is called `pomi`; the ten
skill names remain `orchardcore-cli` and `orchardcore-cli-*`.

For release documentation, replace the development branch in `--ref` with the
corresponding release branch or tag that contains the marketplace. To match
**this documentation build exactly**, use the commit command under
[Version and provenance](#version-and-provenance).

To receive fixes from a branch and reinstall the updated plugin:

```bash
codex plugin marketplace upgrade orchardcore
codex plugin add pomi@orchardcore
```

Start a new task after updating. A full commit SHA pins a snapshot; a branch
moves when its maintainers push changes. Tags identify releases, but can be
moved by repository maintainers, so use a commit SHA for an immutable identity.
If your CLI lacks the plugin commands, update Codex; these examples use
`plugin add`, not `plugin install`.

Related skills are distributed as a plugin following
[OpenAI's distribution guidance](https://learn.chatgpt.com/docs/build-skills#distribute-skills-with-plugins).
See [Codex plugins](https://learn.chatgpt.com/docs/plugins) for discovery and
workspace availability. This is a repository marketplace, not a listing in
the OpenAI plugin directory.

### Claude Code: install the same plugin

Run these commands **inside Claude Code**:

```text
/plugin marketplace add OrchardCMS/OrchardCore@main
/plugin install pomi@orchardcore
```

Start a new session. Invoke `/pomi:orchardcore-cli`, or ask Claude to use the
relevant Pomi skill. Replace the text after `@` in the repository source with
a release branch or tag when needed. To update a branch installation:

```text
/plugin marketplace update orchardcore
/plugin update pomi@orchardcore
```

Claude Code uses the same checked-in skill files as Codex. See its official
[plugin installation guide](https://code.claude.com/docs/en/discover-plugins)
and [marketplace reference](https://code.claude.com/docs/en/plugin-marketplaces).
For an exact commit snapshot, use the commit-specific ZIP below and register
its extracted local marketplace.

### GitHub Copilot CLI: install the plugin

Copilot CLI recognizes the Claude-compatible catalog and manifest in this
repository:

```bash
copilot plugin marketplace add OrchardCMS/OrchardCore#main
copilot plugin install pomi@orchardcore
```

Start a new session. Use `#<release-branch-or-tag>` instead of the development
branch when needed. Update with `copilot plugin marketplace update orchardcore`
and `copilot plugin update pomi`. See the official
[Copilot CLI plugin reference](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-plugin-reference)
and [installation guide](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-finding-installing).

### Design a website with the Pomi agent

After installing or updating the plugin, start a new Copilot CLI session and
enter:

```text
/agent
```

Select **pomi**. Give it the audience, site purpose, brand material, target
context or new-site directory, and whether to publish. For example:

> Design a website for a community arts center in the existing arts context.
> Use a warm, editorial visual style. Include upcoming events, artist profiles,
> venue information, and booking links. Make events and artists reusable content
> that editors can manage independently. Keep the new content in draft.

For a **new SaaS host**, without an existing context:

> Create an Orchard Core SaaS host in ./ContosoCloud using Pomi, SQLite,
> and the SaaS recipe. Start it, guide me through login, then create a Blank
> tenant named Studio for a photography portfolio.

The agent uses `pomi install <directory> --recipe-name SaaS` for the initial
host. For an unspecified standalone site it prefers `pomi install <directory>
--recipe-name Blank`. Neither command needs an existing Orchard context. The
installer creates the project and runs setup; agents should not replace it with
manual project scaffolding. If Pomi is missing, the installation skill handles
that prerequisite. Generated administrator credentials are saved privately for
you before setup.

For a tenant on an **existing SaaS host**:

> Using my authenticated Default-tenant context, create a tenant named Studio
> at the studio URL prefix. Build a photography portfolio with reusable project
> galleries and a distinctive monochrome design. Use SQLite and guide me through
> authenticating to the child tenant before creating its content.

Pomi inspects the tenant's capabilities, models editable content and page
sections, creates Liquid presentation and Media assets, and verifies the
visitor and editor workflows. It preserves an existing brand when requested
and checks narrow and wide layouts when browser tools are available. Draft
content does not isolate template or theme changes on a live site; specify a
staging tenant for a redesign that must remain private.

The agent inherits your session's model and available tools. It does not install
Pomi, provide credentials, or grant extra Orchard permissions by itself; it can
follow the installation and authentication skills as part of your request.
No separate agent installation is needed with the complete plugin. The
**skills-only ZIP does not include the agent profile**. Other clients can use
the existing skills; agent discovery and selection depend on the client.

Browse the [designer instructions](agents/pomi.agent.md) or read the
[raw agent profile](raw/agents/pomi.agent.md). The canonical profile is
`agents/pomi.agent.md` inside the plugin, alongside `skills/`. Its relative
references remain resolvable when the plugin is installed
outside this repository. See [Copilot's plugin agent documentation](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-creating).

### Copilot and other agents: install standalone skills

1. Download and extract the **Skills-only ZIP**. The outer directory is
   `pomi-skills`, containing `skills/`, a license, and package metadata.
2. Copy the ten directories under `pomi-skills/skills/` into your repository's
   `.github/skills/`. For example:

    ```text
    .github/skills/orchardcore-cli/SKILL.md
    .github/skills/orchardcore-cli/references/shared-rules.md
    .github/skills/orchardcore-cli-content-items/SKILL.md
    ...
    ```

3. Start an agent session and ask Copilot to use the Pomi skills. Commit the
   directories if a repository-based cloud agent or your team should receive them.

Copilot also supports `.agents/skills/` for project skills. Choose one location
rather than installing duplicates. For personal local skills, use
`~/.copilot/skills/`. The plugin manifest is not needed for this installation.
See [GitHub's skill installation instructions](https://docs.github.com/en/copilot/how-tos/copilot-on-github/customize-copilot/customize-cloud-agent/add-skills).

For Claude Code standalone skills, copy the same ten directories into
`.claude/skills/` (project) or `~/.claude/skills/` (personal). Invoke
`/orchardcore-cli` without the plugin namespace. See
[Claude Code skills](https://code.claude.com/docs/en/skills). Choose the plugin
or standalone installation to avoid duplicate listings.

### Try a bounded task

For example:

> Use the Pomi content-items skill to inspect the Article schema in the `my-site`
> context and validate an unpublished draft. Do not publish it.

The agent should discover the tenant's actual commands, use the shared context,
authentication, and output rules, and stay within your requested scope. A public
Markdown link is useful for reading instructions, but installation is what makes
the complete package discoverable to the agent.

## Optional downloads

These downloads belong to the documentation version selected in the website's
version selector. Both ZIPs contain the same ten canonical skill directories
and all their references.

| Download | Purpose |
| --- | --- |
| [Pomi plugin ZIP](../downloads/pomi-plugin.zip) | Complete skills, references, Codex and Claude Code manifests, local marketplaces, license, and Pomi branding |
| [Skills-only ZIP](../downloads/pomi-skills.zip) | The same skill directories for agents with other packaging requirements |
| [Browse skills](#browse-skills) | Readable instructions and raw Markdown links, including linked references |

[Package metadata](../downloads/package-metadata.json) and
[SHA-256 checksums](../downloads/SHA256SUMS) identify the exact build. Use a ZIP for an
archived snapshot or a local installation without fetching the Git marketplace.
The packages do not contain the Pomi executable, credentials, connectors, or
background services.

### Install a downloaded plugin ZIP

Extract the **Pomi plugin ZIP** to a stable directory such as
`~/agent-packages/pomi-plugin`. Its root contains both marketplace catalogs
and `plugins/pomi/`. Register that local marketplace, then install:

```bash
codex plugin marketplace add "$HOME/agent-packages/pomi-plugin"
codex plugin add pomi@pomi-download
```

Inside Claude Code, use `/plugin marketplace add /absolute/path/to/pomi-plugin`
and `/plugin install pomi@pomi-download`. Alternatively, load one session with
`claude --plugin-dir /absolute/path/to/pomi-plugin/plugins/pomi`.
Copilot CLI can use `copilot plugin marketplace add /absolute/path/to/pomi-plugin`
and `copilot plugin install pomi@pomi-download`.

The local catalog is named `pomi-download` to distinguish it from the Git
marketplace. Install one form to avoid duplicate skills. Keep the extracted
directory; to update it, extract the new ZIP there and reinstall its plugin.
Start a new agent session after installation or updates. Older downloads named
the plugin `orchardcore-cli`; remove that old installation when switching to
`pomi`.

## Browse skills

The rendered pages and raw Markdown are generated from
`.agents/skills/pomi/skills/` during the same build as the ZIPs. Each readable
page links to its raw Markdown; raw references retain the original relative
links and bytes.

| Workflow | Readable page | Raw instructions |
| --- | --- | --- |
<!-- pomi-skill-list -->

## Version and provenance

<!-- pomi-package-metadata -->

Release documentation builds use the checked-out release branch or tag; `latest`
uses the configured development branch. A release branch can receive fixes, so
a documentation version name alone is not an immutable package identity. Save
the commit-specific ZIP and its checksum for reproducible use. Each build serves
its own commit-specific filenames; it does not retain downloads from every older
build. The docs CI artifact also records the source commit and has GitHub's
artifact retention limits.

Maintainers should activate the desired release tags in Read the Docs and point
`latest` at the development branch. No skill content is fetched from another
branch during a build. These files appear below the selected version, for example
`/en/<version>/downloads/pomi-plugin.zip` and `/en/<version>/agents/`.
Local builds may report uncommitted inputs; those filenames include `working`
and must not be treated as release artifacts.

To reproduce a committed artifact, check out its `sourceCommit` with a clean
working tree and run:

```bash
python3 .scripts/remote-management/build-plugin.py /tmp/pomi-package-rebuild
```

Compare the generated checksums with the downloaded `SHA256SUMS`. ZIP entry order,
timestamps, and permissions are fixed; no build date or local path enters the
archives. All ten canonical skills remain in `.agents/skills/pomi/skills/`;
generated website pages and archives are not maintained as separate source
copies. Both repository
catalogs point to `.agents/skills/pomi/`, which already contains the manifests,
references, license, compatibility information, and branding. Marketplace
installers do not run `build-plugin.py`; that script only generates optional
ZIPs and documentation assets.
