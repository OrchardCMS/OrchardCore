import { afterEach, beforeAll, describe, expect, it } from "vitest";

// The data set picker of the "Read a data source" editor, which the designer hosts, is driven by the script of the
// editors. Bootstrap isn't loaded here: a stand-in modal shows and hides the dialog as Bootstrap does.
class FakeModal {
    static instances = new Map<Element, FakeModal>();

    constructor(private readonly element: Element) {}

    static getOrCreateInstance(element: Element) {
        if (!FakeModal.instances.has(element)) {
            FakeModal.instances.set(element, new FakeModal(element));
        }

        return FakeModal.instances.get(element)!;
    }

    static getInstance(element: Element) {
        return FakeModal.instances.get(element) ?? null;
    }

    show() {
        this.element.classList.add("show");
        this.element.dispatchEvent(new Event("shown.bs.modal"));
    }

    hide() {
        this.element.dispatchEvent(new Event("hide.bs.modal"));
        this.element.classList.remove("show");
        this.element.dispatchEvent(new Event("hidden.bs.modal"));
    }

    dispose() {
        FakeModal.instances.delete(this.element);
    }
}

const dataSets = [
    { value: "Contents::BlogPost", text: "Blog Post", source: "Content items", description: "The posts of the blog." },
    { value: "Contents::Article", text: "Article", source: "Content items", description: "" },
    { value: "Users::Users", text: "Users", source: "Users", description: "The users of the site." },
];

const card = (dataSet: (typeof dataSets)[number]) => `
<div class="col" data-dp-dataset-card data-dp-category="${dataSet.source}" data-dp-search-text="${[dataSet.text, dataSet.value, dataSet.description, dataSet.source].join(" ").toLowerCase()}">
    <button type="button" data-dp-dataset-choose="${dataSet.value}">Choose</button>
</div>`;

const category = (name: string, label: string, active = false) => `
<button type="button" class="nav-link${active ? " active" : ""}" data-dp-dataset-category="${name}"><span>${label}</span><span data-dp-dataset-count></span></button>`;

const renderPicker = () => `
<form>
    <div data-dp-dataset-picker>
        <input type="hidden" name="DataSet" value="" data-dp-dataset-input />
        <button type="button" data-dp-dataset-open>Choose a data set</button>
        <template data-dp-dataset-dialog>
            <div class="modal fade">
                <input type="search" data-dp-dataset-filter />
                ${category("", "All", true)}${category("Content items", "Content items")}${category("Users", "Users")}
                ${dataSets.map(card).join("")}
                <p hidden data-dp-dataset-empty>No data set matches the filter.</p>
            </div>
        </template>
    </div>
</form>`;

const open = () => {
    document.body.innerHTML = renderPicker();
    document.querySelector<HTMLButtonElement>("[data-dp-dataset-open]")!.click();

    return document.querySelector<HTMLElement>("body > .modal")!;
};

const visibleCards = (dialog: HTMLElement) =>
    Array.from(dialog.querySelectorAll<HTMLElement>("[data-dp-dataset-card]"))
        .filter((element) => !element.hidden)
        .map((element) => element.querySelector<HTMLElement>("[data-dp-dataset-choose]")!.dataset.dpDatasetChoose);

const counts = (dialog: HTMLElement) =>
    Array.from(dialog.querySelectorAll<HTMLElement>("[data-dp-dataset-count]")).map((element) => element.textContent);

const type = (input: HTMLInputElement, value: string) => {
    input.value = value;
    input.dispatchEvent(new Event("input", { bubbles: true }));
};

describe("data set picker", () => {
    beforeAll(async () => {
        (window as unknown as { bootstrap: unknown }).bootstrap = { Modal: FakeModal };
        await import("../../../Scripts/data-pipelines-editors.js");
    });

    afterEach(() => {
        document.body.innerHTML = "";
        FakeModal.instances.clear();
    });

    it("open_Click_ShowsEveryDataSetOverThePage", () => {
        const dialog = open();

        expect(dialog.classList.contains("show")).toBe(true);
        expect(dialog.closest("form")).toBeNull();
        expect(visibleCards(dialog)).toEqual(["Contents::BlogPost", "Contents::Article", "Users::Users"]);
        expect(counts(dialog)).toEqual(["3", "2", "1"]);
    });

    it("category_Click_ShowsTheDataSetsOfItsSource", () => {
        const dialog = open();
        const users = dialog.querySelector<HTMLButtonElement>("[data-dp-dataset-category='Users']")!;

        users.click();

        expect(visibleCards(dialog)).toEqual(["Users::Users"]);
        expect(users.classList.contains("active")).toBe(true);
        expect(users.getAttribute("aria-pressed")).toBe("true");
    });

    it("filter_Terms_MatchTheNameAndTheDescription", () => {
        const dialog = open();
        const filter = dialog.querySelector<HTMLInputElement>("[data-dp-dataset-filter]")!;

        type(filter, "posts blog");

        expect(visibleCards(dialog)).toEqual(["Contents::BlogPost"]);
        expect(counts(dialog)).toEqual(["1", "1", "0"]);

        type(filter, "nothing");

        expect(visibleCards(dialog)).toEqual([]);
        expect(dialog.querySelector<HTMLElement>("[data-dp-dataset-empty]")!.hidden).toBe(false);
    });

    it("choose_Click_SetsTheDataSetNotifiesTheFormAndCloses", () => {
        const dialog = open();
        const form = document.querySelector("form")!;
        let changes = 0;
        form.addEventListener("change", () => changes++);

        dialog.querySelector<HTMLButtonElement>("[data-dp-dataset-choose='Users::Users']")!.click();

        expect(document.querySelector<HTMLInputElement>("[data-dp-dataset-input]")!.value).toBe("Users::Users");
        expect(changes).toBe(1);
        expect(document.querySelector("body > .modal")).toBeNull();
    });
});
