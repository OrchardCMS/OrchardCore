// Keeps the description and the next occurrences of a cron expression editor in step with what is typed. They come
// from the server, which describes the expression with the parser that evaluates the schedules.

interface CronExpressionDescription {
    isValid: boolean;
    description?: string | null;
    error?: string | null;
    occurrences?: string[];
    timeZone?: string;
}

const typingDelay = 300;

function initializeCronExpressionEditor(editor: HTMLElement) {
    if (editor.dataset.cronInitialized === "true") {
        return;
    }

    editor.dataset.cronInitialized = "true";

    const input = editor.querySelector<HTMLInputElement>("[data-cron-input]");
    const description = editor.querySelector<HTMLElement>("[data-cron-description]");
    const occurrences = editor.querySelector<HTMLElement>("[data-cron-occurrences]");
    const occurrencesLabel = editor.querySelector<HTMLElement>("[data-cron-occurrences-label]");
    const occurrencesList = editor.querySelector<HTMLElement>("[data-cron-occurrences-list]");
    const guru = editor.querySelector<HTMLAnchorElement>("[data-cron-guru]");
    const describeUrl = editor.dataset.cronDescribeUrl;

    if (!input || !description || !occurrences || !occurrencesLabel || !occurrencesList || !describeUrl) {
        return;
    }

    // An expression that applies to UTC applies to the time zone of the site when its checkbox is checked.
    const toggleId = editor.dataset.cronSiteTimeZoneToggle;
    const toggle = toggleId ? (document.getElementById(toggleId) as HTMLInputElement | null) : null;
    const isUtc = () => editor.dataset.cronUtc === "true" && !toggle?.checked;

    let timer: number | undefined;
    let request: AbortController | undefined;

    const updateGuruLink = (expression: string) => {
        if (guru) {
            // crontab.guru reads the expression from the fragment, its fields separated by underscores.
            guru.href = expression ? `https://crontab.guru/#${encodeURI(expression.split(/\s+/).join("_"))}` : "https://crontab.guru/";
        }
    };

    const show = (result: CronExpressionDescription | null, showError: boolean) => {
        const isInvalid = result !== null && !result.isValid;

        description.textContent = isInvalid ? (showError ? result?.error ?? "" : "") : result?.description ?? "";
        description.classList.toggle("text-danger", isInvalid && showError);

        occurrencesList.replaceChildren();

        const dates = result?.isValid ? result.occurrences ?? [] : [];

        for (const date of dates) {
            const item = document.createElement("li");
            item.textContent = date;
            occurrencesList.appendChild(item);
        }

        // A valid expression may never occur, such as one for February 31.
        occurrencesLabel.textContent = dates.length === 0
            ? editor.dataset.cronNeverText ?? ""
            : (editor.dataset.cronOccurrencesText ?? "").replace("{0}", result?.timeZone ?? "");
        occurrences.hidden = !result?.isValid;
    };

    const refresh = async (showError: boolean) => {
        const expression = input.value.trim();

        updateGuruLink(expression);
        request?.abort();

        if (!expression) {
            show(null, false);
            return;
        }

        request = new AbortController();

        const url = new URL(describeUrl, window.location.href);
        url.searchParams.set("expression", expression);
        url.searchParams.set("utc", isUtc() ? "true" : "false");

        try {
            const response = await fetch(url, {
                credentials: "same-origin",
                headers: { Accept: "application/json" },
                signal: request.signal,
            });

            if (response.ok) {
                show((await response.json()) as CronExpressionDescription, showError);
            }
        } catch {
            // The request was replaced by a more recent one, or the server can't be reached: the description that
            // is shown stays, and the expression is validated when the form is submitted.
        }
    };

    input.addEventListener("input", () => {
        window.clearTimeout(timer);
        timer = window.setTimeout(() => void refresh(true), typingDelay);
    });

    toggle?.addEventListener("change", () => void refresh(true));

    editor.querySelectorAll<HTMLElement>("[data-cron-preset]").forEach((preset) => {
        preset.addEventListener("click", (event) => {
            event.preventDefault();
            input.value = preset.dataset.cronPreset ?? "";
            window.clearTimeout(timer);
            input.dispatchEvent(new Event("change", { bubbles: true }));
            input.focus();
            void refresh(true);
        });
    });

    // The server validates the value the page was rendered with, so its error is not shown twice.
    void refresh(false);
}

function initializeCronExpressionEditors(root: ParentNode) {
    root.querySelectorAll<HTMLElement>("[data-cron-editor]").forEach(initializeCronExpressionEditor);
}

initializeCronExpressionEditors(document);

// An editor can be added to the page after it loaded, as the editor of an activity in the workflow designer.
new MutationObserver((mutations) => {
    for (const mutation of mutations) {
        mutation.addedNodes.forEach((node) => {
            if (node instanceof HTMLElement) {
                if (node.matches("[data-cron-editor]")) {
                    initializeCronExpressionEditor(node);
                }

                initializeCronExpressionEditors(node);
            }
        });
    }
}).observe(document.body, { childList: true, subtree: true });
