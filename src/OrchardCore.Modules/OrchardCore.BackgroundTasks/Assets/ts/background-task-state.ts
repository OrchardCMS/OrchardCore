// Keeps the execution state of the displayed tasks up to date. The status of the tasks is polled while the page
// is visible, and the tasks whose status or number of runs changed are replaced with their fresh rendering,
// fetched from the same page, so that the scroll position, the selection and the other tasks are kept.

interface BackgroundTaskStatusResponse {
    name: string;
    status: string;
    runCount: number;
}

// The polling is faster while a task is queued or running, and backs off for a long running task.
const activeDelay = 2000;
const maxActiveDelay = 10000;
const idleDelay = 10000;

// The polling only follows the server that rendered the page, as the execution state is kept in memory by each server.
// The page is reloaded when that server no longer answers, for instance because it restarted with a new identifier.
const maxOtherServerPolls = 30;

const container = document.querySelector<HTMLElement>("[data-background-task-states-url]");
const statesUrl = container?.dataset.backgroundTaskStatesUrl;
const serverId = container?.dataset.backgroundTaskServer;

const getSignature = (status: string | undefined, runCount: number | string | undefined) => `${status ?? ""}|${runCount ?? ""}`;

const getTaskElements = (root: ParentNode) => {
    const elements = new Map<string, HTMLElement>();

    root.querySelectorAll<HTMLElement>("[data-task-name]").forEach((element) => {
        if (element.dataset.taskName) {
            elements.set(element.dataset.taskName, element);
        }
    });

    return elements;
};

const isMenuOpen = (element: HTMLElement) => element.querySelector("[aria-expanded='true']") !== null;

// Whether the list only shows the tasks in a given execution state, so that the other tasks can enter it.
const isFilteredByState = () => ["running", "failed"].includes(new URLSearchParams(window.location.search).get("Options.Status")?.toLowerCase() ?? "");

const hasPendingTask = (root: HTMLElement) =>
    Array.from(getTaskElements(root).values()).some((element) => element.dataset.taskStatus === "Queued" || element.dataset.taskStatus === "Running");

// Replaces the given tasks with their fresh rendering. Returns false when the page needs to be reloaded instead.
const refreshTasksAsync = async (root: HTMLElement, names: string[], addedNames: string[]) => {
    const response = await fetch(window.location.href, {
        headers: { Accept: "text/html", "X-Background-Tasks-Refresh": "true" },
        credentials: "same-origin",
        // The response only holds a part of the page, it must not be used for a navigation.
        cache: "no-store",
    });

    // Ignore a redirection to another page, for instance to the login page when the session expired.
    if (!response.ok || (response.redirected && new URL(response.url).pathname !== window.location.pathname)) {
        return true;
    }

    const freshDocument = new DOMParser().parseFromString(await response.text(), "text/html");

    // Another server answered, the next polling tries again as the signatures are still outdated.
    if (freshDocument.querySelector<HTMLElement>("[data-background-task-server]")?.dataset.backgroundTaskServer !== serverId) {
        return true;
    }

    const currentTasks = getTaskElements(root);
    const freshTasks = getTaskElements(freshDocument);

    // A task entered the list filtered by state.
    if (addedNames.some((name) => freshTasks.has(name))) {
        return false;
    }

    for (const name of names) {
        const current = currentTasks.get(name);
        const fresh = freshTasks.get(name);

        if (!current) {
            continue;
        }

        // The task left the page, for instance because of the status filter.
        if (!fresh) {
            return false;
        }

        // Don't replace a task whose actions menu was opened in the meantime, its signature is still outdated so it is retried later.
        if (isMenuOpen(current)) {
            continue;
        }

        const replacement = document.importNode(fresh, true);

        // Keep the selection checkbox, which holds the listeners of the list management script and the selection.
        const currentCheckbox = current.querySelector<HTMLInputElement>("input[type='checkbox'][name]");
        const replacementCheckbox = replacement.querySelector<HTMLInputElement>("input[type='checkbox'][name]");
        if (currentCheckbox && replacementCheckbox) {
            replacementCheckbox.replaceWith(currentCheckbox);
        }

        // Keep the expanded details, for instance of the last error.
        const currentDetails = current.querySelectorAll("details");
        replacement.querySelectorAll("details").forEach((details, index) => {
            details.open = currentDetails[index]?.open ?? details.open;
        });

        current.replaceWith(replacement);
    }

    return true;
};

if (container && statesUrl) {
    let delay = activeDelay;
    let timeout: number | undefined;
    let polling = false;
    let otherServerPolls = 0;
    const otherTaskSignatures = new Map<string, string>();

    const schedule = () => {
        window.clearTimeout(timeout);

        if (hasPendingTask(container)) {
            timeout = window.setTimeout(poll, delay);
            delay = Math.min(delay * 1.5, maxActiveDelay);
        } else {
            delay = activeDelay;
            timeout = window.setTimeout(poll, idleDelay);
        }
    };

    const poll = async () => {
        // Don't poll while the page is hidden, it polls again as soon as it is visible.
        if (document.hidden || polling) {
            return;
        }

        polling = true;

        try {
            const response = await fetch(statesUrl, {
                headers: { Accept: "application/json" },
                credentials: "same-origin",
            });

            const respondingServerId = response.headers.get("X-Background-Tasks-Server");

            if (respondingServerId && respondingServerId !== serverId) {
                otherServerPolls++;

                if (otherServerPolls >= maxOtherServerPolls) {
                    window.location.reload();
                    return;
                }
            } else if (response.ok) {
                otherServerPolls = 0;

                const states = (await response.json()) as BackgroundTaskStatusResponse[];
                const currentTasks = getTaskElements(container);

                const changedNames: string[] = [];
                const addedNames: string[] = [];

                for (const state of states) {
                    const signature = getSignature(state.status, state.runCount);
                    const element = currentTasks.get(state.name);

                    if (!element) {
                        const previous = otherTaskSignatures.get(state.name);
                        otherTaskSignatures.set(state.name, signature);

                        if (previous !== undefined && previous !== signature && isFilteredByState()) {
                            addedNames.push(state.name);
                        }
                    } else if (!isMenuOpen(element) && getSignature(element.dataset.taskStatus, element.dataset.taskRunCount) !== signature) {
                        changedNames.push(state.name);
                    }
                }

                if ((changedNames.length > 0 || addedNames.length > 0) && !(await refreshTasksAsync(container, changedNames, addedNames))) {
                    window.location.reload();
                    return;
                }
            }
        } catch {
            // Ignore transient errors, the next poll tries again.
        } finally {
            polling = false;
        }

        schedule();
    };

    document.addEventListener("visibilitychange", () => {
        if (!document.hidden) {
            delay = activeDelay;
            poll();
        }
    });

    // Poll right away, to know the state of the tasks that are not on the page from now on.
    poll();
}
