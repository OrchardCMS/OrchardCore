import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import SaveStatusIndicator from "../SaveStatusIndicator.vue";

describe("SaveStatusIndicator", () => {
    it("render_SavedWithDraft_SaysTheDraftIsSaved", () => {
        const wrapper = mount(SaveStatusIndicator, { props: { status: "saved", hasDraft: true, isPublished: true } });

        expect(wrapper.text()).toBe("Draft saved");
    });

    it("render_SavedWithoutDraft_SaysEverythingIsPublished", () => {
        const wrapper = mount(SaveStatusIndicator, { props: { status: "saved", hasDraft: false, isPublished: true } });

        expect(wrapper.text()).toBe("No unpublished changes");
    });

    it("render_SavedNeverPublished_SaysSaved", () => {
        const wrapper = mount(SaveStatusIndicator, { props: { status: "saved", hasDraft: false, isPublished: false } });

        expect(wrapper.text()).toBe("Saved");
    });

    it("render_Unsaved_IgnoresTheDraft", () => {
        const wrapper = mount(SaveStatusIndicator, { props: { status: "unsaved", hasDraft: false, isPublished: true } });

        expect(wrapper.text()).toBe("Unsaved changes");
    });
});
