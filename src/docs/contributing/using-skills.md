# Using Orchard Core Agent Skills

Orchard Core ships a set of **agent skills** that help AI coding assistants (such as Claude Code, GitHub Copilot, or any agent that supports the skills convention) perform common Orchard Core development tasks consistently and correctly.

A skill is a self-contained folder that bundles step-by-step instructions, code templates, naming conventions, and reference material for a specific task. When an AI agent loads a skill, it follows the project's recommended workflow instead of guessing, which produces output that matches Orchard Core conventions.

## Where the skills live

The skills are stored in the repository under:

```
.agents/skills/
```

Each skill is a directory containing a `SKILL.md` entry point and an optional `references/` folder with deeper documentation:

```
.agents/skills/
├── orchardcore-module-creator/
│   ├── SKILL.md
│   └── references/
│       ├── module-structure.md
│       ├── patterns.md
│       └── examples.md
├── orchardcore-theme-creator/
├── orchardcore-asset-manager/
├── orchardcore-admin-edit-views/
└── orchardcore-tester/
```

Every `SKILL.md` starts with YAML front matter that declares the skill's `name` and a `description` telling the agent **when** to use it:

```markdown
---
name: orchardcore-module-creator
description: Creates new OrchardCore modules with proper structure, manifest, startup, and patterns. Use when the user needs to create a new module, add content parts, fields, drivers, handlers, or admin functionality.
---
```

## Available skills

| Skill | Use it when you need to… |
|-------|--------------------------|
| `orchardcore-module-creator` | Create a new module, add content parts, fields, drivers, handlers, or admin functionality. |
| `orchardcore-theme-creator` | Create a new theme, customize layouts, or set up frontend assets. |
| `orchardcore-asset-manager` | Build, watch, or manage frontend assets (SCSS, JS, TS, Vue) and troubleshoot the asset pipeline. |
| `orchardcore-admin-edit-views` | Create or modify admin edit views (`*.Edit.cshtml`) using the `ocat-*` CSS class conventions. |
| `orchardcore-tester` | Build, run, set up, and test features through browser automation with Playwright. |

## How to use the skills

The exact mechanics depend on your AI assistant, but the general flow is the same.

### 1. Open the repository in an agent that supports skills

Clone Orchard Core and open it in your AI coding assistant. Agents that follow the skills convention automatically discover the `.agents/skills/` folder. The repository root also contains an [`AGENTS.md`](https://github.com/OrchardCMS/OrchardCore/blob/main/AGENTS.md) file with project-wide guidance (build commands, conventions, and a pointer to the skills) that most agents read on startup.

### 2. Describe your task in natural language

You don't invoke a skill by name. Instead, describe what you want and the agent matches your request against each skill's `description`. For example:

- *"Create a new module that adds a Rating content part."* → triggers `orchardcore-module-creator`
- *"Add a new dark admin theme."* → triggers `orchardcore-theme-creator`
- *"Rebuild the SCSS for this module and watch for changes."* → triggers `orchardcore-asset-manager`
- *"Fix the layout of this field's edit view."* → triggers `orchardcore-admin-edit-views`
- *"Run the app and test that media upload works."* → triggers `orchardcore-tester`

### 3. Follow the guided workflow

Once a skill is active, the agent walks through its documented steps — creating the required files, applying the correct naming conventions, registering services in `Startup.cs`, and pulling additional templates from the skill's `references/` files when needed.

### 4. Read the references directly (optional)

The skills are plain Markdown, so they are also useful to humans. Browse a `SKILL.md` or its `references/` files to learn the recommended structure for a module, theme, or admin view even without an AI agent.

## Contributing a new skill

Skills are part of the repository and evolve with the codebase. To add or improve one:

1. Create a folder under `.agents/skills/<skill-name>/`.
2. Add a `SKILL.md` with `name` and `description` front matter. Keep the `description` action-oriented and explicit about when the skill applies — this is what the agent matches against.
3. Put the high-level workflow in `SKILL.md` and move long templates or detailed patterns into a `references/` subfolder so the entry point stays focused.
4. Follow the conventions in [`AGENTS.md`](https://github.com/OrchardCMS/OrchardCore/blob/main/AGENTS.md) and the existing skills.
5. Open a pull request. See [Contributing code](contributing-code.md) for the general process.

When you add a skill, also list it in the *Available Skills* table in `AGENTS.md` so agents and contributors can discover it.
