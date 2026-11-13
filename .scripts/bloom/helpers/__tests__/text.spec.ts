import { describe, expect, it } from "vitest";
import { firstTextElement, truncateText } from "../text";

const cases: [string, number, string][] = [
    ["", 0, ""],
    ["abc", 0, ""],
    ["abc", 2, "ab"],
    ["abc", 3, "abc"],
    ["abc", 4, "abc"],
    ["a😀b", 2, "a"],
    ["a😀b", 3, "a😀"],
    ["ae\u0301b", 2, "a"],
    ["ae\u0301b", 3, "ae\u0301"],
    ["a🇺🇸b", 3, "a"],
    ["a🇺🇸b", 5, "a🇺🇸"],
    ["a👍🏽b", 3, "a"],
    ["a👩‍💻b", 5, "a"],
    ["a👩‍💻b", 6, "a👩‍💻"],
    ["a\r\nb", 2, "a"],
];

describe("truncateText", () => {
    it.each(cases)("preserves whole elements in %s with limit %i", (value, limit, expected) => {
        expect(truncateText(value, limit)).toBe(expected);
    });

    it("rejects negative limits", () => {
        expect(() => truncateText("text", -1)).toThrow(RangeError);
    });
});

describe("firstTextElement", () => {
    it.each(["😀", "e\u0301", "🇺🇸", "👩‍💻", "👍🏽"])("keeps %s intact", (element) => {
        expect(firstTextElement(element + "rest")).toBe(element);
    });

    it("returns empty for empty text", () => {
        expect(firstTextElement("")).toBe("");
    });
});
