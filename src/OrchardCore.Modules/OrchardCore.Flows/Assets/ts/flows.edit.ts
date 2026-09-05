import initSortableWidgets from "@orchardcore/bloom/components/sortable-widgets";
import evalScripts from "@orchardcore/bloom/helpers/evalScripts";

// confirmDialog is a global defined by TheAdmin's own script (already loaded on
// every admin page); declared here rather than imported since it isn't bundled.
declare const confirmDialog: (options: Record<string, unknown> & { callback: (response: boolean) => void }) => void;

declare global {
    interface Window {
        toggleWidgets: () => void;
        initFlowSortableWidgets: (containerId: string, typesDatasetKey: string, groupName: string, partName: string) => void;
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
        const target = (event.target as Element).closest<HTMLElement>(".add-widget");
        if (!target) {
            return;
        }

        const type = target.dataset.widgetType;
        const targetId = target.dataset.targetId!;
        const htmlFieldPrefix = target.dataset.htmlFieldPrefix!;
        const createEditorUrl = document.getElementById(targetId)!.dataset.buildeditorurl;
        const prefixesName = target.dataset.prefixesName;
        const flowmetadata = target.dataset.flowmetadata;
        const parentContentType = target.dataset.parentContentType;
        const partName = target.dataset.partName;

        const indexes = getIndexes(targetId, htmlFieldPrefix);

        // Use a prefix based on the items count (not a guid) so that the browser autofill still works.
        const index = indexes.length ? Math.max(...indexes) + 1 : 0;
        const prefix = htmlFieldPrefix + "-" + index.toString();

        const contentTypesName = target.dataset.contenttypesName;
        const contentItemsName = target.dataset.contentitemsName;
        fetch(createEditorUrl + "?id=" + type + "&prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&contentItemsName=" + contentItemsName + "&targetId=" + targetId + "&flowmetadata=" + flowmetadata + "&parentContentType=" + parentContentType + "&partName=" + partName)
            .then((response) => response.text())
            .then((data) => {
                const result = JSON.parse(data);
                document.getElementById(targetId)!.insertAdjacentHTML("beforeend", result.Content);
                evalScripts(result.Scripts);
            });
    });

    document.addEventListener("click", (event) => {
        const target = (event.target as Element).closest<HTMLElement>(".insert-widget");
        if (!target) {
            return;
        }

        const type = target.dataset.widgetType;
        const widgetTemplate = target.closest(".widget-template")!;
        const targetId = target.dataset.targetId!;
        const htmlFieldPrefix = target.dataset.htmlFieldPrefix!;
        const createEditorUrl = document.getElementById(targetId)!.dataset.buildeditorurl;
        const flowmetadata = target.dataset.flowmetadata;
        const prefixesName = target.dataset.prefixesName;
        const parentContentType = target.dataset.parentContentType;
        const partName = target.dataset.partName;

        const indexes = getIndexes(targetId, htmlFieldPrefix);

        // Use a prefix based on the items count (not a guid) so that the browser autofill still works.
        const index = indexes.length ? Math.max(...indexes) + 1 : 0;
        const prefix = htmlFieldPrefix + "-" + index.toString();

        const contentTypesName = target.dataset.contenttypesName;
        const contentItemsName = target.dataset.contentitemsName;
        fetch(createEditorUrl + "?id=" + type + "&prefix=" + prefix + "&prefixesName=" + prefixesName + "&contentTypesName=" + contentTypesName + "&contentItemsName=" + contentItemsName + "&targetId=" + targetId + "&flowmetadata=" + flowmetadata + "&parentContentType=" + parentContentType + "&partName=" + partName)
            .then((response) => response.text())
            .then((data) => {
                const result = JSON.parse(data);
                widgetTemplate.insertAdjacentHTML("beforebegin", result.Content);
                evalScripts(result.Scripts);
            });
    });

    document.addEventListener("click", (event) => {
        const target = (event.target as Element).closest<HTMLElement>(".widget-delete");
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
        const target = (event.target as Element).closest<HTMLElement>(".widget-editor-footer label, .widget-editor-header label");
        if (!target) {
            return;
        }

        const tmpl = target.closest(".widget-template")!;
        const radio = target.querySelector<HTMLInputElement>("input:first-child")!;
        if (radio.id !== "undefined" && radio.id.indexOf("Size") > 0) {
            const radioSize = radio.value;
            Array.from(tmpl.classList).forEach((item) => {
                if (item.indexOf("col-md-") === 0) tmpl.classList.remove(item);
            });
            const colSize = Math.round(Number(radioSize) / 100 * 12);
            tmpl.classList.add("col-md-" + colSize);

            const dropdown = target.closest(".dropdown-menu")!;
            dropdown.previousElementSibling!.textContent = radioSize + "%";
        } else if (radio.id !== "undefined" && radio.id.indexOf("Alignment") > 0) {
            const svg = target.querySelector("svg")!.outerHTML;
            const alignDropdown = target.closest(".dropdown-menu")!;
            alignDropdown.previousElementSibling!.innerHTML = svg;
        }

        target.parentElement!.querySelectorAll(".dropdown-item").forEach((item) => item.classList.remove("active"));
        target.classList.toggle("active");
        document.dispatchEvent(new CustomEvent("contentpreview:render"));
    });

    document.addEventListener("click", (event) => {
        const target = (event.target as Element).closest<HTMLElement>(".widget-editor-btn-toggle");
        if (target) {
            target.closest(".widget-editor")!.classList.toggle("collapsed");
        }
    });

    document.addEventListener("keyup", (event) => {
        const target = (event.target as Element).closest<HTMLInputElement>(".widget-editor-body .form-group input.content-caption-text");
        if (!target) {
            return;
        }

        const firstHeader = target.closest(".widget-editor")!.querySelector(".widget-editor-header");
        const headerTextLabel = firstHeader ? firstHeader.querySelector<HTMLElement>(".widget-editor-header-text") : null;
        const contentTypeDisplayText = headerTextLabel?.dataset.contentTypeDisplayText;
        const title = target.value;
        const newDisplayText = title + " " + contentTypeDisplayText;

        if (headerTextLabel) {
            headerTextLabel.textContent = newDisplayText;
        }
    });
});

