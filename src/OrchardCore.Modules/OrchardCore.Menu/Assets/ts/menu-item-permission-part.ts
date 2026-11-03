import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import initPermissionPicker, { PermissionPickerItem } from "@orchardcore/bloom/components/permission-picker";

observeAndInit("#PermissionPicker", (element) => {
    const selectedItems = getDatasetJson<PermissionPickerItem[]>(element, "selectedItems") ?? [];
    const allItems = getDatasetJson<PermissionPickerItem[]>(element, "allItems") ?? [];
    const translations = getDatasetJson<Record<string, string>>(element, "translations");

    if (!translations) {
        return;
    }

    initPermissionPicker({
        element,
        selectedItems,
        allItems,
        hiddenInputId: element.dataset.selectedNamesInputId ?? "",
        hiddenInputName: element.dataset.selectedNamesInputName ?? "",
        hintText: element.dataset.hintText ?? "",
        createdEventName: "menu-permission-picker-created",
        translations,
    });
});
