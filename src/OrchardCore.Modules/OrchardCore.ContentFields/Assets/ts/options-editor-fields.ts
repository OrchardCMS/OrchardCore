import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import initOptionsTableEditor, { setTranslations } from "@orchardcore/bloom/components/options-table-editor";

// Shared by both TextFieldPredefinedListEditorSettings.Edit.cshtml (radio "default" column) and
// MultiTextFieldSettings.Edit.cshtml (checkbox "default" column) - which mode to render is
// entirely determined by whether data-default-value-input-name is present, since only the radio
// case needs a shared form-field name for its input group. Column/default-column key config is
// static per view (hard-coded below, not server-driven); the actual localized label TEXT comes
// from ContentFieldsJSLocalizer's "options-table-editor" group via data-translations, following
// the IJSLocalizer pattern (see src/docs/reference/modules/Localize/javascript-localization.md).
observeAndInit(".options-editor-wrapper", (element) => {
    const rows = getDatasetJson<Record<string, string>[]>(element, "options") ?? [];
    const translations = getDatasetJson<Record<string, string>>(element, "translations");
    const defaultValueInputName = element.dataset.defaultValueInputName;
    const modalBodyElements = document.getElementsByClassName(`${element.id}-ModalBody`);

    if (!translations) {
        return;
    }

    setTranslations(translations);

    initOptionsTableEditor({
        element,
        rows,
        columns: [
            { key: "name", labelKey: "OptionLabelColumn", placeholderKey: "EnterAName" },
            { key: "value", labelKey: "ValueColumn", placeholderKey: "EnterAValue" },
        ],
        defaultColumn: defaultValueInputName
            ? { key: "value", labelKey: "DefaultColumn", mode: "radio", radioGroupName: defaultValueInputName }
            : { key: "default", labelKey: "DefaultColumn", mode: "checkbox" },
        initialDefaultValue: element.dataset.defaultValue,
        filterEmptyKey: "name",
        addKey: "AddAnOption",
        editDataKey: "EditData",
        okKey: "Ok",
        cancelKey: "Cancel",
        removeRowKey: "RemoveElementFromList",
        jsonTextareaLabelKey: "Options",
        jsonTextareaHintKey: "OptionsJsonHint",
        hiddenInputId: element.dataset.optionsInputId ?? "",
        hiddenInputName: element.dataset.optionsInputName ?? "",
        modalBodyElements,
    });
});
