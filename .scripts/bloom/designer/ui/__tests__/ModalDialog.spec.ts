import { afterEach, describe, expect, it } from "vitest";
import { mount, type VueWrapper } from "@vue/test-utils";
import ModalDialog from "../ModalDialog.vue";

describe("ModalDialog", () => {
    let wrapper: VueWrapper | null = null;

    afterEach(() => {
        wrapper?.unmount();
        wrapper = null;
    });

    const dialogClasses = () => Array.from(document.body.querySelector(".modal-dialog")?.classList ?? []);

    it("render_WithoutSize_UsesTheDefaultWidth", () => {
        wrapper = mount(ModalDialog, { props: { title: "Title" }, attachTo: document.body });

        expect(dialogClasses()).not.toContain("modal-lg");
        expect(dialogClasses()).not.toContain("modal-xl");
    });

    it("render_WithSize_UsesTheBootstrapSizeClass", () => {
        wrapper = mount(ModalDialog, { props: { title: "Title", size: "lg" }, attachTo: document.body });

        expect(dialogClasses()).toContain("modal-lg");
    });
});
