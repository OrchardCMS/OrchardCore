import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { dispatchFieldChange } from "@orchardcore/bloom/helpers/editorLifecycle";

// Initializes the "Regenerate" button of every HttpRequestEvent editor, including editors injected after
// the page loaded (the workflow designer panel). The workflow type and activity come from the closest
// wrapper carrying data-workflow-type-id / data-activity-id, not from the first one on the page.
const initWorkflowUrlGenerator = (button: HTMLElement) => {
    const root = button.closest<HTMLElement>("[data-workflow-type-id]") ?? document.body;
    const activityRoot = button.closest<HTMLElement>("[data-activity-id]") ?? root;
    const urlInput = root.querySelector<HTMLInputElement>("#workflow-url-text");
    const tokenLifeSpanInput = root.querySelector<HTMLInputElement>("#token-lifespan");

    const generateWorkflowUrl = async () => {
        const query = new URLSearchParams({
            workflowTypeId: root.dataset.workflowTypeId ?? "",
            activityId: activityRoot.dataset.activityId ?? "",
            tokenLifeSpan: tokenLifeSpanInput?.value ?? "0",
        });

        const headers: Record<string, string> = {};
        headers[button.dataset.antiforgeryHeaderName ?? ""] = button.dataset.antiforgeryToken ?? "";

        const response = await fetch(`${button.dataset.generateUrl}?${query}`, { method: "POST", headers });

        if (!response.ok || !urlInput) {
            return;
        }

        urlInput.value = await response.text();
        dispatchFieldChange(urlInput);
    };

    button.addEventListener("click", () => {
        generateWorkflowUrl().catch((error: unknown) => console.error(error));
    });

    if (urlInput?.value === "") {
        generateWorkflowUrl().catch((error: unknown) => console.error(error));
    }
};

observeAndInit("[data-generate-url]", initWorkflowUrlGenerator);
