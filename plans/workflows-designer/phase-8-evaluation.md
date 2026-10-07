# Phase 8 — Evaluate: branching model, composition, state machines

These change the execution model, so this phase spikes them and decides, rather than building all of them. This turns the sketch in [`later-phases.md`](later-phases.md#phase-8--evaluate-branching-model-composition-state-machines) into steps.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Decisions

| # | Decision | Why |
|---|---|---|
| B1 | **Multiple transitions per outcome** ship behind a per-workflow setting, `BranchingMode` (`FirstOnly`, the default, or `All`). With `All`, the engine follows every transition of an outcome, and the designer lets an outcome connect to several activities. `FirstOnly` keeps today's behavior, so stored workflows don't change. | It's a small, contained engine change, and an implicit fork is what users try first. The setting keeps it opt-in. |
| B2 | **Composition** (containers, nested sequences and flowcharts) isn't built. The evaluation (below) records what it needs and why it waits. | It needs a new execution model (a tree of execution contexts and a scheduler). Phase 7's Execute Workflow task covers reuse, the main need. |
| B3 | **State machines** aren't built. The evaluation shows how to model one with today's activities. | They need composition first, and a modeling pattern covers the common approval lifecycles. |
| B4 | **BPMN** stays out of scope. | As the sketch says. |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [x] 8.1 Branching mode

- `WorkflowType.BranchingMode` (`WorkflowBranchingMode`: `FirstOnly`, `All`, stored as text), in the draft settings, the settings forms, versions, the diff and recipes. The engine follows every transition of an outcome with `All`. The validation's duplicate-outcome warning only applies to `FirstOnly`.
- The designer: with `All`, connecting an outcome adds a connection instead of replacing the existing one.
- **Tests**: the engine with both modes (two activities connected to one outcome); the validation; the settings; Vitest for connecting.
- **Notes from implementing this step:**
  - **Setting.**
    - `WorkflowBranchingMode` (`FirstOnly`, the default, and `All`) is serialized as text, so recipes can say `"BranchingMode": "All"` and the designer gets `"All"`.
    - `WorkflowType.BranchingMode` follows `IsActivity` through every copy: the draft, the settings forms (designer, properties, duplicate), versions, their fingerprint, the diff and the recipe step.
    - The forms show it as **Outcomes with several transitions**: **Follow the first transition only** or **Follow every transition**.
  - **Engine.**
    - `ExecuteWorkflowAsync` takes the transitions of each outcome: the first one in `FirstOnly`, all of them in `All`.
    - It pushes them in reverse, so they run in the order they were added.
    - A transition to a missing activity still fails as before, which a test of the recursion counters relies on.
  - **Validation.** The `DuplicateOutcomeTransition` warning only applies to `FirstOnly`.
  - **Designer.** `connectCommand` takes `keepOthers`: in `All` mode, `connectOutcome` adds a transition instead of replacing the outcome's transition. Undo removes only the new one.
  - **Tests.**
    - The engine follows one or both transitions by mode, in order.
    - No warning in `All` mode.
    - The setting is published from the draft, is listed by the diff, creates a new version, and is shown and stored by the designer's settings form.
    - Vitest: connecting in `All` mode adds an edge, and undo removes only it.
    - Workflows tests: 269/269. Vitest: 236/236.

### - [x] 8.2 Evaluation of composition and state machines

- This file's **Evaluation** section: what composition and state machines need in the engine, the designer and storage, the estimates, the decision, and the modeling patterns available today.
- **Notes from implementing this step:**
  - **Evaluation.** The section below covers each decision:
    - the branching mode as built (B1);
    - what composition would need in the engine, storage and designer, with an estimate of several weeks (B2);
    - the state-machine pattern available today (B3);
    - why BPMN stays out (B4).
  - **Join.** `JoinTask` collects the transitions that lead into it, so a Join after implicit branches behaves as after a Fork.
  - **Tracker.** The table of `later-phases.md` records the branching mode as done, and composition and state machines as evaluated.

### - [x] 8.3 Docs and release notes

- `src/docs/reference/modules/Workflows/README.md`: the branching mode, and the state-machine pattern.
- Release notes.
- **Notes from implementing this step:**
  - **Reference.** A **Branching** section in the Workflows module's page explains:
    - Fork and Join, and the **Outcomes with several transitions** setting;
    - what following every transition does in the designer and the engine;
    - that the default keeps the old behavior.
    - Its **Modeling a State Machine** subsection has the pattern from the evaluation, with the Script task's **Available Outcomes** and a `WaitAny` Join.
  - **Release notes.** A **Workflow Branching** section. There's no breaking change.

### - [ ] 8.4 End-to-end test

- A seeded workflow in `All` mode connects one outcome to two activities; both run (the instance page shows both executed), and connecting the outcome to a third activity in the designer keeps the other transitions.

## Definition of done (Phase 8)

- [ ] Steps 8.1–8.4 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Workflows in `FirstOnly` mode behave as before.
- [ ] Docs and release notes are updated.

## Evaluation

### Multiple transitions per outcome (B1, built in 8.1)

- **Engine.** `ExecuteWorkflowAsync` pushes the destinations of an outcome on its stack of scheduled activities. It pushed the first transition's destination only; with `All`, it pushes every one, in reverse, so they run in the order they were added. The change is a loop, and `FirstOnly` keeps the old path.
- **Behavior.** The branches run one after the other in the same run, as after a Fork: there's no parallelism in either case. A branch that waits on an event makes the instance wait on it while the other branches go on. A Join after the branches works as after a Fork, since it counts the transitions that lead into it, whatever started the branches.
- **Designer and validation.** With `All`, connecting an outcome adds a transition instead of replacing the existing one, and the "only the first transition is followed" warning goes away.
- **Cost and risk.** About a day; low risk, since the mode is opt-in and stored workflows keep `FirstOnly`.

### Composition: containers, nested sequences and flowcharts (B2, not built)

What it would need:

- **Engine.**
    - A run is a flat loop over one graph today, and an instance's state is one dictionary of activity states.
    - Containers (a Sequence or Flowchart activity that holds activities) need a tree of execution scopes. Each container schedules its children and completes when they complete, and owns its scoped variables.
    - An activity that waits must record the path of its scope, so the instance resumes inside the container. The journal records the scope too.
- **Storage.**
    - `ActivityRecord` would hold its children, or a parent id, and `WorkflowState` would keep the state of each scope.
    - Versions, the diff, recipes and deployment steps would carry the nested graphs.
    - Flat workflows would stay a single root scope, so they keep working.
- **Designer.** Nested canvases (opening a container, with breadcrumbs back to its parents), moving activities between scopes, and validation per scope.
- **Estimate.** Several weeks: 2–3 for the engine, 2–3 for the designer, and 1–2 for compatibility and tests.

**Decision: not now.** Phase 7's Execute Workflow task covers reuse, and Phase 1's collapsible branches cover the size of large workflows. Revisit composition when there's demand for scoped variables or nested structure.

### State machines (B3, not built)

- **Elsa's model.** A state machine activity has states, each with activities run on entry and exit, and transitions triggered by events. Each state is a container, so it needs composition first.
- **Modeling one today.** A typed `state` variable (Phase 3) and a loop:
    1. A Set Variable sets `state` to the first state, for example `Draft`.
    2. A Script task with an outcome per state (`setOutcome(variable('state'))`) branches to the state.
    3. Each state's branch waits for the events that leave it: a Signal, a content or user event, or a timer. When several events can fire, it uses a Fork into the events and a Join that waits for any of them. The branch then sets `state` to the next state, and goes back to step 2.
    4. A final state ends the loop.
- **What Phases 5 and 7 add.** The journal shows the states an instance went through (Phase 5), and the work of each state can be a workflow of its own, run with Execute Workflow (Phase 7).

**Decision: document the pattern** (step 8.3), and build state machines only after composition.

### BPMN (B4)

Out of scope, as the sketch says: importing and exporting BPMN needs a mapping of its elements to activities, and an engine that runs its semantics (tokens, gateways, events), which isn't the goal of this plan.
