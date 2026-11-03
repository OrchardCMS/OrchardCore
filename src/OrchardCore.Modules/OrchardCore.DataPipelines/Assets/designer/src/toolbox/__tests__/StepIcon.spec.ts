import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import StepIcon from "../StepIcon.vue";
import { CATEGORY_ICONS } from "../filter";

describe("StepIcon", () => {
    it("StepIcon_IconHtml_RendersTheShapeOfTheServer", () => {
        const wrapper = mount(StepIcon, { props: { step: { iconHtml: '<i class="fa-solid fa-filter"></i>', category: "Transform" } } });

        expect(wrapper.find(".dpd-step-icon .fa-filter").exists()).toBe(true);
    });

    it("StepIcon_NoIconHtml_UsesTheIconOfTheCategory", () => {
        const wrapper = mount(StepIcon, { props: { step: { iconHtml: null, category: "Destination" } } });

        expect(wrapper.find("i.dpd-step-icon").classes()).toEqual(expect.arrayContaining(CATEGORY_ICONS.Destination.split(" ")));
    });
});
