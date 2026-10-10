import { describe, expect, it } from "vitest";
import { CATEGORY_ICONS, compatibleSteps, filterLibrary, sourceSteps, stepIcon } from "../filter";
import { library } from "../../__tests__/fixtures";

const names = (categories: ReturnType<typeof filterLibrary>) => categories.map((category) => [category.category, category.steps.map((step) => step.name)]);

describe("toolbox filter", () => {
    it("filterLibrary_NoQuery_GroupsInSourceTransformFileDestinationOrder", () => {
        expect(filterLibrary(library, "").map((category) => category.category)).toEqual(["Source", "Transform", "File", "Destination"]);
        expect(filterLibrary(null, "")).toEqual([]);
    });

    it("filterLibrary_Query_MatchesNameDescriptionOrCategoryIgnoringCaseAndAccents", () => {
        expect(names(filterLibrary(library, "UTILISATEURS"))).toEqual([["Source", ["UsersSource"]]]);
        // The description: "Les comptes".
        expect(names(filterLibrary(library, "comptés"))).toEqual([["Source", ["UsersSource"]]]);
        // The category's display name.
        expect(names(filterLibrary(library, "destinations table"))).toEqual([["Destination", ["TableDestination"]]]);
        expect(filterLibrary(library, "nothing like this")).toEqual([]);
    });

    it("compatibleSteps_RecordsOutput_OffersStepsWhoseFirstInputTakesRows", () => {
        expect(compatibleSteps(library, "Records", "").map((item) => item.step.name)).toEqual(["FilterStep", "JoinStep", "CsvFile", "TableDestination"]);
    });

    it("compatibleSteps_FilesOutput_OffersStepsWhoseFirstInputTakesFiles", () => {
        expect(compatibleSteps(library, "Files", "").map((item) => item.step.name)).toEqual(["ZipFiles", "EmailDestination"]);
        expect(compatibleSteps(library, "Files", "zip").map((item) => [item.step.name, item.category.displayName])).toEqual([["ZipFiles", "Files"]]);
    });

    it("sourceSteps_Library_ReturnsTheSources", () => {
        expect(sourceSteps(library).map((step) => step.name)).toEqual(["ContentItemsSource", "UsersSource"]);
    });

    it("stepIcon_NoIcon_UsesTheCategoryIcon", () => {
        expect(stepIcon({ icon: "fa-solid fa-user", category: "Source" })).toBe("fa-solid fa-user");
        expect(stepIcon({ icon: null, category: "File" })).toBe(CATEGORY_ICONS.File);
    });
});
