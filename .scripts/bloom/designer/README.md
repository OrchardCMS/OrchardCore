# @bloom/designer

The building blocks of the graph designers of the admin, such as the Workflows and the Data Pipelines designers. A
designer is a Vue application that edits a graph of nodes and edges, saves it to a draft as the user works, and
publishes it. It imports what it needs from `@bloom/designer/...` and keeps only what is specific to its graph: its
nodes and edges, its toolbox and its panels.

| Folder | What it holds |
|--------|---------------|
| `core` | Translations (`t(key, fallback, ...args)`), the API errors (`DesignerApiError`, `ProblemDetails`), the undo history, the revision queue and autosave. |
| `graph` | The graph model a designer describes its nodes and edges with (`GraphModel`), the rules of the edges (`rejectConnection`, `replacedBy`...) and the undoable commands. |
| `canvas` | `GraphCanvas`, the surface of a canvas (grid, pan, zoom, marquee, moving nodes, drawing edges, keyboard, drop, context menu), its geometry and viewport helpers. |
| `draft` | The draft chrome: save status, draft banner, conflict, publish and versions dialogs. |
| `panel` | `ServerFormHost`, which hosts a server-rendered editor and applies it as the user types, and `IssuesList`. |
| `ui` | The modal dialog, the toasts, the confirmation and the per-browser preferences. |
| `styles` | `designer-base.css`, the styles of the shared components (`ocd-*` classes), imported before the designer's own stylesheet. |

## Describing a graph

The shared code never reads the designer's types directly. A designer describes them with a `GraphModel`:

```ts
const model: GraphModel<DesignerNode, DesignerConnection> = {
    ends: (connection) => ({ sourceId: connection.sourceStepId, sourcePort: connection.sourcePort, targetId: connection.targetStepId, targetPort: connection.targetPort }),
    create: (ends) => ({ sourceStepId: ends.sourceId, sourcePort: ends.sourcePort, targetStepId: ends.targetId, targetPort: ends.targetPort }),
    key: (connection) => endsKey(model.ends(connection)),
    inputs: (node) => node.inputs,
    outputs: (node) => node.outputs.map((port) => ({ ...port, allowsMany: true })),
};
```

A port that doesn't `allowsMany` keeps one edge: a new edge to an input that accepts one replaces the previous one, and
so does a new edge from an output that leads to one node, such as a workflow outcome followed only once. An activity
without ports is described with an implicit input, and its outcomes as outputs.

## Translations

`t(key, fallback, ...args)` returns the server translation of `key` (loaded with `loadTranslations`), else the
designer's own wording (`setDefaultTexts`), else `fallback`. The shared components use generic English, such as
"The draft was changed elsewhere"; a designer names its own domain with `setDefaultTexts` or its server translations.

## Tests

```bash
cd .scripts/bloom
yarn vitest run
```
