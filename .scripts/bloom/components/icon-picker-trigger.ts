// Shared by every AdminMenu tree-node editor with an icon picker (LinkAdminNode, PlaceholderAdminNode,
// ContentTypesAdminNode, ListsAdminNode .Fields.TreeEdit.cshtml) - wires the "Pick"/remove-icon buttons
// to the page-wide iconPickerVue modal. All 4 views render at most once per page (full-page navigation,
// never AJAX-injected), so a plain global querySelectorAll scan is safe - no observeAndInit needed.
const initIconPickerTriggers = () => {
    document.querySelectorAll<HTMLElement>(".icon-picker-trigger").forEach((trigger) => {
        trigger.addEventListener("click", (e) => {
            const node = (e.currentTarget as HTMLElement).dataset.relatedNode;
            if (node) {
                iconPickerVue.show(node, `sample-icon-${node}`);
            }
        });
    });

    document.querySelectorAll<HTMLElement>("button.remove-icon").forEach((button) => {
        button.addEventListener("click", (e) => {
            const node = (e.currentTarget as HTMLElement).dataset.relatedNode;
            if (!node) {
                return;
            }

            const input = document.getElementById(node) as HTMLInputElement | null;
            if (input) {
                input.value = "";
            }

            const sampleIcon = document.getElementById(`sample-icon-${node}`);
            if (sampleIcon) {
                // Changing the class alone is not enough, the icon needs to be recreated.
                sampleIcon.outerHTML = `<i id="sample-icon-${node}" class=" "></i>`;
            }
        });
    });
};

export default initIconPickerTriggers;
