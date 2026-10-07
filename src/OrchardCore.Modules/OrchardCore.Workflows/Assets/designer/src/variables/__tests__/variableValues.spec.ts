import { describe, expect, it } from "vitest";
import { editorOf, findVariable, formatDefault, formatValue, isAssignable, parseDefault, typeDisplayName } from "../variableValues";
import { variableTypes } from "./fixtures";

describe("variableValues", () => {
    it("editorOf_KnownAndUnknownTypes_ReturnsTheirEditorOrText", () => {
        expect(editorOf(variableTypes, "Number")).toBe("number");
        expect(editorOf(variableTypes, "contentItem")).toBe("none");
        expect(editorOf(variableTypes, "missing")).toBe("text");
        expect(typeDisplayName(variableTypes, "boolean")).toBe("Boolean");
        expect(typeDisplayName(variableTypes, "missing")).toBe("missing");
    });

    it.each([
        ["text", "hello", { valid: true, value: "hello" }],
        ["text", "", { valid: true, value: null }],
        ["number", "4.5", { valid: true, value: 4.5 }],
        ["number", "many", { valid: false }],
        ["boolean", "true", { valid: true, value: true }],
        ["boolean", "yes", { valid: false }],
        ["json", '{"a":[1]}', { valid: true, value: { a: [1] } }],
        ["json", "{a", { valid: false }],
        ["datetime", "2026-10-07T10:30", { valid: true, value: "2026-10-07T10:30" }],
        ["datetime", "not a date", { valid: false }],
        ["none", "ignored", { valid: true, value: null }],
    ] as const)("parseDefault_%s_%s", (editor, text, expected) => {
        expect(parseDefault(text, editor)).toEqual(expected);
    });

    it("formatDefault_Values_ReturnTheTextOfTheirInput", () => {
        expect(formatDefault(null, "text")).toBe("");
        expect(formatDefault(42, "number")).toBe("42");
        expect(formatDefault(false, "boolean")).toBe("false");
        expect(formatDefault({ a: 1 }, "json")).toBe('{\n  "a": 1\n}');
        expect(formatDefault("2026-10-07T10:30:00.000Z", "datetime")).toBe("2026-10-07T10:30");
        expect(formatDefault("anything", "none")).toBe("");
    });

    it.each([
        ["number", "number", true],
        ["number", "string", true],
        ["any", "boolean", true],
        ["object", "any", true],
        ["number", "boolean", false],
        ["string", "number", false],
    ])("isAssignable_%s_To_%s_Is_%s", (output, variable, expected) => {
        expect(isAssignable(output, variable)).toBe(expected);
    });

    it("findVariable_DifferentCase_FindsTheDeclaration", () => {
        expect(findVariable([{ name: "greeting", typeName: "string" }], "Greeting")?.name).toBe("greeting");
        expect(formatValue({ a: 1 })).toBe('{"a":1}');
        expect(formatValue(3)).toBe("3");
    });
});
