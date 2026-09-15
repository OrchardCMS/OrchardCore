// Shared by BagPart-Blocks.Edit.cshtml and FlowPart-Blocks.Edit.cshtml - both call the existing
// window.initFlowSortableWidgets (flows.edit.ts, same TS program) with a fixed literal
// "contentTypes" dataset key and their own fixed literal group name; only partName varies
// per-instance, read off the placeholder's own data-part-name attribute.
const initFlowSortablePlaceholder = (placeholder: HTMLElement, groupName: string) => {
    const partName = placeholder.dataset.partName ?? "";
    window.initFlowSortableWidgets(placeholder.id, "contentTypes", groupName, partName);
};

export default initFlowSortablePlaceholder;
