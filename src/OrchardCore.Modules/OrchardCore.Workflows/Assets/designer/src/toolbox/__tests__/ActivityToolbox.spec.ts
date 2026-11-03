import { describe, expect, it, vi } from "vitest";
import { mount } from "@vue/test-utils";
import ActivityToolbox from "../ActivityToolbox.vue";
import { ACTIVITY_DRAG_TYPE } from "../filter";
import { library } from "./library";

describe("ActivityToolbox", () => {
    it("render_Library_ShowsCollapsedCategoriesThatToggle", async () => {
        const wrapper = mount(ActivityToolbox, { props: { library, loading: false, error: null }, attachTo: document.body });
        const http = wrapper.get("[data-cy=toolbox-category-HTTP]");

        expect(wrapper.findAll(".wfd-toolbox-category")).toHaveLength(3);
        expect(http.attributes("aria-expanded")).toBe("false");
        expect(wrapper.get("[data-cy=toolbox-activity-HttpRequestEvent]").isVisible()).toBe(false);

        await http.trigger("click");

        expect(http.attributes("aria-expanded")).toBe("true");
        expect(wrapper.get("[data-cy=toolbox-activity-HttpRequestEvent]").isVisible()).toBe(true);
        wrapper.unmount();
    });

    it("search_Query_FiltersAndExpandsMatches", async () => {
        const wrapper = mount(ActivityToolbox, { props: { library, loading: false, error: null }, attachTo: document.body });

        await wrapper.get("[data-cy=toolbox-search]").setValue("notify");

        expect(wrapper.findAll(".wfd-toolbox-card").map((card) => card.attributes("data-cy"))).toEqual(["toolbox-activity-NotifyTask"]);
        expect(wrapper.get("[data-cy=toolbox-activity-NotifyTask]").isVisible()).toBe(true);

        await wrapper.get("[data-cy=toolbox-search]").setValue("nothing like this");
        expect(wrapper.find("[data-cy=toolbox-empty]").exists()).toBe(true);
        wrapper.unmount();
    });

    it("kindFilter_Events_ShowsOnlyEvents", async () => {
        const wrapper = mount(ActivityToolbox, { props: { library, loading: false, error: null } });

        await wrapper.get("[data-cy=toolbox-filter-events]").setValue(true);

        expect(wrapper.findAll(".wfd-toolbox-card").map((card) => card.attributes("data-cy"))).toEqual([
            "toolbox-activity-ContentPublishedEvent",
            "toolbox-activity-HttpRequestEvent",
        ]);
    });

    it("card_ClickAndDrag_EmitsAddAndCarriesActivityName", async () => {
        const wrapper = mount(ActivityToolbox, { props: { library, loading: false, error: null } });
        const card = wrapper.get("[data-cy=toolbox-activity-NotifyTask]");

        await card.trigger("click");
        expect(wrapper.emitted("add")).toEqual([["NotifyTask"]]);

        const setData = vi.fn();
        const event = new Event("dragstart", { bubbles: true });
        Object.assign(event, { dataTransfer: { setData, effectAllowed: "" } });
        card.element.dispatchEvent(event);

        expect(setData).toHaveBeenCalledWith(ACTIVITY_DRAG_TYPE, "NotifyTask");
        expect(card.find("i").classes()).toEqual(expect.arrayContaining(["fa-solid", "fa-bell"]));
    });

    it("card_Preset_EmitsAndCarriesItsKey", async () => {
        const withPreset = {
            categories: [
                ...library.categories,
                {
                    name: "Workflows",
                    activities: [
                        {
                            name: "ExecuteWorkflowTask",
                            preset: "workflow:approval",
                            displayText: "Approval",
                            category: "Workflows",
                            isEvent: false,
                            hasEditor: true,
                            thumbnailHtml: "<h4>Approval</h4>",
                        },
                    ],
                },
            ],
        };
        const wrapper = mount(ActivityToolbox, { props: { library: withPreset, loading: false, error: null } });
        const card = wrapper.get("[data-cy='toolbox-activity-preset:workflow:approval']");

        await card.trigger("click");
        expect(wrapper.emitted("add")).toEqual([["preset:workflow:approval"]]);

        const setData = vi.fn();
        const event = new Event("dragstart", { bubbles: true });
        Object.assign(event, { dataTransfer: { setData, effectAllowed: "" } });
        card.element.dispatchEvent(event);

        expect(setData).toHaveBeenCalledWith(ACTIVITY_DRAG_TYPE, "preset:workflow:approval");
    });

    it("render_LoadingAndError_ShowsStatus", () => {
        expect(mount(ActivityToolbox, { props: { library: null, loading: true, error: null } }).find(".spinner-border").exists()).toBe(true);
        expect(mount(ActivityToolbox, { props: { library: null, loading: false, error: "Boom" } }).get("[role=alert]").text()).toBe("Boom");
    });
});
