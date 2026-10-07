# Phase 6 — Real time with OrchardCore.SignalR

Live updates in the designer and the instance viewer, built on `OrchardCore.SignalR`: who else has the workflow open, a notice when someone else changes or publishes it, and an instance page that follows the instance as it runs. Autosave stays on REST. This turns the sketch in [`later-phases.md`](later-phases.md#phase-6--real-time-with-orchardcoresignalr) into steps.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Target experience

```
┌ Approval  [Draft · saved]   Also here: (AL) (BO)            [Versions] [Publish] ┐
│ ⓘ Alice changed this workflow. Reload to see their changes.  [Reload]  [×]     │
└──────────────────────────────────────────────────────────────────────────────────┘

Instance #42 (Halted → Executing → Finished)   the canvas and the journal follow the instance live
```

## Decisions

| # | Decision | Why |
|---|---|---|
| R1 | A feature, **`OrchardCore.Workflows.SignalR`**, depends on `OrchardCore.Workflows` and `OrchardCore.SignalR`. It maps `WorkflowsHub` at `/hubs/workflows`, with a `WorkflowsHub` policy (the cookie and API schemes, and `ManageWorkflows`). Without the feature, everything works as before. | Real time is optional, and needs SignalR's services. Same layout as `OrchardCore.Media.SignalR`. |
| R2 | **Groups** per workflow type (`workflow-type:{workflowTypeId}`) and per instance (`workflow:{workflowId}`). Subscribing checks `ManageWorkflows` and that the type or instance exists. | Group membership is shared by the Redis and Azure backplanes, so it works on several nodes. |
| R3 | **Events** are sent after the request's changes are committed (a deferred task of the shell scope), through `IWorkflowDesignerNotifier`, whose default implementation does nothing: `DraftChanged` (revision, who), `Published` (version, who), `DraftDiscarded`, and `InstanceChanged` (status, after each save of the instance). | Clients that reload on an event see the committed state. The engine and the draft manager don't depend on SignalR. |
| R4 | **Presence** is announced between clients: joining a type's group tells the others, who answer to the newcomer; leaving (or disconnecting) tells the group. The server only relays, and keeps no list. | It works across nodes without shared state. |
| R5 | **Client**: the designer configuration carries `hubUrl` (tenant-aware) when the feature is on, and the page loads the `signalr` resource. The designer connects with the global `signalR` client, subscribes again when it reconnects, shows who else is there, and shows a notice with **Reload** when someone else changes, publishes or discards the draft. The instance viewer reloads the instance when it changes. | No new npm dependency, and nothing changes when the feature is off. |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [x] 6.1 Notifier and events

- `A/Services/IWorkflowDesignerNotifier.cs` (`WorkflowTypeChangedAsync` with the kinds `DraftChanged`, `Published` and `DraftDiscarded`, and `InstanceChangedAsync`), a no-op default in `M/`, called by the draft manager and by `WorkflowManager` when it saves or deletes an instance.
- **Tests**: the draft manager and the manager call the notifier with the right data.
- **Notes from implementing this step:**
  - **Notifier.** `IWorkflowDesignerNotifier` has two methods:
    - `WorkflowTypeChangedAsync(WorkflowTypeChange)`: the kind (`DraftChanged`, `Published` or `DraftDiscarded`), the type, the draft's revision, the published version, and who made the change.
    - `InstanceChangedAsync(WorkflowInstanceChange)`: the instance, its type, its status, and whether it was deleted.
    - `NullWorkflowDesignerNotifier` is registered by default and does nothing.
  - **Draft manager.**
    - Every successful change of the draft notifies `DraftChanged` with its new revision.
    - Publishing notifies `Published` with the new version.
    - Discarding notifies `DraftDiscarded`, only when there was a draft.
    - The user comes from the request's claims.
  - **Engine.** `WorkflowManager` notifies after each save of an instance (`PersistAsync`). An instance deleted when it finishes notifies with `IsDeleted`.
  - **API change.** `WorkflowManager` and `WorkflowTypeDraftManager` take an `IWorkflowDesignerNotifier` (release notes in 6.4).
  - **Tests.**
    - The draft manager notifies a save (with the user), a publish and a discard, and nothing for a discard without a draft.
    - A started instance is notified with its status.
    - Workflows tests: 238/238.

### - [ ] 6.2 Hub and feature

- Feature `OrchardCore.Workflows.SignalR`: `WorkflowsHub` (subscribe and unsubscribe, presence relay), the policy, the route, and `SignalRWorkflowDesignerNotifier`, which sends the events to the groups after the scope's changes are committed.
- The designer and viewer configurations carry `hubUrl` when the feature is on; their pages load the `signalr` resource.
- **Tests**: the policy and the subscribe checks; the notifier sends to the right groups after the commit; the configuration has `hubUrl` only with the feature.

### - [ ] 6.3 Designer and viewer clients

- A `realtime` service in the designer (connection, subscriptions, reconnect), presence avatars in the toolbar, the change notice with **Reload**, and the viewer's reload on `InstanceChanged`.
- **Tests** (Vitest, with a fake connection): subscribe and resubscribe, presence, notices only for others' changes, the viewer's reload.

### - [ ] 6.4 Docs and release notes

- `src/docs/reference/modules/Workflows/README.md`: the feature, what it shows, multi-node.
- Release notes.

### - [ ] 6.5 End-to-end test

- With the feature enabled: two pages on the same workflow; a change in one shows the notice in the other, and both show each other's presence. An instance page shows the instance finishing when its signal is triggered.

## Definition of done (Phase 6)

- [ ] Steps 6.1–6.5 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Without the feature, the designer behaves as before.
- [ ] Docs and release notes are updated.

## Open questions

- R4: should presence show what each person is editing (their selected activity)?
- Should the designer merge others' changes live instead of offering to reload?
