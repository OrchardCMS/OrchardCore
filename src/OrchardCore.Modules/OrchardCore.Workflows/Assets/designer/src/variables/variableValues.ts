import type { VariableDefinition, VariableEditor, VariableType } from "../api/types";

/**
 * The editor of a type, or `text` for a type the designer doesn't know (its feature was disabled).
 */
export const editorOf = (types: VariableType[], typeName: string): VariableEditor => {
    const editor = types.find((type) => type.name.toLowerCase() === typeName.toLowerCase())?.editor;

    return (["text", "number", "boolean", "datetime", "json", "none"] as const).find((known) => known === editor) ?? "text";
};

/**
 * The display name of a type, or its name when the type isn't available.
 */
export const typeDisplayName = (types: VariableType[], typeName: string) =>
    types.find((type) => type.name.toLowerCase() === typeName.toLowerCase())?.displayName ?? typeName;

// A datetime-local input shows 'YYYY-MM-DDTHH:mm'; the server reads text without an offset as UTC.
const toDateTimeLocal = (value: string) => value.replace(/(\.\d+)?(Z|[+-]\d{2}:\d{2})$/, "").slice(0, 16);

/**
 * The text of a default value in the input of `editor`; an empty text is no default.
 */
export const formatDefault = (value: unknown, editor: VariableEditor): string => {
    if (value === null || value === undefined || editor === "none") {
        return "";
    }

    if (editor === "json") {
        return JSON.stringify(value, null, 2);
    }

    if (editor === "datetime" && typeof value === "string") {
        return toDateTimeLocal(value);
    }

    return typeof value === "object" ? JSON.stringify(value) : String(value);
};

export type ParsedDefault = { valid: true; value: unknown } | { valid: false };

/**
 * Reads the text of a default value; an empty text is no default (`null`).
 */
export const parseDefault = (text: string, editor: VariableEditor): ParsedDefault => {
    const trimmed = text.trim();

    if (trimmed === "" || editor === "none") {
        return { valid: true, value: null };
    }

    switch (editor) {
        case "number": {
            const value = Number(trimmed);

            return Number.isFinite(value) ? { valid: true, value } : { valid: false };
        }

        case "boolean":
            return trimmed === "true" || trimmed === "false" ? { valid: true, value: trimmed === "true" } : { valid: false };

        case "json":
            try {
                return { valid: true, value: JSON.parse(trimmed) };
            } catch {
                return { valid: false };
            }

        case "datetime":
            return Number.isNaN(Date.parse(trimmed)) ? { valid: false } : { valid: true, value: trimmed };

        default:
            return { valid: true, value: text };
    }
};

/**
 * Whether values of an output type always convert to a variable type, as the server's OutputTypeMismatch
 * check decides: the same type, an `any` side, or a `string` variable.
 */
export const isAssignable = (outputType: string, variableType: string) => {
    const output = outputType.toLowerCase();
    const variable = variableType.toLowerCase();

    return output === variable || output === "any" || variable === "any" || variable === "string";
};

/**
 * The declared variable named `name`, ignoring case.
 */
export const findVariable = (variables: VariableDefinition[], name: string) => variables.find((variable) => variable.name.toLowerCase() === name.toLowerCase());

/**
 * A stored value as text, for the read-only views.
 */
export const formatValue = (value: unknown) => (value !== null && typeof value === "object" ? JSON.stringify(value) : String(value));
