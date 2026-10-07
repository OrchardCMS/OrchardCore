# Phase 4 — Per-input expression syntax

Any expression input of an activity can be Literal, Liquid or JavaScript, chosen per input, and modules can add syntaxes. This replaces the paired-property pattern (`Condition` / `LiquidCondition` plus `WorkflowScriptSyntax`). It turns the sketch in [`later-phases.md`](later-phases.md#phase-4--per-input-expression-syntax) into steps.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Target experience

```
If/Else task
  Condition   [ JavaScript ▾ ]  ┌──────────────────────────────┐
                                │ variable("attempts") < 3     │
                                └──────────────────────────────┘
              Literal · Liquid · JavaScript · (custom syntaxes)

Set Variable task
  Value       [ Liquid ▾ ]      ┌──────────────────────────────┐  ← Monaco, language follows the syntax
                                │ Hello {{ Workflow.Input.Name }}│
                                └──────────────────────────────┘
```

## Decisions

| # | Decision | Why |
|---|---|---|
| E1 | `WorkflowExpression<T>` gains `string Syntax`. `null` (every expression stored before this phase) means the activity decides, as it does today. Syntax names are strings (`Literal`, `Liquid`, `JavaScript`), not an enum. | Stored data deserializes unchanged, and modules can add syntaxes. |
| E2 | **Providers** implement `IWorkflowExpressionProvider` (`Name`, `DisplayName`, `EditorLanguage`, `EvaluateAsync<T>`, `Validate`). `IWorkflowExpressionManager` lists them and evaluates an expression with the provider of its syntax (or a default syntax). The built-in Liquid and JavaScript providers wrap `IWorkflowExpressionEvaluator` and `IWorkflowScriptEvaluator`, so their behavior is unchanged. | One evaluation path for every syntax. |
| E3 | **Literal** converts the text to the expected type: text as is; booleans and numbers in the invariant culture; JSON for objects and lists (a list can also be comma-separated text). | A literal value is the common case and needs no script. |
| E4 | **Compatibility**: a migrated activity keeps its legacy properties. When the new expression has no syntax, it evaluates the legacy pair chosen by the legacy `Syntax` property, exactly as before. Saving from the new editor writes the expression with its syntax and removes the legacy keys. There's no data migration. | Recipes, exports and stored workflows keep working; edited activities move to the new shape. |
| E5 | **Editor**: a reusable shape, `WorkflowExpressionEditor`, with a syntax select and either a one-line input or a Monaco editor whose language follows the syntax. It's safe to inject (Phase 1, step 1.3). The driver passes the allowed syntaxes (all registered providers by default), and validates the syntax and the expression on update. | Every activity edits expressions the same way; activity authors restrict what makes sense. |
| E6 | **Activities migrated**: If/Else, While Loop, For Each, For Loop, Set Output, Set Property, Set Variable and Correlate. Their constructors take `IWorkflowExpressionManager` instead of the two evaluators (release notes). Activities that use a single syntax today (Script, HTTP, Notify, Liquid…) are unchanged. | They are the ones with a syntax toggle. |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [x] 4.1 Syntax and providers

- `WorkflowExpression<T>.Syntax`; `WorkflowExpressionSyntaxes` constants (`Literal`, `Liquid`, `JavaScript`).
- `A/Services/IWorkflowExpressionProvider.cs`, `A/Services/IWorkflowExpressionManager.cs`; `M/Expressions/` providers and manager, registered in `M/Startup.cs`.
- **Tests**: each provider (Literal conversions, Liquid and JavaScript delegate to their evaluators), the manager (lookup ignoring case, default syntax, unknown syntax), JSON round trip with and without a syntax.
- **Notes from implementing this step:**
  - **Storage.** `Syntax` is left out of the stored JSON when it is null (`JOptions.Default` skips nulls), so activities stored before this phase, and their version fingerprints, don't change.
  - **Providers.**
    - Each provider has `EditorLanguage` (the Monaco language of the multi-line editor).
    - `Validate(text, valueType)` returns its errors:
      - Literal checks that the text converts to the type.
      - Liquid parses the template with `ILiquidTemplateManager`.
      - JavaScript checks nothing, since script errors are logged when the script runs.
    - `WorkflowExpressionEvaluationContext` carries the Liquid encoder and the extra JavaScript method providers.
  - **Literal.**
    - Converts text as is (including to `object`), booleans and numbers in the invariant culture, lists from a JSON array or comma-separated values, and anything else from JSON.
    - An empty text is the type's default.
    - A text that doesn't convert throws `FormatException`, which faults the workflow.
  - **Manager.**
    - The last registration of a name wins.
    - `List()` returns Literal, Liquid and JavaScript first, then the others by name.
    - `EvaluateAsync` uses the expression's syntax, or `defaultSyntax`. A syntax that isn't registered throws `NotSupportedException`.
  - **Legacy pairs.** `WorkflowExpressionSyntaxes.Resolve(expression, legacyLiquid, legacySyntax)` returns the expression to evaluate: the expression itself when it has a syntax, otherwise the legacy JavaScript or Liquid text with that syntax. Step 4.2 uses it.
  - **Tests.**
    - `Expressions/WorkflowExpressionProvidersTests.cs` (11): the providers, the manager, the JSON shape and `Resolve`.
    - Workflows tests: 200/200.

### - [x] 4.2 Activities evaluate per-input syntaxes

- The eight activities evaluate through `IWorkflowExpressionManager`, resolving the legacy pair when the expression has no syntax (E4). A helper resolves the legacy pair.
- **Tests**: for each activity, legacy JavaScript and Liquid properties evaluate as before; the new shape with each syntax, including Literal.
- **Notes from implementing this step:**
  - **Constructors.** The eight activities take `IWorkflowExpressionManager` instead of `IWorkflowScriptEvaluator` and `IWorkflowExpressionEvaluator`. Their legacy properties (`LiquidCondition`, `LiquidValue`, `LiquidEnumerable`, `LiquidFrom`/`To`/`Step` and `Syntax`) are documented as legacy and only read when an expression has no syntax.
  - **Correlate** used a single `Value` with either syntax, so its legacy pair is `Value` itself.
  - **For Each.** A Liquid list (legacy or new) is still evaluated as an object and read as a list, a JSON array or comma-separated text, as before. A JavaScript list that evaluates to `null` is now an empty list instead of a `NullReferenceException`.
  - **For Loop.** A legacy bound keeps its exact behavior: a Liquid text is parsed with `double.Parse`, and a JavaScript bound that is a number isn't run as a script. A bound with a syntax is evaluated by its provider.
  - **Tests.**
    - The legacy Liquid tests in `WorkflowManagerTests` (Set Output, Set Property, If/Else, While Loop, For Loop, For Each) pass unchanged, through `TestExpressions.CreateManager` over the same evaluators.
    - `Expressions/ExpressionActivitiesTests.cs` (13) covers the new shape (Literal for every activity, Liquid for If/Else and For Each), the precedence of a syntax over the legacy pair, legacy JavaScript, and legacy Liquid for Correlate.
    - Workflows tests: 215/215.

### - [ ] 4.3 Expression editor

- `WorkflowExpressionEditorViewModel` and the `WorkflowExpressionEditor` shape (`M/Views/WorkflowExpressionEditor.cshtml`), with the `workflow-expression-editor` script (`M/Assets/Scripts/workflow-expression-editor.ts`): syntax select, one-line input or Monaco, the Monaco language follows the syntax, values kept in form fields, disposed on `oc:editor-unmounting`.
- A driver helper that reads an editor's fields and validates them (allowed syntax, provider validation).
- **Tests**: the helper (allowed syntaxes, Liquid validation, unknown syntax).

### - [ ] 4.4 Migrate the activity editors

- The drivers, view models and views of the eight activities use the expression editor. Opening a legacy activity shows its legacy expression with its syntax; saving writes the new shape and removes the legacy keys.
- **Tests**: controller tests (a legacy If/Else opens with its Liquid condition; saving writes `Condition.Syntax` and removes `LiquidCondition` and `Syntax`; an invalid Liquid value shows its error; a disallowed syntax is rejected).

### - [ ] 4.5 Docs and release notes

- `src/docs/reference/modules/Workflows/README.md`: an **Expressions** section (the syntaxes, choosing one per input, Literal conversions, adding a provider, using the editor shape in a custom activity).
- Release notes: the feature, the constructor changes, the new stored shape.

### - [ ] 4.6 End-to-end test

- A test module (`test/OrchardCore.Tests.Modules/WorkflowsSample`) registers a custom syntax. The test opens an If/Else, sees the custom syntax in its select, and runs a seeded HTTP workflow whose Set Variable uses a Literal value and whose response uses a Liquid one.

## Definition of done (Phase 4)

- [ ] Steps 4.1–4.6 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Workflows stored before this phase run as before.
- [ ] Docs and release notes are updated.

## Open questions

Decided with the defaults above unless the maintainer says otherwise:

- E4: should a later major version migrate stored activities to the new shape and drop the legacy properties?
- E5: should one-line inputs use Monaco too? They don't, to keep the editor light; multi-line values do.
