document.addEventListener('DOMContentLoaded', () => {
    const generateWorkflowUrl = function () {
        const workflowTypeId = document.querySelector<HTMLElement>('[data-workflow-type-id]')?.dataset.workflowTypeId;
        const activityId = document.querySelector<HTMLElement>('[data-activity-id]')?.dataset.activityId;
        const tokenLifeSpan = document.querySelector<HTMLInputElement>('#token-lifespan')?.value;
        const generateUrlBase = document.querySelector<HTMLElement>('[data-generate-url]')?.dataset.generateUrl;
        const generateUrl = `${generateUrlBase}?workflowTypeId=${workflowTypeId}&activityId=${activityId}&tokenLifeSpan=${tokenLifeSpan}`;
        const antiforgeryHeaderName = document.querySelector<HTMLElement>('[data-antiforgery-header-name]')?.dataset.antiforgeryHeaderName ?? '';
        const antiforgeryToken = document.querySelector<HTMLElement>('[data-antiforgery-token]')?.dataset.antiforgeryToken ?? '';
        const headers: Record<string, string> = {};

        headers[antiforgeryHeaderName] = antiforgeryToken;

        fetch(generateUrl, {
            method: 'POST',
            headers,
        })
            .then((response) => response.text())
            .then((url) => {
                const urlInput = document.getElementById('workflow-url-text') as HTMLInputElement | null;
                if (urlInput) {
                    urlInput.value = url;
                }
            });
    };

    document.getElementById('generate-url-button')?.addEventListener('click', () => {
        generateWorkflowUrl();
    });

    if ((document.getElementById('workflow-url-text') as HTMLInputElement | null)?.value == '') {
        generateWorkflowUrl();
    }
});
