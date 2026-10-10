import type { DesignerField, FieldType, PreviewPort } from "../api/types";
import { t } from "../i18n";

// How the Data tab shows the rows of a preview: one column per field, its type under its name, and the values as the
// server sent them (null shown as such, numbers aligned to the end).

export type PreviewValue = string | number | boolean | null;

export interface PreviewCell {
    text: string;
    isNull: boolean;
    isNumber: boolean;
}

export interface PreviewTable {
    headers: { name: string; displayName: string; type: FieldType; typeLabel: string }[];
    rows: PreviewCell[][];
    // "Showing the first 50 rows.", when the step produced more.
    note: string | null;
}

export const fieldTypeLabel = (type: FieldType) =>
    ({
        Text: () => t("FieldTypeText", "Text"),
        Integer: () => t("FieldTypeInteger", "Integer"),
        Decimal: () => t("FieldTypeDecimal", "Decimal"),
        Boolean: () => t("FieldTypeBoolean", "Boolean"),
        Date: () => t("FieldTypeDate", "Date"),
        DateTime: () => t("FieldTypeDateTime", "Date and time"),
    })[type]?.() ?? type;

const isNumericType = (type: FieldType | undefined) => type === "Integer" || type === "Decimal";

/**
 * The text of a value: null is "null", booleans are "true"/"false", a date and time loses its "T" separator (its zone
 * designator stays, so the value isn't misread), and everything else is shown as is.
 */
export const formatValue = (value: PreviewValue | undefined, type?: FieldType): PreviewCell => {
    if (value === null || value === undefined) {
        return { text: "null", isNull: true, isNumber: false };
    }

    if (typeof value === "boolean") {
        return { text: value ? "true" : "false", isNull: false, isNumber: false };
    }

    if (typeof value === "number") {
        return { text: String(value), isNull: false, isNumber: true };
    }

    if (type === "DateTime" && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(value)) {
        return { text: value.replace("T", " "), isNull: false, isNumber: false };
    }

    return { text: value, isNull: false, isNumber: isNumericType(type) };
};

/**
 * The table of a records port. A row shorter than the fields gets null cells.
 */
export const buildPreviewTable = (port: Pick<PreviewPort, "fields" | "rows" | "truncated">): PreviewTable => ({
    headers: port.fields.map((field: DesignerField) => ({
        name: field.name,
        displayName: field.displayName || field.name,
        type: field.type,
        typeLabel: fieldTypeLabel(field.type),
    })),
    rows: port.rows.map((row) => port.fields.map((field, index) => formatValue(row[index], field.type))),
    note: port.truncated ? t("ShowingFirstRows", "Showing the first {0} rows.", port.rows.length) : null,
});

/**
 * A file size: bytes, then KB, MB and GB with one decimal.
 */
export const formatFileSize = (length: number) => {
    if (length < 1024) {
        return t("Bytes", "{0} B", length);
    }

    const units = [t("Kilobytes", "{0} KB"), t("Megabytes", "{0} MB"), t("Gigabytes", "{0} GB")];
    let value = length / 1024;
    let unit = 0;

    while (value >= 1024 && unit < units.length - 1) {
        value /= 1024;
        unit++;
    }

    return units[unit].replace("{0}", value.toFixed(1));
};

/**
 * The reference of a field in a formula: its name in square brackets, a closing bracket in it doubled.
 */
export const fieldReference = (field: Pick<DesignerField, "name">) => `[${field.name.replace(/]/g, "]]")}]`;
