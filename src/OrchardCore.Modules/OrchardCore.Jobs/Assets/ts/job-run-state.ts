// Keeps the displayed runs up to date. What tells whether the runs changed is polled while the page is visible, and
// the runs that changed are replaced with their fresh rendering, fetched from the same page, so that the scroll
// position, the selection and the other runs are kept. Any server can answer, as the runs are read from the database.

interface JobRunState {
    runId: string;
    signature: string;
    isActive: boolean;
}

interface JobRunStates {
    latest: string | null;
    runs: JobRunState[];
}

// The polling is faster while a run is queued, running or waiting. It keeps that pace for a while, so that a short
// run is shown as ended right after it did, and backs off for a long run.
const activeDelay = 2000;
const activeDelayPolls = 15;
const maxActiveDelay = 10000;
const idleDelay = 15000;

const container = document.querySelector<HTMLElement>("[data-job-runs-states-url]");
const statesUrl = container?.dataset.jobRunsStatesUrl;

const getRunElements = (root: ParentNode) => {
    const elements = new Map<string, HTMLElement>();

    root.querySelectorAll<HTMLElement>("[data-job-run-id]").forEach((element) => {
        if (element.dataset.jobRunId) {
            elements.set(element.dataset.jobRunId, element);
        }
    });

    return elements;
};

const isMenuOpen = (element: ParentNode) => element.querySelector("[aria-expanded='true']") !== null;

const hasSelection = (root: ParentNode) => root.querySelector("input[type='checkbox'][name='runIds']:checked") !== null;

const hasActiveRun = (root: HTMLElement) => Array.from(getRunElements(root).values()).some((element) => element.dataset.jobRunActive === "true");

// A list shows the runs from the most recent one on its first page, when it is not filtered by a status a new run doesn't have.
const showsLatestRuns = () => {
    const parameters = new URLSearchParams(window.location.search);
    const page = parameters.get("pagenum");
    const status = parameters.get("Options.Status")?.toLowerCase() ?? "";

    return (!page || page === "1") && ["", "active", "queued"].includes(status);
};

// Replaces the given runs with their fresh rendering. Returns false when the page needs to be reloaded instead.
const refreshRunsAsync = async (root: HTMLElement, runIds: string[]) => {
    const response = await fetch(window.location.href, {
        headers: { Accept: "text/html", "X-Jobs-Refresh": "true" },
        credentials: "same-origin",
        // The response only holds a part of the page, it must not be used for a navigation.
        cache: "no-store",
    });

    // Ignore a redirection to another page, for instance to the login page when the session expired.
    if (!response.ok || (response.redirected && new URL(response.url).pathname !== window.location.pathname)) {
        return true;
    }

    const freshDocument = new DOMParser().parseFromString(await response.text(), "text/html");
    const currentRuns = getRunElements(root);
    const freshRuns = getRunElements(freshDocument);

    for (const runId of runIds) {
        const current = currentRuns.get(runId);
        const fresh = freshRuns.get(runId);

        if (!current) {
            continue;
        }

        // The run left the page, for instance because of the status filter, or was deleted.
        if (!fresh) {
            return false;
        }

        // Don't replace a run whose actions menu was opened in the meantime: its signature is still outdated, so it is retried later.
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

        // Keep the expanded details, for instance of the error.
        const currentDetails = current.querySelectorAll("details");
        replacement.querySelectorAll("details").forEach((details, index) => {
            details.open = currentDetails[index]?.open ?? details.open;
        });

        current.replaceWith(replacement);
    }

    return true;
};

if (container && statesUrl) {
    const isList = container.matches("form");
    let delay = activeDelay;
    let activePolls = 0;
    let timeout: number | undefined;
    let polling = false;
    let latest: string | null | undefined;

    const schedule = () => {
        window.clearTimeout(timeout);

        if (hasActiveRun(container)) {
            timeout = window.setTimeout(poll, delay);
            activePolls++;

            if (activePolls >= activeDelayPolls) {
                delay = Math.min(delay * 1.5, maxActiveDelay);
            }
        } else {
            delay = activeDelay;
            activePolls = 0;

            // A run that ended only changes when it is deleted; a list may get new runs.
            if (isList) {
                timeout = window.setTimeout(poll, idleDelay);
            }
        }
    };

    const poll = async () => {
        // Don't poll while the page is hidden, it polls again as soon as it is visible.
        if (document.hidden || polling) {
            return;
        }

        polling = true;

        try {
            const currentRuns = getRunElements(container);
            const url = new URL(statesUrl, window.location.href);

            currentRuns.forEach((_, runId) => url.searchParams.append("runIds", runId));

            const response = await fetch(url, {
                headers: { Accept: "application/json" },
                credentials: "same-origin",
                cache: "no-store",
            });

            if (response.ok && response.headers.get("Content-Type")?.includes("json")) {
                const states = (await response.json()) as JobRunStates;
                const known = new Map(states.runs.map((state) => [state.runId, state]));
                const changed: string[] = [];

                currentRuns.forEach((element, runId) => {
                    const state = known.get(runId);

                    // A run that doesn't exist anymore was deleted.
                    if (!state || state.signature !== element.dataset.jobRunSignature) {
                        changed.push(runId);
                    }
                });

                let reload = false;

                if (changed.length > 0) {
                    reload = !(await refreshRunsAsync(container, changed));
                }

                // A run was queued since the list was rendered.
                if (isList && latest !== undefined && states.latest !== latest && states.latest && !currentRuns.has(states.latest) && showsLatestRuns()) {
                    reload = true;
                }

                latest = states.latest;

                if (reload && !hasSelection(container) && !isMenuOpen(container)) {
                    window.location.reload();
                    return;
                }
            }
        } catch {
            // The next polling tries again.
        } finally {
            polling = false;
            schedule();
        }
    };

    document.addEventListener("visibilitychange", () => {
        if (!document.hidden) {
            window.clearTimeout(timeout);
            void poll();
        }
    });

    // The first polling only records the most recent run, to tell the new ones from then on.
    void poll();
}
