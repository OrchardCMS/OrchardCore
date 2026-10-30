import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { loadTranslations } from "../../i18n";
import { javaScriptItems, liquidItems, registerVariableCompletions, startVariableCompletions, type CompletionProvider, type MonacoLike } from "../variableCompletions";
import { variableTypes, variables } from "./fixtures";

const createMonaco = () => {
    const providers = new Map<string, CompletionProvider>();
    const disposed: string[] = [];
    const monaco: MonacoLike = {
        languages: {
            CompletionItemKind: { Function: 1, Variable: 4 },
            CompletionItemInsertTextRule: { InsertAsSnippet: 4 },
            registerCompletionItemProvider: (language, provider) => {
                providers.set(language, provider);

                return { dispose: () => disposed.push(language) };
            },
        },
    };

    return { monaco, providers, disposed };
};

const model = (line: string) => ({ getLineContent: () => line });

const source = { variables, types: variableTypes };

describe("variableCompletions", () => {
    beforeEach(() => loadTranslations({ VariableCompletionDetail: "{0} variable" }));

    afterEach(() => {
        loadTranslations({});
        vi.useRealTimers();
        delete window.__orchardCoreMonacoReady;
    });

    it("items_AvailableValues_AreSuggestedInJavaScriptAndLiquid", () => {
        loadTranslations({ VariableCompletionDetail: "{0} variable", ValueCompletionDetail: "{0}, from {1}" });
        const { monaco } = createMonaco();
        const withValues = {
            ...source,
            values: [
                {
                    key: "Input:Owner",
                    name: "Owner",
                    source: "Input" as const,
                    typeName: "any",
                    description: "The owner.",
                    javaScript: 'input("Owner")',
                    liquid: "{{ Workflow.Input.Owner }}",
                    activityTitle: "Notify Owner",
                },
            ],
        };

        const javaScript = javaScriptItems(monaco, withValues, model("in"), { lineNumber: 1, column: 3 });
        const liquid = liquidItems(monaco, withValues, model("{{ Wor"), { lineNumber: 1, column: 7 });

        expect(javaScript[0]).toMatchObject({ label: 'input("Owner")', insertText: 'input("Owner")', documentation: "The owner." });
        expect(javaScript[0].detail).toContain("Notify Owner");
        expect(liquid[0]).toMatchObject({ label: "Workflow.Input.Owner", insertText: "Workflow.Input.Owner" });
    });

    it("javaScriptItems_TypedPrefix_ReplacesItWithVariableAndSetVariableCalls", () => {
        const { monaco } = createMonaco();

        const items = javaScriptItems(monaco, source, model("const x = var"), { lineNumber: 2, column: 14 });

        expect(items.map((item) => item.label)).toEqual(['variable("greeting")', 'setVariable("greeting", …)', 'variable("attempts")', 'setVariable("attempts", …)']);
        expect(items[0].range).toEqual({ startLineNumber: 2, endLineNumber: 2, startColumn: 11, endColumn: 14 });
        expect(items[1].insertText).toBe('setVariable("greeting", $0)');
        expect(items[1].insertTextRules).toBe(4);
        expect(items[0].detail).toBe("Text variable");
        expect(items[0].documentation).toBe("Shown to the user");
    });

    it("liquidItems_TypedMemberChain_ReplacesTheWholeChain", () => {
        const { monaco } = createMonaco();

        const items = liquidItems(monaco, source, model("{{ Workflow.Var"), { lineNumber: 1, column: 16 });

        expect(items.map((item) => item.insertText)).toEqual(["Workflow.Variables.greeting", "Workflow.Variables.attempts"]);
        expect(items[0].range.startColumn).toBe(4);
        expect(items[1].kind).toBe(4);
    });

    it("registerVariableCompletions_JavaScriptAndLiquid_ReadsTheCurrentVariablesAndDisposesBoth", () => {
        const { monaco, providers, disposed } = createMonaco();
        const current = { variables: [variables[0]], types: variableTypes };

        const registration = registerVariableCompletions(monaco, () => current);
        current.variables = variables;
        const suggestions = providers.get("liquid")!.provideCompletionItems(model(""), { lineNumber: 1, column: 1 }).suggestions;

        expect([...providers.keys()]).toEqual(["javascript", "liquid"]);
        expect(providers.get("liquid")!.triggerCharacters).toEqual(["."]);
        expect(suggestions).toHaveLength(2);

        registration.dispose();

        expect(disposed).toEqual(["javascript", "liquid"]);
    });

    it("startVariableCompletions_MonacoLoadedLater_RegistersOnceItIsReadyAndStops", async () => {
        vi.useFakeTimers();
        const { monaco, providers, disposed } = createMonaco();

        const stop = startVariableCompletions(() => source, 100);
        await vi.advanceTimersByTimeAsync(300);

        expect(providers.size).toBe(0);

        window.__orchardCoreMonacoReady = Promise.resolve(monaco);
        await vi.advanceTimersByTimeAsync(100);

        expect(providers.size).toBe(2);

        stop();

        expect(disposed).toEqual(["javascript", "liquid"]);
    });

    it("startVariableCompletions_StoppedBeforeMonacoLoads_RegistersNothing", async () => {
        vi.useFakeTimers();
        const { monaco, providers } = createMonaco();

        const stop = startVariableCompletions(() => source, 100);
        stop();
        window.__orchardCoreMonacoReady = Promise.resolve(monaco);
        await vi.advanceTimersByTimeAsync(500);

        expect(providers.size).toBe(0);
    });
});
