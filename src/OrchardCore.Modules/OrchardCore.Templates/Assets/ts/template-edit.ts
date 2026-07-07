import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";
import { initializeTemplatePreview } from "@orchardcore/bloom/components/template-preview-sync";

const editorElement = document.querySelector<HTMLElement>(".template-editor");
const textArea = document.querySelector<HTMLTextAreaElement>(".template-editor-value");
const nameInput = document.querySelector<HTMLInputElement>(".template-name");

if (editorElement && textArea && nameInput) {
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- Monaco's own AMD/RequireJS loader, not a CommonJS import.
    require(["vs/editor/editor.main"], () => {
        ConfigureLiquidIntellisense(monaco);
        syncMonacoTheme();

        const editor = monaco.editor.create(editorElement, {
            automaticLayout: true,
            language: "liquid",
        });

        const antiforgerytoken = document.querySelector<HTMLInputElement>("[name='__RequestVerificationToken']")?.value ?? "";

        const toQueryString = (data: Record<string, string>) =>
            Object.keys(data)
                .map((key) => `${encodeURIComponent(key)}=${encodeURIComponent(data[key])}`)
                .join("&");

        // Pre-existing: the original called `setTimeout(sendFormData(), delay)` - invoking
        // sendFormData() immediately and passing its (undefined) return value as the timer
        // callback, so the intended 1500ms debounce never actually happened - every content
        // change sends the preview data synchronously with no delay. Preserved as-is (the
        // dead setTimeout/clearTimeout bookkeeping is dropped since it had no observable effect).
        const sendFormData = () => {
            const formData = {
                Name: nameInput.value,
                Content: editor.getValue(),
                __RequestVerificationToken: antiforgerytoken,
            };
            localStorage.setItem("OrchardCore.templates", JSON.stringify(toQueryString(formData)));
        };

        editor.getModel().onDidChangeContent(() => {
            sendFormData();
        });

        editor.getModel().setValue(textArea.value);

        window.addEventListener("submit", () => {
            textArea.value = editor.getValue();
        });

        window.addEventListener("storage", (ev) => {
            if (ev.key !== "OrchardCore.templates:ready") {
                return;
            }

            sendFormData();
        });

        window.addEventListener("unload", () => {
            localStorage.removeItem("OrchardCore.templates");
            localStorage.setItem("OrchardCore.templates:not-connected", "");
            localStorage.removeItem("OrchardCore.templates:not-connected");
        });
    });

    // Pre-existing: listens for "propertychange", a legacy IE-only event that never fires in
    // any modern browser, so this second live-preview hook is effectively dormant - the real
    // one is the debounced (see above) onDidChangeContent wiring inside the require() callback.
    // Preserved rather than "fixed" to "change", which would activate a currently-dead path.
    initializeTemplatePreview(textArea, "propertychange");
}
