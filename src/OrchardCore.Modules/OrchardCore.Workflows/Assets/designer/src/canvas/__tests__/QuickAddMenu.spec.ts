import { afterEach, describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import QuickAddMenu from "../QuickAddMenu.vue";
import { library } from "../../toolbox/__tests__/library";

const setup = () => mount(QuickAddMenu, { props: { library, outcome: "Done", left: 10, top: 20 }, attachTo: document.body });

describe("QuickAddMenu", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("render_Library_ListsTheTasksFirstAndFocusesTheSearch", async () => {
        const wrapper = setup();
        await new Promise((resolve) => setTimeout(resolve));

        expect(wrapper.findAll(".wfd-quick-add-name").map((item) => item.text())).toEqual([
            "Créer un contenu",
            "Http Redirect Task",
            "Notify Task",
            "Contenu publié",
            "Http Request Event",
        ]);
        expect(document.activeElement).toBe(wrapper.get("[data-cy=quick-add-search]").element);
        expect(wrapper.get("[data-cy=quick-add]").attributes("style")).toContain("left: 10px");
    });

    it("keyboard_SearchArrowsAndEnter_PickTheActiveActivity", async () => {
        const wrapper = setup();
        const search = wrapper.get("[data-cy=quick-add-search]");

        await search.setValue("http");
        expect(wrapper.findAll(".wfd-quick-add-name").map((item) => item.text())).toEqual(["Http Redirect Task", "Http Request Event"]);

        await search.trigger("keydown", { key: "ArrowDown" });
        expect(wrapper.findAll(".wfd-quick-add-item")[1].attributes("aria-selected")).toBe("true");

        await search.trigger("keydown", { key: "Enter" });
        expect(wrapper.emitted("pick")).toEqual([["HttpRequestEvent"]]);
    });

    it("dismiss_EscapeOrAClickElsewhere_Cancels", async () => {
        const wrapper = setup();

        await wrapper.get("[data-cy=quick-add-search]").trigger("keydown", { key: "Escape" });
        document.body.dispatchEvent(new MouseEvent("pointerdown", { bubbles: true }));

        expect(wrapper.emitted("cancel")).toHaveLength(2);
    });
});
