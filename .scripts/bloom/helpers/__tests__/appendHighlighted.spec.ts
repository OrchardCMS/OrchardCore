import { describe, expect, it } from "vitest";
import { appendHighlighted } from "../appendHighlighted";

describe("appendHighlighted", () => {
    it.each([
        ["Content Types", "types", "Types"],
        ["Café", "cafe", "Café"],
        ["A😀B", "😀", "😀"],
        ["Ae\u0301B", "e\u0301", "e\u0301"],
        ["A👩‍💻B", "👩‍💻", "👩‍💻"],
    ])("highlights an aligned whole-grapheme match in %s", (text, term, expected) => {
        const parent = document.createElement("span");
        appendHighlighted(parent, text, term);

        expect(parent.textContent).toBe(text);
        expect(parent.querySelector("mark")?.textContent).toBe(expected);
    });

    it.each([
        ["Æ😀 ", "😀"],
        ["Æmenu", "menu"],
        [" leading", "leading"],
        ["Ae\u0301B", "e"],
        ["A👩‍💻B", "👩"],
        ["A🇺🇸B", "🇺"],
        ["menu", ""],
        ["menu", "missing"],
    ])("leaves text intact when offsets or boundaries are ambiguous in %s", (text, term) => {
        const parent = document.createElement("span");
        appendHighlighted(parent, text, term);

        expect(parent.textContent).toBe(text);
        expect(parent.querySelector("mark")).toBeNull();
        expect(parent.childNodes).toHaveLength(1);
    });

    it("never interprets titles as HTML", () => {
        const parent = document.createElement("span");
        const text = "<img src=x onerror=alert(1)>";
        appendHighlighted(parent, text, "img");

        expect(parent.textContent).toBe(text);
        expect(parent.querySelector("img")).toBeNull();
        expect(parent.querySelector("mark")?.textContent).toBe("img");
    });
});
