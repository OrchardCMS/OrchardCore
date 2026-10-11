import { describe, expect, it } from "vitest";
import { buildPreviewTable, fieldReference, formatFileSize, formatValue } from "../previewTable";
import type { DesignerField } from "../../api/types";

const fields: DesignerField[] = [
    { name: "Title", displayName: "Title", type: "Text" },
    { name: "Count", displayName: "Count", type: "Integer" },
    { name: "Price", displayName: "", type: "Decimal" },
    { name: "Active", displayName: "Active", type: "Boolean" },
    { name: "Created", displayName: "Created", type: "DateTime" },
];

describe("previewTable", () => {
    it("buildPreviewTable_Rows_FormatsEachCellByItsFieldType", () => {
        const table = buildPreviewTable({ fields, rows: [["Order", 3, "12.50", true, "2026-10-01T10:30:00Z"]], truncated: false });

        expect(table.headers.map((header) => [header.displayName, header.typeLabel])).toEqual([
            ["Title", "Text"],
            ["Count", "Integer"],
            ["Price", "Decimal"],
            ["Active", "Boolean"],
            ["Created", "Date and time"],
        ]);
        expect(table.rows[0].map((cell) => cell.text)).toEqual(["Order", "3", "12.50", "true", "2026-10-01 10:30:00Z"]);
        // Numbers (and decimals sent as text) are aligned to the end.
        expect(table.rows[0].map((cell) => cell.isNumber)).toEqual([false, true, true, false, false]);
        expect(table.note).toBeNull();
    });

    it("buildPreviewTable_NullsAndShortRows_ShowNull", () => {
        const table = buildPreviewTable({ fields, rows: [[null, 1]], truncated: false });

        expect(table.rows[0].map((cell) => [cell.text, cell.isNull])).toEqual([
            ["null", true],
            ["1", false],
            ["null", true],
            ["null", true],
            ["null", true],
        ]);
    });

    it("buildPreviewTable_Truncated_NotesHowManyRowsAreShown", () => {
        const table = buildPreviewTable({ fields: fields.slice(0, 1), rows: [["a"], ["b"]], truncated: true });

        expect(table.note).toBe("Showing the first 2 rows.");
    });

    it("formatValue_FalseAndZero_AreNotNull", () => {
        expect(formatValue(false)).toEqual({ text: "false", isNull: false, isNumber: false });
        expect(formatValue(0)).toEqual({ text: "0", isNull: false, isNumber: true });
        expect(formatValue("2026-10-01", "Date").text).toBe("2026-10-01");
    });

    it("formatValue_DateTimeWithTicks_KeepsMillisecondsAtMost", () => {
        expect(formatValue("2026-10-10T20:47:54.0699028Z", "DateTime").text).toBe("2026-10-10 20:47:54.069Z");
        expect(formatValue("2026-10-10T20:47:54.0000000Z", "DateTime").text).toBe("2026-10-10 20:47:54Z");
        expect(formatValue("2026-10-10T20:47:54.5+02:00", "DateTime").text).toBe("2026-10-10 20:47:54.5+02:00");
    });

    it("formatFileSize_Lengths_UsesTheRightUnit", () => {
        expect(formatFileSize(512)).toBe("512 B");
        expect(formatFileSize(1536)).toBe("1.5 KB");
        expect(formatFileSize(5 * 1024 * 1024)).toBe("5.0 MB");
    });

    it("fieldReference_Name_IsBracketedWithClosingBracketsDoubled", () => {
        expect(fieldReference({ name: "Field Name" })).toBe("[Field Name]");
        expect(fieldReference({ name: "a]b" })).toBe("[a]]b]");
    });
});
