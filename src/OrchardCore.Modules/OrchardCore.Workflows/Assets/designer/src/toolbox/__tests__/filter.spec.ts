import { describe, expect, it } from "vitest";
import { filterLibrary } from "../filter";
import { library } from "./library";

const names = (result: ReturnType<typeof filterLibrary>) => result.flatMap((category) => category.activities.map((activity) => activity.name));

describe("filterLibrary", () => {
    it("filterLibrary_NoQuery_ReturnsEveryActivity", () => {
        expect(names(filterLibrary(library, "", "all"))).toHaveLength(5);
        expect(filterLibrary(null, "", "all")).toEqual([]);
    });

    it("filterLibrary_DisplayText_IsCaseInsensitive", () => {
        expect(names(filterLibrary(library, "REDIRECT", "all"))).toEqual(["HttpRedirectTask"]);
    });

    it("filterLibrary_CategoryName_MatchesItsActivities", () => {
        expect(names(filterLibrary(library, "http", "all"))).toEqual(["HttpRequestEvent", "HttpRedirectTask"]);
    });

    it("filterLibrary_Accents_AreIgnored", () => {
        expect(names(filterLibrary(library, "publie", "all"))).toEqual(["ContentPublishedEvent"]);
        expect(names(filterLibrary(library, "créer", "all"))).toEqual(["CreateContentTask"]);
    });

    it("filterLibrary_SeveralTerms_MustAllMatch", () => {
        expect(names(filterLibrary(library, "http event", "all"))).toEqual(["HttpRequestEvent"]);
        expect(names(filterLibrary(library, "notify http", "all"))).toEqual([]);
    });

    it("filterLibrary_Kind_KeepsEventsOrTasks", () => {
        expect(names(filterLibrary(library, "", "events"))).toEqual(["ContentPublishedEvent", "HttpRequestEvent"]);
        expect(names(filterLibrary(library, "http", "tasks"))).toEqual(["HttpRedirectTask"]);
    });

    it("filterLibrary_NoMatch_DropsEmptyCategories", () => {
        expect(filterLibrary(library, "notify", "all").map((category) => category.name)).toEqual(["Primitives"]);
    });
});
