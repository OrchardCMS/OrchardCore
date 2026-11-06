import { afterEach, beforeAll, describe, expect, it } from "vitest";

// The filter rows of the "Read a data source" editor, which the designer hosts, are driven by the script of the editors.
const fields = [
    ["Title", "Text"],
    ["Count", "Integer"],
    ["Price", "Decimal"],
    ["Published", "Boolean"],
    ["Day", "Date"],
    ["Modified", "DateTime"],
];

// The types each operator applies to, as DataFilterOperatorExtensions.AppliesTo renders them.
const operators: Record<string, string> = {
    Equals: "Text Integer Decimal Boolean Date DateTime",
    Contains: "Text",
    StartsWith: "Text",
    GreaterThan: "Integer Decimal Date DateTime",
    Between: "Integer Decimal Date DateTime",
    In: "Text Integer Decimal Boolean Date DateTime",
    IsEmpty: "Text Integer Decimal Boolean Date DateTime",
    InLastDays: "Date DateTime",
};

const renderRow = (field: string, operator: string, value = "", second = "") => `
<div data-dp-row data-dp-filter-row>
    <select name="Filters[0].Field" data-dp-filter-field>
        <option value="">Choose a field</option>
        ${fields.map(([name, type]) => `<option value="${name}" data-dp-type="${type}" ${name === field ? "selected" : ""}>${name}</option>`).join("")}
    </select>
    <select name="Filters[0].Operator" data-dp-filter-operator>
        ${Object.entries(operators).map(([name, types]) => `<option value="${name}" data-dp-types="${types}" ${name === operator ? "selected" : ""}>${name}</option>`).join("")}
    </select>
    <input name="Filters[0].Value" value="${value}" placeholder="Value" data-dp-filter-value data-dp-value-placeholder="Value" data-dp-list-placeholder="Values, separated by commas" />
    <select name="Filters[0].Value" hidden disabled data-dp-filter-boolean>
        <option value="">Choose a value</option>
        <option value="true">Yes</option>
        <option value="false">No</option>
    </select>
    <span hidden data-dp-filter-days>days</span>
    <span hidden data-dp-filter-and>and</span>
    <input name="Filters[0].SecondValue" value="${second}" hidden data-dp-filter-second />
    <button type="button" hidden data-dp-filter-text></button>
</div>`;

const flush = () => new Promise((resolve) => setTimeout(resolve));

const addRow = async (field: string, operator: string, value = "", second = "") => {
    const form = document.createElement("form");
    form.innerHTML = renderRow(field, operator, value, second);
    document.body.appendChild(form);
    await flush();

    const row = form.querySelector<HTMLElement>("[data-dp-filter-row]")!;

    return {
        form,
        row,
        field: row.querySelector<HTMLSelectElement>("[data-dp-filter-field]")!,
        operator: row.querySelector<HTMLSelectElement>("[data-dp-filter-operator]")!,
        value: row.querySelector<HTMLInputElement>("[data-dp-filter-value]")!,
        booleanValue: row.querySelector<HTMLSelectElement>("[data-dp-filter-boolean]")!,
        second: row.querySelector<HTMLInputElement>("[data-dp-filter-second]")!,
        and: row.querySelector<HTMLElement>("[data-dp-filter-and]")!,
        days: row.querySelector<HTMLElement>("[data-dp-filter-days]")!,
        toggle: row.querySelector<HTMLButtonElement>("[data-dp-filter-text]")!,
    };
};

const choose = (select: HTMLSelectElement, value: string) => {
    select.value = value;
    select.dispatchEvent(new Event("change", { bubbles: true }));
};

const visibleOperators = (select: HTMLSelectElement) => Array.from(select.options).filter((option) => !option.hidden).map((option) => option.value);