// Called from a plain onclick="toggleWidgets();" attribute on the "toggle all
// widgets" button rendered by both ContentCard-FlowPart.Edit and
// ContentCard-FlowPartBlocks.Edit, so it needs to stay a real global.
window.toggleWidgets = function toggleWidgets() {
    const dots = document.querySelectorAll<HTMLElement>(".dot");
    const newIcon = dots[0]?.getAttribute("data-icon") === "angles-up" ? "angles-down" : "angles-up";

    dots.forEach((icon) => icon.setAttribute("data-icon", newIcon));

    document.querySelectorAll(".widget.widget-editor.card").forEach((card) => card.classList.toggle("collapsed"));
};

// Widget form fields are named/id'd with a GUID-based prefix scoped to whichever
// container (possibly a nested Flow/Bag inside another widget) they were originally
// rendered under. Moving a widget to a different container needs those prefixes
// rewritten so ASP.NET Core's model binder maps the posted fields back onto the
// right collection.
function retargetFieldNames(item: HTMLElement, sourceId: string, destinationId: string, partName: string) {
    const inputs = Array.from(item.querySelectorAll<HTMLElement>(`:scope > input[name*='${partName}']`));
    const destinationExists = document.getElementById(destinationId) != null;
    const sourceExists = document.getElementById(sourceId) != null;
    const sourceGuid = sourceId.substring(0, sourceId.lastIndexOf("_") + 1);
    const sourceNameGuid = sourceGuid.split("_").join(".");

    if (destinationExists) {
        const destGuid = destinationId.substring(0, destinationId.lastIndexOf("_") + 1);
        const destNameGuid = destGuid.split("_").join(".");

        inputs.forEach((input) => {
            if (input.id) {
                input.id = sourceExists ? input.id.replace(sourceGuid, destGuid) : destGuid + input.id;
            }

            const name = input.getAttribute("name");

            if (name) {
                input.setAttribute("name", sourceExists ? name.replace(sourceNameGuid, destNameGuid) : destNameGuid + name);
            }
        });
    } else if (sourceExists) {
        inputs.forEach((input) => {
            if (input.id) {
                input.id = input.id.replace(sourceGuid, "");
            }

            const name = input.getAttribute("name");

            if (name) {
                input.setAttribute("name", name.replace(sourceNameGuid, ""));
            }
        });
    }
}

// Called from inline <script> blocks in FlowPart.Edit.cshtml, FlowPart-Blocks.Edit.cshtml,
// BagPart.Edit.cshtml and BagPart-Blocks.Edit.cshtml with per-render values (container id,
// dataset key used for type-gating, connected group, part name) that only the server knows.
window.initFlowSortableWidgets = function initFlowSortableWidgets(
    containerId: string,
    typesDatasetKey: string,
    groupName: string,
    partName: string,
) {
    initSortableWidgets(containerId, {
        groupName,
        accepts: (target, dragged) => {
            const acceptedTypes = (target.dataset[typesDatasetKey] ?? "").split(";");
            const contentType = dragged.querySelector<HTMLElement>(".card")?.dataset.contentType;

            return contentType != null && acceptedTypes.includes(contentType);
        },
        onReparented: (item, from, to) => retargetFieldNames(item, from.id, to.id, partName),
    });
};
