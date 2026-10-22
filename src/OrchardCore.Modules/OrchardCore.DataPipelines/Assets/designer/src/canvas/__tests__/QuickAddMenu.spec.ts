import { afterEach, describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import QuickAddMenu from "../QuickAddMenu.vue";
import { library } from "../../__tests__/fixtures";
import type { PortKind } from "../../api/types";

const setup = (kind: PortKind = "Records") => mount(QuickAddMenu, { props: { library, kind, port: "Matched", left: 10, top: 20 }, attachTo: document.body });

const names = (wrapper: ReturnType<typeof setup>) => wrapper.findAll(".dpd-quick-add-name").map((item) => item.text());

describe("QuickAddMenu", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("render_RecordsOutput_ListsOnlyStepsThatTakeRowsAndFocusesTheSearch", async () => {
        const wrapper = setup();
        await new Promise((resolve) => setTimeout(resolve));

        expect(names(wrapper)).toEqual(["Filter", "Join", "Write a CSV file", "Write to a table"]);
        expect(document.activeElement).toBe(wrapper.get("[data-cy=quick-add-search]").element);
        expect(wrapper.get("[data-cy=quick-add]").attributes("style")).toContain("left: 10px");
    });

    it("render_FilesOutput_ListsOnlyStepsThatTakeFiles", () => {
        expect(names(setup("Files"))).toEqual(["Zip the files", "Send by email"]);
    });

    it("keyboard_SearchArrowsAndEnter_PickTheActiveStep", async () => {
        const wrapper = setup();
        const search = wrapper.get("[data-cy=quick-add-search]");

        await search.setValue("write");
        expect(names(wrapper)).toEqual(["Write a CSV file", "Write to a table"]);

        await search.trigger("keydown", { key: "ArrowDown" });
        await search.trigger("keydown", { key: "Enter" });

        expect(wrapper.emitted("pick")).toEqual([["TableDestination"]]);
    });

    it("search_NoMatch_SaysNoStepCanReceiveTheOutput", async () => {
        const wrapper = setup("Files");

        await wrapper.get("[data-cy=quick-add-search]").setValue("filter");

        expect(wrapper.find("[data-cy=quick-add-empty]").exists()).toBe(true);
    });

    it("dismiss_EscapeOrAClickElsewhere_Cancels", async () => {
        const wrapper = setup();

        await wrapper.get("[data-cy=quick-add-search]").trigger("keydown", { key: "Escape" });
        document.body.dispatchEvent(new MouseEvent("pointerdown", { bubbles: true }));

        expect(wrapper.emitted("cancel")).toHaveLength(2);
    });
});