describe("filter rows", () => {
    beforeAll(async () => {
        await import("../../../Scripts/data-pipelines-editors.js");
    });

    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("operators_TextField_ShowsTheOperatorsOfText", async () => {
        const row = await addRow("Title", "Equals");

        expect(visibleOperators(row.operator)).toEqual(["Equals", "Contains", "StartsWith", "In", "IsEmpty"]);
    });

    it("operators_FieldChangesToANumber_HidesTheTextOperatorsAndChoosesOneThatApplies", async () => {
        const row = await addRow("Title", "Contains", "abc");

        choose(row.field, "Count");

        expect(visibleOperators(row.operator)).toEqual(["Equals", "GreaterThan", "Between", "In", "IsEmpty"]);
        expect(row.operator.value).toBe("Equals");
        expect(row.operator.selectedOptions[0].disabled).toBe(false);
    });

    it("operators_FieldChangesAndTheOperatorApplies_KeepsIt", async () => {
        const row = await addRow("Count", "GreaterThan", "5");

        choose(row.field, "Day");

        expect(row.operator.value).toBe("GreaterThan");
    });

    it("operators_SavedOperatorThatDoesNotApply_StaysSelectedWhenLoaded", async () => {
        const row = await addRow("Day", "Contains", "2026");

        expect(row.operator.value).toBe("Contains");
        expect(row.operator.selectedOptions[0].hidden).toBe(false);
        expect(row.operator.selectedOptions[0].disabled).toBe(false);
    });

    it("values_Between_ShowsTheSecondValue", async () => {
        const row = await addRow("Count", "Equals", "1");

        expect(row.second.hidden).toBe(true);
        expect(row.and.hidden).toBe(true);

        choose(row.operator, "Between");

        expect(row.second.hidden).toBe(false);
        expect(row.and.hidden).toBe(false);
        expect(row.second.type).toBe("number");
    });

    it("values_IsEmpty_HidesTheValues", async () => {
        const row = await addRow("Count", "IsEmpty");

        expect(row.value.hidden).toBe(true);
        expect(row.second.hidden).toBe(true);
        expect(row.toggle.hidden).toBe(true);
    });

    it.each([
        ["Count", "Equals", "number", "1"],
        ["Price", "Equals", "number", "any"],
        ["Day", "Equals", "date", null],
        ["Modified", "GreaterThan", "datetime-local", null],
        ["Title", "Equals", "text", null],
    ])("values_%sField_UsesTheInputOfItsType", async (field, operator, type, step) => {
        const row = await addRow(field, operator);

        expect(row.value.type).toBe(type);
        expect(row.value.getAttribute("step")).toBe(step);
        expect(row.toggle.hidden).toBe(type === "text");
    });

    it("values_InLastDays_TakesANumberOfDays", async () => {
        const row = await addRow("Modified", "InLastDays", "7");

        expect(row.value.type).toBe("number");
        expect(row.value.min).toBe("1");
        expect(row.days.hidden).toBe(false);
        expect(row.second.hidden).toBe(true);
    });

    it("values_BooleanField_UsesAYesNoSelect", async () => {
        const row = await addRow("Published", "Equals", "True");

        expect(row.booleanValue.hidden).toBe(false);
        expect(row.booleanValue.disabled).toBe(false);
        expect(row.booleanValue.value).toBe("true");
        expect(row.value.hidden).toBe(true);
        expect(row.value.disabled).toBe(true);
        expect(new FormData(row.form).getAll("Filters[0].Value")).toEqual(["true"]);
    });

    it("values_Placeholder_StaysAsText", async () => {
        const row = await addRow("Day", "GreaterThan", "{Parameter:Since}");

        expect(row.value.type).toBe("text");
        expect(row.value.value).toBe("{Parameter:Since}");
        expect(row.toggle.getAttribute("aria-pressed")).toBe("true");
    });

    it("values_DateWithoutTimeOnADateTimeField_StaysAsText", async () => {
        const row = await addRow("Modified", "Between", "2026-01-01", "2026-01-31");

        expect(row.value.type).toBe("text");
        expect(row.second.type).toBe("text");
    });

    it.each([["Day"], ["Count"], ["Published"], ["Title"]])("values_IsOneOfOnA%sField_TakesAListAsText", async (field) => {
        const row = await addRow(field, "In", "2026-01-01, {Parameter:Day}");

        expect(row.value.type).toBe("text");
        expect(row.value.value).toBe("2026-01-01, {Parameter:Day}");
        expect(row.value.placeholder).toBe("Values, separated by commas");
        expect(row.value.disabled).toBe(false);
        expect(row.booleanValue.disabled).toBe(true);
        expect(row.second.hidden).toBe(true);
        expect(row.toggle.hidden).toBe(true);
    });

    it("values_IsOneOfThenEquals_ShowsTheTypedInputAgain", async () => {
        const row = await addRow("Count", "In", "1, 2");

        choose(row.operator, "Equals");

        expect(row.value.placeholder).toBe("Value");
        expect(row.value.type).toBe("text");
        expect(row.toggle.getAttribute("aria-pressed")).toBe("true");
    });

    it("toggle_TextModeOn_WritesTheValueAsText", async () => {
        const row = await addRow("Published", "Equals", "false");

        row.toggle.click();

        expect(row.toggle.getAttribute("aria-pressed")).toBe("true");
        expect(row.booleanValue.disabled).toBe(true);
        expect(row.value.disabled).toBe(false);
        expect(row.value.type).toBe("text");
        expect(row.value.value).toBe("false");
    });

    it("toggle_TextModeOffWithAValueTheInputCannotHold_ClearsIt", async () => {
        const row = await addRow("Day", "Equals", "{Parameter:Since}");

        row.toggle.click();

        expect(row.toggle.getAttribute("aria-pressed")).toBe("false");
        expect(row.value.type).toBe("date");
        expect(row.value.value).toBe("");
    });

    it("toggle_Click_NotifiesTheForm", async () => {
        const row = await addRow("Day", "Equals", "2026-01-01");
        let changes = 0;
        row.form.addEventListener("change", () => changes++);

        row.toggle.click();

        expect(changes).toBe(1);
    });
});
