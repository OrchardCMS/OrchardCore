import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import initIconPickerTriggers from "@orchardcore/bloom/components/icon-picker-trigger";
import initPermissionPicker, { PermissionPickerItem } from "@orchardcore/bloom/components/permission-picker";

const permissionPickerElement = document.querySelector<HTMLElement>("#PermissionPicker");

if (permissionPickerElement) {
    const selectedItems = getDatasetJson<PermissionPickerItem[]>(permissionPickerElement, "selectedItems") ?? [];
    const allItems = getDatasetJson<PermissionPickerItem[]>(permissionPickerElement, "allItems") ?? [];
    const translations = getDatasetJson<Record<string, string>>(permissionPickerElement, "translations");

    if (translations) {
        initPermissionPicker({
            element: permissionPickerElement,
            selectedItems,
            allItems,
            hiddenInputId: permissionPickerElement.dataset.selectedNamesInputId ?? "",
            hiddenInputName: permissionPickerElement.dataset.selectedNamesInputName ?? "",
            hintText: permissionPickerElement.dataset.hintText ?? "",
            createdEventName: "admin-menu-permission-picker-created",
            translations,
        });
    }
}

initIconPickerTriggers();
