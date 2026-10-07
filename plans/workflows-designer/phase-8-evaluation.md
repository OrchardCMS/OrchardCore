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

### - [ ] 8.2 Evaluation of composition and state machines

- This file's **Evaluation** section: what composition and state machines need in the engine, the designer and storage, the estimates, the decision, and the modeling patterns available today.

### - [ ] 8.3 Docs and release notes

- `src/docs/reference/modules/Workflows/README.md`: the branching mode, and the state-machine pattern.
- Release notes.

### - [ ] 8.4 End-to-end test

- A seeded workflow in `All` mode connects one outcome to two activities; both run (the instance page shows both executed), and connecting the outcome to a third activity in the designer keeps the other transitions.

## Definition of done (Phase 8)

- [ ] Steps 8.1–8.4 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Workflows in `FirstOnly` mode behave as before.
- [ ] Docs and release notes are updated.

## Evaluation

Written in step 8.2.
