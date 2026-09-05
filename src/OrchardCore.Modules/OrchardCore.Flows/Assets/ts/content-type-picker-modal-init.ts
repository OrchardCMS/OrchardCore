// Shared by BagPart-Blocks.Edit.cshtml and FlowPart-Blocks.Edit.cshtml - both render this exact
// template-move-to-body dance, guarded server-side by a shared Context.Items key so only one of
// them ever actually emits it per page. A plain top-level call (not observeAndInit) is correct
// here since the server guard already guarantees at most one execution per page.
const initContentTypePickerModal = () => {
    const template = document.getElementById("contentTypePickerModalTemplate") as HTMLTemplateElement | null;
    const pathBase = template?.dataset.pathBase ?? "";

    if (!document.getElementById("contentTypePickerModal")) {
        if (template) {
            document.body.appendChild(template.content.cloneNode(true));
            template.remove();
        }
    } else {
        template?.remove();
    }

    window.initializeContentTypePickerApplication(pathBase);
};

export default initContentTypePickerModal;
