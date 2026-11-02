export interface ConfirmOptions {
    title: string;
    message: string;
    okText: string;
    cancelText: string;
    okClass?: string;
}

type ConfirmDialog = (options: Record<string, unknown> & { callback: (response: boolean) => void }) => void;

const escapeHtml = (value: string) =>
    value.replace(/[&<>"']/g, (character) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[character]!);

/**
 * Asks for confirmation with the admin theme's dialog (window.confirmDialog), falling back to the browser's.
 * The theme dialog renders its text as HTML, so the text is escaped here.
 */
export const confirmAction = (options: ConfirmOptions): Promise<boolean> =>
    new Promise((resolve) => {
        const dialog = (globalThis as { confirmDialog?: ConfirmDialog }).confirmDialog;

        if (typeof dialog === "function") {
            dialog({
                title: escapeHtml(options.title),
                message: escapeHtml(options.message),
                okText: escapeHtml(options.okText),
                cancelText: escapeHtml(options.cancelText),
                okClass: options.okClass ?? "btn-danger",
                cancelClass: "btn-secondary",
                callback: resolve,
            });
        } else {
            resolve(window.confirm(options.message));
        }
    });
