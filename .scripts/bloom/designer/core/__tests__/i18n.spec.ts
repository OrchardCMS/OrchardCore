import { afterEach, describe, expect, it } from "vitest";
import { format, hasText, loadTranslations, setDefaultTexts, t } from "../i18n";

describe("i18n", () => {
    afterEach(() => {
        loadTranslations({});
        setDefaultTexts({});
    });

    it("t_NoTranslation_ReturnsTheFallback", () => {
        expect(t("Close", "Close")).toBe("Close");
    });

    it("t_Translation_WinsOverTheDesignerTextAndTheFallback", () => {
        setDefaultTexts({ Close: "Done" });
        loadTranslations({ Close: "Fermer" });

        expect(t("Close", "Close")).toBe("Fermer");
    });

    it("t_DesignerText_WinsOverTheFallback", () => {
        setDefaultTexts({ ConflictTitle: "The pipeline was changed elsewhere" });

        expect(t("ConflictTitle", "The draft was changed elsewhere")).toBe("The pipeline was changed elsewhere");
        expect(hasText("ConflictTitle")).toBe(true);
        expect(hasText("Other")).toBe(false);
    });

    it("t_Arguments_ReplacesThePlaceholders", () => {
        loadTranslations({ VersionNumber: "Version {0} of {1}" });

        expect(t("VersionNumber", "Version {0}", 3, 5)).toBe("Version 3 of 5");
    });

    it("format_MissingArgument_KeepsThePlaceholder", () => {
        expect(format("{0} and {1}", ["a"])).toBe("a and {1}");
    });
});
