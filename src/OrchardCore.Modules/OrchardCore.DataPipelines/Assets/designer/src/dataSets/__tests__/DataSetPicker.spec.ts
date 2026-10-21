import { afterEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import DataSetPicker from "../DataSetPicker.vue";
import type { DataSetsResult } from "../../api/types";

const result: DataSetsResult = {
    dataSets: [
        { value: "Contents::BlogPost", displayName: "Blog Post", description: "The posts of the blog.", source: "Content items", group: "Content" },
        { value: "Contents::Article", displayName: "Article", source: "Content items" },
        { value: "Users::Users", displayName: "Users", description: "One row per user.", source: "Users" },
    ],
};

// The dialog is rendered at the end of <body>.
const query = <T extends Element = HTMLElement>(selector: string) => document.querySelector<T>(selector);
const queryAll = (selector: string) => Array.from(document.querySelectorAll<HTMLElement>(selector));

const cards = () => queryAll("[data-cy^='data-set-'][data-cy*='::']").map((card) => card.dataset.cy!.replace("data-set-", ""));
const counts = () => queryAll("[data-cy^=data-set-category-] .badge").map((badge) => badge.textContent?.trim());

const mountPicker = async (props: Partial<InstanceType<typeof DataSetPicker>["$props"]> = {}) => {
    const wrapper = mount(DataSetPicker, { props: { load: vi.fn().mockResolvedValue(result), ...props }, attachTo: document.body });
    await flushPromises();

    return wrapper;
};

const typeFilter = async (value: string) => {
    const input = query<HTMLInputElement>("[data-cy=data-set-filter]")!;
    input.value = value;
    input.dispatchEvent(new Event("input"));
    await flushPromises();
};

describe("DataSetPicker", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("mount_DataSets_ShowsEachWithItsDescriptionAndCountsThemBySource", async () => {
        await mountPicker();

        expect(cards()).toEqual(["Contents::BlogPost", "Contents::Article", "Users::Users"]);
        expect(counts()).toEqual(["3", "2", "1"]);
        expect(query("[data-cy='data-set-Contents::BlogPost']")?.textContent).toContain("The posts of the blog.");
        expect(document.activeElement).toBe(query("[data-cy=data-set-filter]"));
    });

    it("category_Click_ShowsTheDataSetsOfItsSource", async () => {
        await mountPicker();

        query<HTMLButtonElement>("[data-cy='data-set-category-Users']")!.click();
        await flushPromises();

        expect(cards()).toEqual(["Users::Users"]);
        expect(query("[data-cy='data-set-category-Users']")?.getAttribute("aria-pressed")).toBe("true");
    });

    it("filter_Words_MatchTheNameTheDescriptionAndTheGroup", async () => {
        await mountPicker();

        await typeFilter("blog posts");
        expect(cards()).toEqual(["Contents::BlogPost"]);
        expect(counts()).toEqual(["1", "1", "0"]);

        await typeFilter("nothing");
        expect(cards()).toEqual([]);
        expect(query("[data-cy=data-set-empty]")?.textContent).toContain("No data set matches the filter.");
    });

    it("choose_Click_EmitsTheDataSet", async () => {
        const wrapper = await mountPicker();

        query<HTMLButtonElement>("[data-cy='data-set-Users::Users'] [data-cy=data-set-choose]")!.click();

        expect(wrapper.emitted("pick")?.[0]).toEqual([result.dataSets[2]]);
    });

    it("current_DataSet_IsMarkedAndCanNotBeChosenAgain", async () => {
        await mountPicker({ current: "Contents::BlogPost" });

        expect(query("[data-cy='data-set-Contents::BlogPost'] [data-cy=data-set-choose]")).toBeNull();
        expect(query("[data-cy='data-set-Contents::BlogPost']")?.textContent).toContain("Chosen");
        expect(query("[data-cy=data-set-picker]")?.textContent).toContain("Change the data set");
    });

    it("load_Failed_ShowsAnError", async () => {
        await mountPicker({ load: vi.fn().mockRejectedValue(new Error("offline")) });

        expect(query("[data-cy=data-set-picker] [role=alert]")?.textContent).toContain("The data sets could not be loaded.");
    });
});
