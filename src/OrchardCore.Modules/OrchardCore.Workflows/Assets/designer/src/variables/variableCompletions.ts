import type { VariableDefinition, VariableType } from "../api/types";
import { typeDisplayName } from "./variableValues";
import { t } from "../i18n";

// The parts of the Monaco API the completions use. Monaco is loaded by the OrchardCore.Resources "monaco"
// resource, once a script editor needs it, and exposed as window.__orchardCoreMonacoReady.

export interface CompletionPosition {
    lineNumber: number;
    column: number;
}

export interface CompletionRange {
    startLineNumber: number;
    endLineNumber: number;
    startColumn: number;
    endColumn: number;
}

export interface CompletionModel {
    getLineContent(lineNumber: number): string;
}

export interface CompletionItem {
    label: string;
    kind: number;
    detail: string;
    documentation?: string;
    insertText: string;
    insertTextRules?: number;
    filterText?: string;
    range: CompletionRange;
}

export interface CompletionProvider {
    triggerCharacters?: string[];
    provideCompletionItems(model: CompletionModel, position: CompletionPosition): { suggestions: CompletionItem[] };
}

export interface MonacoLike {
    languages: {
        CompletionItemKind: { Function: number; Variable: number };
        CompletionItemInsertTextRule: { InsertAsSnippet: number };
        registerCompletionItemProvider(languageId: string, provider: CompletionProvider): { dispose(): void };
    };
}

export interface CompletionSource {
    variables: VariableDefinition[];
    types: VariableType[];
}

declare global {
    interface Window {
        __orchardCoreMonacoReady?: Promise<unknown>;
    }
}

// The completion replaces what was typed of it: the identifier (JavaScript) or member chain (Liquid) before
// the cursor.
const typedRange = (model: CompletionModel, position: CompletionPosition, pattern: RegExp): CompletionRange => {
    const before = model.getLineContent(position.lineNumber).slice(0, position.column - 1);
    const typed = pattern.exec(before)?.[0] ?? "";

    return {
        startLineNumber: position.lineNumber,
        endLineNumber: position.lineNumber,
        startColumn: position.column - typed.length,
        endColumn: position.column,
    };
};

const detailOf = (source: CompletionSource, variable: VariableDefinition) => t("VariableCompletionDetail", typeDisplayName(source.types, variable.typeName));

/**
 * The JavaScript completions: `variable("name")` and `setVariable("name", …)` for each declared variable.
 */
export const javaScriptItems = (monaco: MonacoLike, source: CompletionSource, model: CompletionModel, position: CompletionPosition): CompletionItem[] => {
    const range = typedRange(model, position, /[\w$]*$/);
    const { Function } = monaco.languages.CompletionItemKind;

    return source.variables.flatMap((variable) => {
        const name = JSON.stringify(variable.name);
        const documentation = variable.description || undefined;

        return [
            { label: `variable(${name})`, kind: Function, detail: detailOf(source, variable), documentation, insertText: `variable(${name})`, range },
            {
                label: `setVariable(${name}, …)`,
                kind: Function,
                detail: detailOf(source, variable),
                documentation,
                insertText: `setVariable(${name}, $0)`,
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                filterText: `setVariable(${name})`,
                range,
            },
        ];
    });
};

/**
 * The Liquid completions: `Workflow.Variables.name` for each declared variable.
 */
export const liquidItems = (monaco: MonacoLike, source: CompletionSource, model: CompletionModel, position: CompletionPosition): CompletionItem[] => {
    const range = typedRange(model, position, /[\w.]*$/);

    return source.variables.map((variable) => ({
        label: `Workflow.Variables.${variable.name}`,
        kind: monaco.languages.CompletionItemKind.Variable,
        detail: detailOf(source, variable),
        documentation: variable.description || undefined,
        insertText: `Workflow.Variables.${variable.name}`,
        range,
    }));
};

/**
 * Registers the variable completions of the JavaScript and Liquid editors; `getSource` is read on every
 * completion, so the list follows the declarations.
 */
export const registerVariableCompletions = (monaco: MonacoLike, getSource: () => CompletionSource) => {
    const registrations = [
        monaco.languages.registerCompletionItemProvider("javascript", {
            provideCompletionItems: (model, position) => ({ suggestions: javaScriptItems(monaco, getSource(), model, position) }),
        }),
        monaco.languages.registerCompletionItemProvider("liquid", {
            triggerCharacters: ["."],
            provideCompletionItems: (model, position) => ({ suggestions: liquidItems(monaco, getSource(), model, position) }),
        }),
    ];

    return {
        dispose: () => registrations.forEach((registration) => registration.dispose()),
    };
};

/**
 * Registers the completions as soon as Monaco is on the page (the first script editor loads it), and returns
 * a function that stops waiting and removes them.
 */
export const startVariableCompletions = (getSource: () => CompletionSource, interval = 1000) => {
    let registration: { dispose(): void } | null = null;
    let stopped = false;
    let timer: ReturnType<typeof setInterval> | null = null;

    const attach = (ready: Promise<unknown>) => {
        void ready.then(
            (monaco) => {
                if (!stopped) {
                    registration = registerVariableCompletions(monaco as MonacoLike, getSource);
                }
            },
            () => undefined,
        );
    };

    if (window.__orchardCoreMonacoReady) {
        attach(window.__orchardCoreMonacoReady);
    } else {
        timer = setInterval(() => {
            if (window.__orchardCoreMonacoReady) {
                clearInterval(timer!);
                timer = null;
                attach(window.__orchardCoreMonacoReady);
            }
        }, interval);
    }

    return () => {
        stopped = true;

        if (timer !== null) {
            clearInterval(timer);
        }

        registration?.dispose();
    };
};
