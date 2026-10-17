import { afterEach, describe, expect, it } from "vitest";
import { loadTranslations, t } from "../i18n";

// Every source file of the designer (not the tests), as text.
const sources = import.meta.glob(["../**/*.ts", "../**/*.vue", "!../**/__tests__/**"], { query: "?raw", import: "default", eager: true }) as Record<string, string>;

// t("Key", "English") and t('Key', 'English') calls.
const CALL = /\bt\(\s*(["'])([A-Za-z0-9]+)\1\s*,\s*(["'])((?:\\.|(?!\3).)*)\3/g;

const calls = () =>
    Object.entries(sources).flatMap(([file, text]) => [...text.matchAll(CALL)].map((match) => ({ file, key: match[2], fallback: match[4] })));

describe("translations", () => {
    afterEach(() => loadTranslations({}));

    it("t_EmptyDictionary_UsesTheEnglishFallbackWithItsArguments", () => {
        loadTranslations({});

        expect(t("RowCount", "{0} rows", "1,234")).toBe("1,234 rows");
    });

    it("t_Translated_UsesTheDictionary", () => {
        loadTranslations({ RowCount: "{0} lignes" });

        expect(t("RowCount", "{0} rows", 5)).toBe("5 lignes");
    });

    it("sources_EveryKey_HasOneEnglishText", () => {
        const found = calls();
        const byKey = new Map<string, Set<string>>();

        for (const call of found) {
            byKey.set(call.key, (byKey.get(call.key) ?? new Set()).add(call.fallback));
        }

        const conflicting = [...byKey].filter(([, fallbacks]) => fallbacks.size > 1).map(([key, fallbacks]) => `${key}: ${[...fallbacks].join(" | ")}`);

        expect(found.length).toBeGreaterThan(100);
        expect(conflicting).toEqual([]);
    });
});
