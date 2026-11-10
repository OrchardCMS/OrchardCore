// Wires a bulk-actions dropdown item that, when 2+ items are checked, shows a confirm dialog
// (using the button's own data-title/data-message, same {...this.dataset} spread idiom already
// used elsewhere in this codebase, e.g. ContentsAdminList.cshtml) and opens a target modal once
// confirmed. Shared by every "bulk action opens a deployment modal" button.
const wireBulkActionConfirm = (buttonId: string, modalId: string) => {
    const button = document.getElementById(buttonId);

    button?.addEventListener("click", () => {
        const checkedItemCount = document.querySelectorAll("input[type='checkbox'][name='itemIds']:checked").length;

        if (checkedItemCount <= 1) {
            return;
        }

        confirmDialog({
            ...button.dataset,
            callback: (confirmed) => {
                if (!confirmed) {
                    return;
                }

                const modalElement = document.getElementById(modalId);

                if (modalElement) {
                    new bootstrap.Modal(modalElement).show();
                }
            },
        });
    });
};

export default wireBulkActionConfirm;
