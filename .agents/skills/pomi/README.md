# Pomi agent plugin

Start with the [workflow router](skills/orchardcore-cli/SKILL.md), or select a
specialist directly. Each links to bundled context, authentication, and output
rules. Longer manuals use official links and require network access.
Live tenant schemas take precedence. Install Pomi separately; this package
contains instructions and branding, not a CLI executable or credentials.

## Web and content designer agent

The plugin includes [Pomi, a web and content designer](agents/pomi.agent.md).
It takes a site brief through visual design, structured content, implementation,
and verification, using the ten skills for command procedures. It supports
standalone CMS applications and tenants in an existing SaaS host, and keeps
content editable through Orchard rather than embedding whole pages in HTML.

In Copilot CLI, install the plugin and open the agent selector:

```bash
copilot plugin marketplace add OrchardCMS/OrchardCore#main
copilot plugin install pomi@orchardcore
copilot
```

Enter `/agent` and select `pomi`. For example:

> Use the existing studio context to design a photography website with an
> editorial visual style, reusable project galleries, an about page, and clear
> contact navigation. Keep the content easy to maintain and leave it in draft.

The profile inherits the session's model and available tools; it does not add
credentials, browser tools, or server permissions. Its procedures are linked
relative to the installed plugin. The full plugin ZIP includes the agent;
the skills-only ZIP includes only the skills and their references.

## Install from the Orchard Core marketplace

```bash
codex plugin marketplace add OrchardCMS/OrchardCore --ref main
codex plugin add pomi@orchardcore
```

Start a new task and ask for the `orchardcore-cli` skill. Claude Code uses the
same skill files; run these commands inside Claude Code:

```text
/plugin marketplace add OrchardCMS/OrchardCore@main
/plugin install pomi@orchardcore
```

See [compatibility.json](compatibility.json) for supported Pomi/server versions.
The manifests hold the package version. Git marketplace installs select the
source revision; generated ZIPs also include `package-metadata.json` with the
exact source commit and checksums.

## Maintaining the package

This directory is the complete, directly installable plugin and the canonical
source of the designer agent and all ten skills. Keep every sibling directory and reference together.
Bump the version in both manifests when changing the shipped agent, skills, or assets, so
marketplace clients can detect updates. The documentation builder packages this
directory; it does not generate an independent copy of the instructions.
