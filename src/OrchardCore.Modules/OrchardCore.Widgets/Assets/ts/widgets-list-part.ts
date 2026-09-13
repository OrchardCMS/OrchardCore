import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

observeAndInit(".widget-template-placeholder-widget-list", (placeholder) =>
    window.initWidgetsListSortable(placeholder.id, "widgetslist-widgets"),
);
