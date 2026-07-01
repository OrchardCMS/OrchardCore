// monaco-editor ships monaco.d.ts as a pure ambient declaration file with no "types" entry in its
// package.json, so `import` (or `/// <reference types="monaco-editor" />`) can't resolve it - a
// path reference is the only way to bring the ambient `monaco` namespace into scope. `monaco` is a
// runtime global here too (loaded via Monaco's own AMD loader, not bundled), so there is nothing to
// import at the value level either.
// eslint-disable-next-line @typescript-eslint/triple-slash-reference
/// <reference path="../../../../../node_modules/monaco-editor/monaco.d.ts" />

const liquidTags = [
    "if",
    "else",
    "elseif",
    "endif",
    "render",
    "assign",
    "capture",
    "endcapture",
    "case",
    "endcase",
    "comment",
    "endcomment",
    "cycle",
    "decrement",
    "for",
    "endfor",
    "include",
    "increment",
    "layout",
    "raw",
    "endraw",
    "render",
    "tablerow",
    "endtablerow",
    "unless",
    "endunless",
];

const liquidFilters = [
    "abs",
    "append",
    "at_least",
    "at_most",
    "capitalize",
    "ceil",
    "compact",
    "date",
    "default",
    "divided_by",
    "downcase",
    "escape",
    "escape_once",
    "first",
    "floor",
    "join",
    "json",
    "last",
    "lstrip",
    "map",
    "minus",
    "modulo",
    "newline_to_br",
    "plus",
    "prepend",
    "remove",
    "remove_first",
    "replace",
    "replace_first",
    "reverse",
    "round",
    "rstrip",
    "size",
    "slice",
    "sort",
    "sort_natural",
    "split",
    "strip",
    "strip_html",
    "strip_newlines",
    "times",
    "truncate",
    "truncatewords",
    "uniq",
    "upcase",
    "url_decode",
    "url_encode",
    "where",
];

interface ILiquidContextInfo {
    showFilters: boolean;
    showTags: boolean;
    includeEndTags: boolean;
    inTag: boolean;
    inObject: boolean;
}

function getLiquidContextInfo(model: monaco.editor.ITextModel, position: monaco.Position): ILiquidContextInfo {
    let inTag: boolean = false;
    let inObject: boolean = false;
    let showTags: boolean = false;
    let showFilters: boolean = false;

    const findStart = model.findPreviousMatch("\\{(%|\\{)", position, true, false, null, true);
    if (findStart && findStart.matches && !position.isBefore(findStart.range.getEndPosition())) {
        if (findStart.matches[1] == "%") {
            inTag = true;
        } else if (findStart.matches[1] == "{") {
            inObject = true;
        }

        const searchPattern = inTag ? "%}" : "}}";
        const findEnd = model.findNextMatch(searchPattern, position, false, false, null, true);
        const currentRange = findEnd ? findStart.range.plusRange(findEnd.range) : findStart.range.setEndPosition(position.lineNumber, position.column);
        if (currentRange.containsPosition(position)) {
            if (inTag) {
                const findTagName = model.findNextMatch("\\{%\\s*([a-zA-Z-_]+)", findStart.range.getStartPosition(), true, false, null, true);
                if (findTagName && currentRange.containsRange(findTagName.range) && findTagName.matches && findTagName.matches.length > 1) {
                    if (findTagName.matches[1] == "assign") {
                        showFilters = true;
                    } else {
                        showTags = false;
                    }
                } else {
                    showTags = true;
                }
            } else {
                showFilters = true;
            }
        }
    }

    return {
        showFilters,
        showTags,
        inTag,
        inObject,
    } as ILiquidContextInfo;
}

const completionItemProvider: monaco.languages.CompletionItemProvider = {
    triggerCharacters: [" "],
    provideCompletionItems: (model: monaco.editor.ITextModel, position: monaco.Position, context: monaco.languages.CompletionContext) => {
        let items: string[] = [];

        if (context.triggerCharacter == " ") {
            const startTrigger = model.getValueInRange(new monaco.Range(position.lineNumber, position.column - 3, position.lineNumber, position.column - 1));
            if (startTrigger != "{%" && !startTrigger.endsWith("|")) {
                return null;
            }
        }

        const liquidContext: ILiquidContextInfo = getLiquidContextInfo(model, position);
        if (liquidContext.showFilters) {
            items = liquidFilters;
        } else if (liquidContext.showTags) {
            items = liquidTags.filter((value: string) => {
                return !value.startsWith("end");
            });
        }

        const suggestions = items.map((value: string) => {
            return {
                label: value,
                kind: monaco.languages.CompletionItemKind.Keyword,
                insertText: value,
                insertTextRules: monaco.languages.CompletionItemInsertTextRule.KeepWhitespace,
            } as monaco.languages.CompletionItem;
        });

        return { suggestions } as monaco.languages.ProviderResult<monaco.languages.CompletionList>;
    },
};

function ConfigureLiquidIntellisense(monaco: typeof globalThis.monaco, suggestHtml: boolean = true) {
    if (suggestHtml) {
        const modeConfiguration: monaco.languages.html.ModeConfiguration = {
            completionItems: true,
            colors: true,
            foldingRanges: true,
            selectionRanges: true,
            diagnostics: false,
            documentFormattingEdits: true,
            documentRangeFormattingEdits: true,
        };
        const options: monaco.languages.html.Options = {
            format: monaco.languages.html.htmlDefaults.options.format,
            suggest: { html5: true },
        };
        monaco.languages.html.registerHTMLLanguageService("liquid", options, modeConfiguration);
    }

    monaco.languages.registerCompletionItemProvider("liquid", completionItemProvider);
}

declare global {
    interface Window {
        ConfigureLiquidIntellisense: (monaco: typeof globalThis.monaco, suggestHtml?: boolean) => void;
        liquidFilters: string[];
        liquidTags: string[];
    }
}

window.ConfigureLiquidIntellisense = ConfigureLiquidIntellisense;
window.liquidFilters = liquidFilters;
window.liquidTags = liquidTags;

export { ConfigureLiquidIntellisense, liquidFilters };
