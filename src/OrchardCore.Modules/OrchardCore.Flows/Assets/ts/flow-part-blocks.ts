import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import initFlowWidgetPickerButton from "./flow-widget-picker-button";
import initFlowSortablePlaceholder from "./flow-sortable-placeholder";
import initContentTypePickerModal from "./content-type-picker-modal-init";

if (document.getElementById("contentTypePickerModalTemplate")) {
    initContentTypePickerModal();
}

observeAndInit(".flow-widget-picker-button.flowpart-blocks-picker-button", initFlowWidgetPickerButton);
observeAndInit(".flowpart-blocks-sortable-placeholder", (placeholder) =>
    initFlowSortablePlaceholder(placeholder, "flowpart-widgets"),
);
