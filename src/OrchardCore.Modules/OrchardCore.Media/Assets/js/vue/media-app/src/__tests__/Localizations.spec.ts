import { describe, expect, it } from "vitest";
import { translationsData } from "./mockdata";
import { useLocalizations } from "../services/Localizations";

const { translations, setTranslations } = useLocalizations();

describe("translations", () => {
  it("should set and retrieve translations", () => {
    setTranslations(translationsData);
    const t = translations.value;
    expect(t.NewFolder).toBe("New folder");
    expect(t["NewFolder"]).toBe("New folder");
  });

  it("should return correct values for multiple translation keys", () => {
    setTranslations(translationsData);
    const t = translations.value;
    expect(t.Ok).toBe("Ok");
    expect(t.Cancel).toBe("Cancel");
    expect(t.DeleteSingleFileMessage).toBe("Are you sure you want to delete this file?");
    expect(t.DeleteMultipleFilesMessage).toBe("Are you sure you want to delete these files?");
    expect(t.MoveFileMessage).toBe("Are you sure you want to move the selected file(s) to this folder?");
  });

  it("should overwrite translations when set again", () => {
    setTranslations({ NewFolder: "First" });
    expect(translations.value.NewFolder).toBe("First");

    setTranslations({ NewFolder: "Second" });
    expect(translations.value.NewFolder).toBe("Second");
  });

  it("should return undefined for missing keys", () => {
    setTranslations(translationsData);
    expect(translations.value["NonExistentKey"]).toBeUndefined();
  });
});
