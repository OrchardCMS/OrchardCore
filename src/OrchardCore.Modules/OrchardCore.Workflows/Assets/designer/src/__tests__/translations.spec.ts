import { describe, expect, it } from "vitest";
import { readFileSync, readdirSync } from "node:fs";
import { join } from "node:path";

// Every client string goes through the 'workflows-designer' JS localizer. A key that isn't defined there is
// shown as is, so this checks the keys the sources use against Services/WorkflowsDesignerJSLocalizer.cs.
const sourceRoot = join(import.meta.dirname, "..");
const localizerPath = join(sourceRoot, "..", "..", "..", "Services", "WorkflowsDesignerJSLocalizer.cs");

const sourceFiles = (directory: string): string[] =>
    readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
        const path = join(directory, entry.name);

        if (entry.isDirectory()) {
            return entry.name === "__tests__" ? [] : sourceFiles(path);
        }

        return /\.(ts|vue)$/.test(entry.name) ? [path] : [];
    });

const usedKeys = () => {
    const keys = new Set<string>();

    for (const file of sourceFiles(sourceRoot)) {
        for (const match of readFileSync(file, "utf8").matchAll(/\bt\(\s*["']([A-Za-z0-9]+)["']/g)) {
            keys.add(match[1]);
        }
    }

    return keys;
};

const definedKeyList = () => Array.from(readFileSync(localizerPath, "utf8").matchAll(/\{\s*"([A-Za-z0-9]+)",\s*S\[/g), (match) => match[1]);

const definedKeys = () => new Set(definedKeyList());

describe("translations", () => {
    it("keys_UsedByTheDesigner_AreAllDefinedOnTheServer", () => {
        const defined = definedKeys();
        const used = usedKeys();

        expect(used.size).toBeGreaterThan(50);
        expect([...used].filter((key) => !defined.has(key))).toEqual([]);
    });

    // The localizer builds a dictionary, so a key defined twice breaks every designer page.
    it("keys_DefinedOnTheServer_AreDefinedOnce", () => {
        const keys = definedKeyList();

        expect(keys.filter((key, index) => keys.indexOf(key) !== index)).toEqual([]);
    });

    it("keys_DefinedOnTheServer_AreAllUsed", () => {
        const used = usedKeys();

        expect([...definedKeys()].filter((key) => !used.has(key))).toEqual([]);
    });
});
