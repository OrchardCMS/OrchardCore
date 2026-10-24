import { afterEach, beforeAll, describe, expect, it } from "vitest";

// The "Change" button of the "Read a data source" editor asks its host, the designer, for a data set with an event.
const renderPicker = (value = "Contents::BlogPost") => `
<form>
    <div data-dp-dataset-picker>
        <input type="hidden" name="DataSet" value="${value}" data-dp-dataset-input />
        <button type="button" data-dp-dataset-open>Change</button>
    </div>
</form>`;

type Detail = { current: string; choose: (value: string) => void };

const open = () => {
    document.body.innerHTML = renderPicker();

    let detail: Detail | null = null;
    document.addEventListener("dp:choose-data-set", (event) => (detail = (event as CustomEvent<Detail>).detail), { once: true });
    document.querySelector<HTMLButtonElement>("[data-dp-dataset-open]")!.click();

    return detail as Detail | null;
};

describe("data set picker of the editor", () => {
    beforeAll(async () => {
        await import("../../../Scripts/data-pipelines-editors.js");
    });

    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("open_Click_RaisesTheEventWithTheCurrentDataSet", () => {
        const detail = open();

        expect(detail?.current).toBe("Contents::BlogPost");
    });

    it("choose_AnotherDataSet_SetsTheInputAndNotifiesTheForm", () => {
        const detail = open()!;
        let changes = 0;
        document.querySelector("form")!.addEventListener("change", () => changes++);

        detail.choose("Users::Users");

        expect(document.querySelector<HTMLInputElement>("[data-dp-dataset-input]")!.value).toBe("Users::Users");
        expect(changes).toBe(1);
    });

    it("choose_TheSameDataSet_ChangesNothing", () => {
        const detail = open()!;
        let changes = 0;
        document.querySelector("form")!.addEventListener("change", () => changes++);

        detail.choose("Contents::BlogPost");

        expect(changes).toBe(0);
    });
});
