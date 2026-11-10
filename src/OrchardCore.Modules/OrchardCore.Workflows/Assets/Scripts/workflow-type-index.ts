import initWorkflowBulkSelectList from "@orchardcore/bloom/components/workflow-bulk-select-list";

const root = document.querySelector<HTMLElement>(".workflow-bulk-select-list");

if (root) {
    initWorkflowBulkSelectList({
        selectedText: root.dataset.selectedText ?? "",
        onDropdownItemClick: (item, setBulkAction) => {
            switch (item.dataset.action) {
                case "Delete":
                    confirmDialog({
                        title: item.dataset.title,
                        message: item.dataset.message,
                        callback: (response: boolean) => {
                            if (response) {
                                setBulkAction(item.dataset.action ?? "");
                            }
                        },
                    });
                    break;
                case "Export":
                    setBulkAction(item.dataset.action ?? "");
                    break;
            }
        },
    });
}
