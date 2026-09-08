import { describe, expect, it } from "vitest";
import { translationsData } from "./mockdata";
import { getTranslations, setTranslations } from "@bloom/helpers/localizations";

const translations = getTranslations();

describe("translations", () => {
  it("should set and retrieve translations", () => {
    setTranslations(translationsData);
    expect(translations.NewFolder).toBe("New folder");
    expect(translations["NewFolder"]).toBe("New folder");
  });

  it("should return correct values for multiple translation keys", () => {
    setTranslations(translationsData);
    expect(translations.Ok).toBe("Ok");
    expect(translations.Cancel).toBe("Cancel");
    expect(translations.DeleteSingleFileMessage).toBe("Are you sure you want to delete this file?");
    expect(translations.DeleteMultipleFilesMessage).toBe("Are you sure you want to delete these files?");
    expect(translations.MoveFileMessage).toBe("Are you sure you want to move the selected file(s) to this folder?");
  });

  it("should overwrite translations when set again", () => {
    setTranslations({ NewFolder: "First" });
    expect(translations.NewFolder).toBe("First");

    setTranslations({ NewFolder: "Second" });
    expect(translations.NewFolder).toBe("Second");
  });

  it("should return undefined for missing keys", () => {
    setTranslations(translationsData);
    expect(translations["NonExistentKey"]).toBeUndefined();
  });
});
