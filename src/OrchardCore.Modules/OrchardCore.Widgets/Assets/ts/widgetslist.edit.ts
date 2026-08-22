import initSortableWidgets from "@orchardcore/bloom/components/sortable-widgets";
import evalScripts from "@orchardcore/bloom/helpers/evalScripts";

// confirmDialog is a global defined by TheAdmin's own script (already loaded on
// every admin page); declared here rather than imported since it isn't bundled.
declare const confirmDialog: (options: Record<string, unknown> & { callback: (response: boolean) => void }) => void;

declare global {
    interface Window {
        initWidgetsListSortable: (containerId: string, groupName: string) => void;
    }
}

function getIndexes(targetId: string, htmlFieldPrefix: string) {
    // Retrieve all index values knowing that some elements may have been moved / removed.
    return Array.from(document.getElementById(targetId)!.closest("form")!.querySelectorAll<HTMLInputElement>("input[name*='Prefixes']"))
        .filter((e) => e.value.substring(0, e.value.lastIndexOf("-")) === htmlFieldPrefix)
        .map((e) => parseInt(e.value.substring(e.value.lastIndexOf("-") + 1)) || 0);
}

document.addEventListener("DOMContentLoaded", () => {
    document.addEventListener("click", (event) => {
        const target = (event.target as Element).closest<HTMLElement>(".add-list-widget");
        if (!target) {
            return;
        }

        const type = target.dataset.widgetType;
        const targetId = target.dataset.targetId!;
        const htmlFieldPrefix = target.dataset.htmlFieldPrefix!;
        const createEditorUrl = document.getElementById(targetId)!.dataset.buildeditorurl;
        const prefixesName = target.dataset.prefixesName;
        const parentContentType = target.dataset.parentContentType;
        const partName = target.dataset.partName;
        const zonesName = target.dataset.zonesName;
        const zone = target.dataset.zone;

        const indexes = getIndexes(targetId, htmlFieldPrefix);

        // Use a prefix based on the items count (not a guid) so that the browser autofill still works.
        const index = indexes.length ? Math.max(...indexes) + 1 : 0;
        const prefix = htmlFieldPrefix + "-" + index.toString();

        const contentTypesName = target.dataset.contenttypesName;
        const contentItemsName = target.dataset.contentitemsName;
        fetch(createEditorUrl + "?id=" + type + "&prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&contentItemsName=" + contentItemsName + "&zonesName=" + zonesName + "&zone=" + zone + "&targetId=" + targetId + "&parentContentType=" + parentContentType + "&partName=" + partName)
            .then((response) => response.text())
            .then((data) => {
                const result = JSON.parse(data);
                document.getElementById(targetId)!.insertAdjacentHTML("beforeend", result.Content);
                evalScripts(result.Scripts);
            });
    });

    document.addEventListener("click", (event) => {
        const target = (event.target as Element).closest<HTMLElement>(".insert-list-widget");
        if (!target) {
            return;
        }

        const type = target.dataset.widgetType;
        const widgetTemplate = target.closest(".widget-template")!;
        const targetId = target.dataset.targetId!;
        const htmlFieldPrefix = target.dataset.htmlFieldPrefix!;
        const createEditorUrl = document.getElementById(targetId)!.dataset.buildeditorurl;
        const prefixesName = target.dataset.prefixesName;
        const parentContentType = target.dataset.parentContentType;
        const partName = target.dataset.partName;
        const zonesName = target.dataset.zonesName;
        const zone = target.dataset.zone;

        const indexes = getIndexes(targetId, htmlFieldPrefix);

        // Use a prefix based on the items count (not a guid) so that the browser autofill still works.
        const index = indexes.length ? Math.max(...indexes) + 1 : 0;
        const prefix = htmlFieldPrefix + "-" + index.toString();

        const contentTypesName = target.dataset.contenttypesName;
        const contentItemsName = target.dataset.contentitemsName;
        fetch(createEditorUrl + "?id=" + type + "&prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&contentItemsName=" + contentItemsName + "&zonesName=" + zonesName + "&zone=" + zone + "&targetId=" + targetId + "&parentContentType=" + parentContentType + "&partName=" + partName)
            .then((response) => response.text())
            .then((data) => {
                const result = JSON.parse(data);
                widgetTemplate.insertAdjacentHTML("beforebegin", result.Content);
                evalScripts(result.Scripts);
            });
    });

    document.addEventListener("click", (event) => {
        const target = (event.target as Element).closest<HTMLElement>(".widget-list-delete");
        if (!target) {
            return;
        }

        confirmDialog({
            ...target.dataset,
            callback: (r: boolean) => {
                if (r) {
                    target.closest(".widget-template")!.remove();
                    document.dispatchEvent(new CustomEvent("contentpreview:render"));
                }
            },
        });
    });

    document.addEventListener("change", (event) => {
        if ((event.target as Element).closest(".widget-editor-footer label")) {
            document.dispatchEvent(new CustomEvent("contentpreview:render"));
        }
    });

    document.addEventListener("click", (event) => {
        const target = (event.target as Element).closest<HTMLElement>(".widget-list-editor-btn-toggle");
        if (target) {
            target.closest(".widget-editor")!.classList.toggle("collapsed");
        }
    });
});

// Called from an inline <script> block in WidgetsListPart.Edit.cshtml, once per
// zone, with a per-render container id that only the server knows. Unlike Flows'
// widgets, any widget can move into any zone here - there's no content-type
// gating - and moving between zones just needs the widget's own hidden "zone"
// tracking field updated, not a GUID-based form field id/name rewrite.
window.initWidgetsListSortable = function initWidgetsListSortable(containerId: string, groupName: string) {
    initSortableWidgets(containerId, {
        groupName,
        renderOnAnyDrop: true,
        onReparented: (item, _from, to) => {
            const newZone = to.closest<HTMLElement>(".widget-editor-body")?.dataset.zone;
            const zoneInput = item.querySelector<HTMLInputElement>(".source-zone");

            if (zoneInput && newZone != null) {
                zoneInput.value = newZone;
            }
        },
    });
};
