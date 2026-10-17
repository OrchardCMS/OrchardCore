import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import initMultiselectPicker, { MultiselectPickerItem } from "@orchardcore/bloom/components/multiselect-picker";
import initMultiTextFieldPicker, { MultiTextFieldPickerOption } from "@orchardcore/bloom/components/multitextfield-picker";

observeAndInit(".vue-multiselect[data-editor-type='ContentPicker']", (element) => {
    const selectedItems = getDatasetJson<MultiselectPickerItem[]>(element, "selectedItems") ?? [];
    const translations = getDatasetJson<Record<string, string>>(element, "translations");

    if (!translations) {
        return;
    }

    initMultiselectPicker({
        element,
        selectedItems,
        searchUrl: element.dataset.searchUrl ?? "",
        multiple: element.dataset.multiple === "true",
        hiddenInputId: element.dataset.selectedIdsInputId ?? "",
        hiddenInputName: element.dataset.selectedIdsInputName ?? "",
        statusField: "hasPublished",
        statusLabelKey: "NotPublished",
        clickableLinks: {
            editUrl: element.dataset.editUrl ?? "",
            viewUrl: element.dataset.viewUrl ?? "",
        },
        createdEventName: "vue-multiselect-created",
        translations,
    });
});

observeAndInit(".vue-multiselect[data-editor-type='LocalizationSetContentPicker']", (element) => {
    const selectedItems = getDatasetJson<MultiselectPickerItem[]>(element, "selectedItems") ?? [];
    const translations = getDatasetJson<Record<string, string>>(element, "translations");

    if (!translations) {
        return;
    }

    initMultiselectPicker({
        element,
        selectedItems,
        searchUrl: element.dataset.searchUrl ?? "",
        multiple: element.dataset.multiple === "true",
        hiddenInputId: element.dataset.selectedIdsInputId ?? "",
        hiddenInputName: element.dataset.selectedIdsInputName ?? "",
        statusField: "hasPublished",
        statusLabelKey: "NotPublished",
        createdEventName: "vue-multiselect-created",
        translations,
    });
});

observeAndInit(".vue-multiselect[data-editor-type='UserPicker']", (element) => {
    const selectedItems = getDatasetJson<MultiselectPickerItem[]>(element, "selectedUsers") ?? [];
    const translations = getDatasetJson<Record<string, string>>(element, "translations");

    if (!translations) {
        return;
    }

    initMultiselectPicker({
        element,
        selectedItems,
        searchUrl: element.dataset.searchUrl ?? "",
        multiple: element.dataset.multiple === "true",
        hiddenInputId: element.dataset.selectedIdsInputId ?? "",
        hiddenInputName: element.dataset.selectedIdsInputName ?? "",
        statusField: "isEnabled",
        statusLabelKey: "NotEnabled",
        createdEventName: "vue-multiselect-userpicker-created",
        translations,
        placeholder: element.dataset.placeholder,
    });
});

observeAndInit(".multitextfieldpicker", (element) => {
    const selectedValues = getDatasetJson<MultiTextFieldPickerOption[]>(element, "selectedvalues") ?? [];
    const options = getDatasetJson<MultiTextFieldPickerOption[]>(element, "options") ?? [];
    const translations = getDatasetJson<Record<string, string>>(element, "translations");

    if (!translations) {
        return;
    }

    initMultiTextFieldPicker({
        element,
        selectedValues,
        options,
        valuesInputName: element.dataset.valueskey ?? "",
        translations,
    });
});
