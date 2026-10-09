import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import StartPicker from "../StartPicker.vue";
import { library } from "../../toolbox/__tests__/library";
import type { Library } from "../../api/types";

describe("StartPicker", () => {
    it("render_Library_OffersTheCommonEventsFirstAndAddsThePickedOne", async () => {
        const wrapper = mount(StartPicker, { props: { library } });

        // Only events, the common ones in their order.
        expect(wrapper.findAll(".wfd-start-picker-event").map((event) => event.text())).toEqual(["Contenu publié", "Http Request Event"]);

        await wrapper.get("[data-cy=start-picker-HttpRequestEvent]").trigger("click");

        expect(wrapper.emitted("pick")).toEqual([["HttpRequestEvent"]]);
    });

    it("render_ManyEvents_OffersSix", () => {
        const many: Library = {
            categories: [
                {
                    name: "Events",
                    activities: Array.from({ length: 9 }, (_, index) => ({
                        name: `Event${index}`,
                        displayText: `Event ${index}`,
                        category: "Events",
                        isEvent: true,
                        hasEditor: false,
                        thumbnailHtml: "",
                    })),
                },
            ],
        };

        expect(mount(StartPicker, { props: { library: many } }).findAll(".wfd-start-picker-event")).toHaveLength(6);
    });

    it("render_NoLibrary_ShowsNothing", () => {
        expect(
            mount(StartPicker, { props: { library: null } })
                .find("[data-cy=start-picker]")
                .exists(),
        ).toBe(false);
    });
});
