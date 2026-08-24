# AI Quick Start

Orchard Core is a large, modular framework, and the fastest way to become productive with it is to let an AI assistant work alongside you. Instead of searching the documentation by hand, you can ask an assistant to scaffold modules, write recipes, generate content type migrations, and explain how a feature works — all grounded in accurate, up-to-date Orchard Core knowledge.

The key to accurate answers is the official **Orchard Core MCP server**. It gives AI assistants direct, tool-based access to the Orchard Core documentation and APIs, so responses reflect how Orchard Core actually works rather than guesses.

## What is the MCP server?

[Model Context Protocol (MCP)](https://modelcontextprotocol.io) is an open standard that lets AI assistants call external tools and data sources. The Orchard Core MCP server exposes tools that let an assistant search and read the Orchard Core documentation so it can answer questions and generate code with the correct APIs, module names, and conventions.

The server is hosted at:

```text
https://ai.orchardcore.net
```

Point any MCP-capable AI client at this URL, and the assistant gains access to the Orchard Core tools automatically.

## Connecting your AI assistant

The exact steps depend on the client you use, but they all point to the same remote endpoint, `https://ai.orchardcore.net`. Below are the most common clients. Since Orchard Core is a .NET project, GitHub Copilot is listed first.

### GitHub Copilot CLI

The [GitHub Copilot CLI](https://docs.github.com/en/copilot/how-tos/set-up/install-copilot-cli) brings a Copilot coding agent to your terminal. Install it with npm (requires [Node.js](https://nodejs.org/)):

```bash
npm install -g @github/copilot
```

Start it by running `copilot`, then register the MCP server:

```bash
copilot
/mcp add
```

When prompted, choose an **HTTP** server, name it `orchardcore`, and use the URL `https://ai.orchardcore.net`. Alternatively, add it to your Copilot CLI `mcp-config.json`:

```json
{
  "mcpServers": {
    "orchardcore": {
      "type": "http",
      "url": "https://ai.orchardcore.net"
    }
  }
}
```

### GitHub Copilot (online)

For GitHub Copilot on github.com (Copilot Chat and the Copilot coding agent), add the Orchard Core MCP server in your repository or organization settings under **Settings → Copilot → Coding agent → MCP configuration**:

```json
{
  "mcpServers": {
    "orchardcore": {
      "type": "http",
      "url": "https://ai.orchardcore.net"
    }
  }
}
```

Once saved, Copilot can call the Orchard Core tools when working on issues and pull requests.

### Visual Studio Code (GitHub Copilot)

Create or edit `.vscode/mcp.json` in your workspace:

```json
{
  "servers": {
    "orchardcore": {
      "type": "http",
      "url": "https://ai.orchardcore.net"
    }
  }
}
```

Open the Chat view in **Agent** mode and confirm the `orchardcore` server is listed among the available tools.

### Claude Code

Claude Code is a terminal-based AI coding assistant. If running `claude` returns an error such as `'claude' is not recognized as an internal or external command` (or `command not found` on macOS/Linux), it is not installed yet. Install it with npm (requires [Node.js](https://nodejs.org/)):

```bash
npm install -g @anthropic-ai/claude-code
```

Verify the installation:

```bash
claude --version
```

Then add the Orchard Core MCP server:

```bash
claude mcp add --transport http orchardcore https://ai.orchardcore.net
```

Start a session and ask Orchard Core questions — the assistant will call the server's tools when it needs Orchard Core knowledge.

### Claude Desktop

Edit your `claude_desktop_config.json` (Settings → Developer → Edit Config) and add the server under `mcpServers`:

```json
{
  "mcpServers": {
    "orchardcore": {
      "type": "http",
      "url": "https://ai.orchardcore.net"
    }
  }
}
```

Restart Claude Desktop for the change to take effect.

### Cursor

Edit `.cursor/mcp.json` in your project (or the global `~/.cursor/mcp.json`):

```json
{
  "mcpServers": {
    "orchardcore": {
      "url": "https://ai.orchardcore.net"
    }
  }
}
```

### Other clients

Any client that supports remote (HTTP) MCP servers can connect. Look for an "MCP", "Model Context Protocol", or "Tools/Connectors" setting and register a new **HTTP** server with the URL `https://ai.orchardcore.net`.

## Power your assistant with AI Skills

Beyond the MCP server, you can teach your assistant repeatable, Orchard Core–specific workflows using **AI Skills**. The [CrestApps.AgentSkills](https://github.com/CrestApps/CrestApps.AgentSkills) project provides a curated set of skills and plugins for building Orchard Core solutions with AI.

!!! note
    [CrestApps.AgentSkills](https://github.com/CrestApps/CrestApps.AgentSkills) is a community project. It is maintained by the community and is not an official part of Orchard Core.

You can install these as plugins for either the **Claude** CLI or the **GitHub Copilot** CLI, which is an alternative way to power your AI tool with reusable skills. Follow the instructions in the [CrestApps.AgentSkills repository](https://github.com/CrestApps/CrestApps.AgentSkills) to install the plugin for your CLI of choice.

!!! tip
    Combine the Orchard Core MCP server with the AI Skills plugins for the best results: the MCP server keeps the assistant grounded in accurate documentation, while the skills guide it through common Orchard Core tasks step by step.

## Building on top of Orchard Core with AI

Once connected, use natural language to build on top of your Orchard Core workflow. Because the assistant can consult the official documentation through the MCP server, its suggestions stay aligned with the framework's conventions. Some examples of what to ask:

- **Scaffold a module or theme** — "Create a new Orchard Core module that adds a `Product` content type with a price field."
- **Write recipes** — "Generate a recipe that creates a blog content type, a menu, and a home page."
- **Content modeling** — "Write a data migration that adds an `AutoroutePart` and a `MarkdownBodyPart` to my `Article` type."
- **Explain features** — "How do display drivers and shapes work, and how do I customize the rendering of a part?"
- **Extend behavior** — "Show me how to handle a content event so I can send a notification when an item is published."

!!! tip
    Give the assistant context about your project (the modules you have enabled, the content types you use, and your target Orchard Core version) so its answers and generated code fit your solution.

!!! note
    The MCP server provides the AI with Orchard Core knowledge, but you remain responsible for reviewing generated code and recipes before running them in your application.

## Next steps

- [Create a CMS Web application](README.md)
- [Recipes and Starter Themes](starter-recipes.md)
- [Code Generation Templates](templates/README.md)
