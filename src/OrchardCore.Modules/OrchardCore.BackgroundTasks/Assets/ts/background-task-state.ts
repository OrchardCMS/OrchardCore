// Polls the execution status of the tasks shown as queued or running, and reloads the page as soon as
// one of them changes, so that the page shows the outcome of the run without a manual refresh.

interface BackgroundTaskStatusResponse {
    name: string;
    status: string;
}

const initialDelay = 2000;
const maxDelay = 30000;

const container = document.querySelector<HTMLElement>("[data-background-task-states-url]");
const statesUrl = container?.dataset.backgroundTaskStatesUrl;

if (container && statesUrl) {
    const pendingTasks = new Map<string, string>();

    container.querySelectorAll<HTMLElement>("[data-task-name][data-task-status]").forEach((element) => {
        const name = element.dataset.taskName;
        const status = element.dataset.taskStatus;

        if (name && (status === "Queued" || status === "Running")) {
            pendingTasks.set(name, status);
        }
    });

    if (pendingTasks.size > 0) {
        let delay = initialDelay;

        const poll = async () => {
            try {
                const response = await fetch(statesUrl, {
                    headers: { Accept: "application/json" },
                    credentials: "same-origin",
                });

                if (response.ok) {
                    const states = (await response.json()) as BackgroundTaskStatusResponse[];
                    const hasChanged = states.some((state) => pendingTasks.has(state.name) && pendingTasks.get(state.name) !== state.status);

                    if (hasChanged) {
                        window.location.reload();
                        return;
                    }
                }
            } catch {
                // Ignore transient errors, the next poll tries again.
            }

            // Back off for long running tasks.
            delay = Math.min(delay * 1.5, maxDelay);
            window.setTimeout(poll, delay);
        };

        window.setTimeout(poll, delay);
    }
}
