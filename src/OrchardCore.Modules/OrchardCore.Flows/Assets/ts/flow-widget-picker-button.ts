import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

// Matches content-type-picker.ts's own (module-local, unexported) ContentTypePickerContentType -
// duplicated here rather than imported since structural typing makes the two interchangeable
// wherever showContentTypePicker's Window-global type is checked.
interface FlowWidgetPickerContentType {
    name: string;
    displayName: string;
    description?: string;
    category?: string;
    thumbnail?: string;
}

// Shared by BagPart-Blocks.Edit.cshtml, FlowPart-Blocks.Edit.cshtml and
// ContentCard-FlowPartBlocks.Edit.cshtml - wires an "Add/Insert Widget" button to the shared
// content-type-picker modal (window.showContentTypePicker, defined in content-type-picker.ts,
// same TS program/module so its Window augmentation is already visible here). Widget-attachable
// (repeatable and AJAX-reachable), so callers wire this up via observeAndInit.
const initFlowWidgetPickerButton = (button: HTMLElement) => {
    const contentTypes = getDatasetJson<FlowWidgetPickerContentType[]>(button, "contentTypes") ?? [];
    const targetId = button.dataset.targetId ?? "";
    const htmlFieldPrefix = button.dataset.htmlFieldPrefix ?? "";
    const prefixesName = button.dataset.prefixesName ?? "";
    const contentTypesName = button.dataset.contentTypesName ?? "";
    const contentItemsName = button.dataset.contentItemsName ?? "";
    const parentContentType = button.dataset.parentContentType ?? "";
    const partName = button.dataset.partName ?? "";
    const flowmetadata = button.dataset.flowmetadata === "true";
    const cardCollectionType = button.dataset.cardCollectionType;
    const modalTitle = button.dataset.modalTitle;
    const insertBeforeClosest = button.dataset.insertBeforeClosest;

    button.addEventListener("click", () => {
        const insertBefore = insertBeforeClosest ? (button.closest(insertBeforeClosest) ?? undefined) : undefined;

        window.showContentTypePicker({
            targetId,
            htmlFieldPrefix,
            prefixesName,
            contentTypesName,
            contentItemsName,
            parentContentType,
            partName,
            contentTypes,
            flowmetadata,
            cardCollectionType,
            insertBefore,
            modalTitle,
            onContentTypeSelected: window.contentTypePickerSelectContentType,
        });
    });
};

export default initFlowWidgetPickerButton;
