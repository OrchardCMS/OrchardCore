import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import initFlowSortablePlaceholder from "./flow-sortable-placeholder";

observeAndInit(".flowpart-sortable-placeholder", (placeholder) =>
    initFlowSortablePlaceholder(placeholder, "flowpart-widgets"),
);
