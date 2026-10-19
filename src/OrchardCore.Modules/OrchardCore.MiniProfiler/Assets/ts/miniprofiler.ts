// Makes the MiniProfiler results easier to read. On admin pages, also docks the widget to the middle of the right edge,
// behind a toggle that tucks it away, and follows the admin's light and dark themes.
interface MiniProfilerWindow extends Window {
    MiniProfiler?: {
        options?: {
            colorScheme?: string;
        };
    };
}

interface QueryFlags {
    duplicate: boolean;
    errored: boolean;
}

const storageKey = "OrchardCore.MiniProfiler.Collapsed";
const root = document.documentElement;

// The localized texts, rendered by the MiniProfiler shape.
const labels: DOMStringMap = document.getElementById("oc-mp-labels")?.dataset ?? {};

const getScheme = () => (root.getAttribute("data-bs-theme") === "dark" ? "Dark" : "Light");

const isCollapsed = () => {
    try {
        return localStorage.getItem(storageKey) === "true";
    } catch {
        return false;
    }
};

const setCollapsed = (container: HTMLElement, toggle: HTMLButtonElement, collapsed: boolean) => {
    const label = (collapsed ? toggle.dataset.showLabel : toggle.dataset.hideLabel) ?? "";

    container.classList.toggle("oc-mp-collapsed", collapsed);
    toggle.textContent = collapsed ? "‹" : "›";
    toggle.title = label;
    toggle.setAttribute("aria-label", label);
    toggle.setAttribute("aria-expanded", String(!collapsed));
};

const applyScheme = (container: HTMLElement) => {
    const scheme = getScheme();

    container.classList.remove("mp-scheme-auto", "mp-scheme-light", "mp-scheme-dark");
    container.classList.add(`mp-scheme-${scheme.toLowerCase()}`);

    // The SQL overlay takes its scheme from the options when it opens.
    const options = (window as MiniProfilerWindow).MiniProfiler?.options;

    if (options) {
        options.colorScheme = scheme;
    }
};

const showWarning = (container: HTMLElement, toggle: HTMLButtonElement) => {
    toggle.classList.toggle("oc-mp-warning", container.querySelector(".mp-button-warning") !== null);
};

// Replaces the "{0}", "{1}"... placeholders of a text with values, which can be elements.
const formatNodes = (text: string | undefined, ...values: (string | Node)[]) =>
    (text ?? "").split(/\{(\d+)\}/).map((part, index) => (index % 2 === 1 ? (values[Number(part)] ?? "") : part));

const format = (text: string | undefined, ...values: string[]) => formatNodes(text, ...values).join("");

const createElement = (tagName: string, className: string, ...children: (string | Node)[]) => {
    const element = document.createElement(tagName);

    element.className = className;
    element.append(...children);

    return element;
};

// Timing names look like "Controller Action: ...", "Shape: ..." or "View: ...".
const splitKind = (name: string) => {
    const separator = name.indexOf(": ");

    return separator > 0 ? { kind: name.substring(0, separator), name: name.substring(separator + 2) } : { kind: "", name };
};

// Lets long type names and paths wrap after their dots and slashes, rather than anywhere.
const createBreakableText = (text: string) => {
    const fragment = document.createDocumentFragment();

    text.split(/(?<=[./])/).forEach((part, index) => {
        if (index > 0) {
            fragment.append(document.createElement("wbr"));
        }

        fragment.append(part);
    });

    return fragment;
};

const createStep = (step: string) => {
    const { kind, name } = splitKind(step);
    const element = createElement("span", "oc-mp-step-name", createBreakableText(name));

    if (kind) {
        element.prepend(createElement("span", "oc-mp-kind", `${kind}: `));
    }

    return element;
};

// Split off the kind of each timing and tag its row with it, so that the pipeline steps can stand out from the shapes
// and views they render.
const styleTimings = (result: Element) => {
    result.querySelectorAll<HTMLElement>(".mp-popup .mp-timings tr[data-timing-id] .mp-label").forEach((label) => {
        const { kind, name } = splitKind((label.textContent ?? "").trim());
        const row = label.parentElement;

        if (kind === "Shape") {
            row?.classList.add("oc-mp-shape");
        } else if (kind === "View") {
            row?.classList.add("oc-mp-view");
        } else {
            row?.classList.add("oc-mp-step");
        }

        if (!kind) {
            return;
        }

        const nameElement = createElement("span", "oc-mp-name", name);

        // "/Areas/{module}/Views/{file}" mostly repeats itself; the full path stays in the tooltip.
        const view = kind === "View" ? /^\/Areas\/([^/]+)\/Views\/(.+)$/.exec(name) : null;

        if (view) {
            nameElement.replaceChildren(createElement("span", "oc-mp-kind", `${view[1]}/`), view[2]);
        }

        label.replaceChildren(createElement("span", "oc-mp-kind", `${kind}: `), nameElement);
    });
};

const createBadge = (text: string | undefined, hint: string) => {
    const badge = createElement("span", "oc-mp-badge", text ?? "");

    badge.title = hint;

    return badge;
};

const createFact = (label: string | undefined, ...value: (string | Node)[]) => [
    createElement("span", "oc-mp-fact-label", label ?? ""),
    createElement("span", "oc-mp-fact-value", ...value),
];

// A gap row tells how long the request ran between two queries, and which step spent most of that time.
const styleGap = (row: HTMLTableRowElement) => {
    const [durationCell, reasonCell] = Array.from(row.cells);
    const duration = /[\d.,]+/.exec(durationCell?.textContent ?? "");
    const reason = /^(.*?)\s+—\s+([\d.,]+)\s*ms$/s.exec((reasonCell?.textContent ?? "").trim());

    if (!duration || !reason) {
        return;
    }

    durationCell.replaceChildren(format(labels.gap, duration[0]));
    reasonCell.replaceChildren(createElement("div", "oc-mp-gap-reason", ...formatNodes(labels.gapReason, createStep(reason[1]), reason[2])));
};

// Reworks the queries view, which MiniProfiler copies into an overlay when a query count is clicked: number the
// queries in the order they ran, label their timings, and explain the duplicate and failing commands with badges
// instead of a bare "!". Returns the timings that run such commands, by id.
const styleQueries = (result: Element) => {
    const flaggedTimings = new Map<string, QueryFlags>();
    const queries = result.querySelector<HTMLElement>(".mp-queries");

    if (!queries) {
        return flaggedTimings;
    }

    queries.classList.add("oc-mp-queries");
    queries
        .querySelector("thead th")
        ?.replaceChildren(createElement("div", "oc-mp-heading", labels.query ?? ""), createElement("div", "oc-mp-hint", labels.orderHint ?? ""));

    const commands = new Map<string, number>();
    let number = 0;

    queries.querySelectorAll<HTMLTableRowElement>("tbody > tr").forEach((row) => {
        if (row.classList.contains("mp-gap-info")) {
            styleGap(row);

            return;
        }

        const cell = row.cells[0];
        const [callTypeElement, stepElement, timingElement, firstResultElement] = Array.from(cell?.children ?? []);
        const timing = /([\d.,]+)\s*ms\s*\(T\+([\d.,]+)\s*ms\)/.exec(timingElement?.textContent ?? "");

        if (!callTypeElement || !stepElement || !timing) {
            return;
        }

        number++;

        const command = row.querySelector("pre code")?.textContent ?? "";
        const firstNumber = commands.get(command);
        const errored = callTypeElement.classList.contains("mp-warning");

        // MiniProfiler shows the same "!" for duplicate and failing commands.
        const duplicate = callTypeElement.querySelector(".mp-warning") !== null && (!errored || firstNumber !== undefined);

        if (command && firstNumber === undefined) {
            commands.set(command, number);
        }

        const callType = Array.from(callTypeElement.childNodes)
            .filter((node) => node.nodeType === Node.TEXT_NODE)
            .map((node) => node.textContent)
            .join("")
            .trim()
            .replace(" - ", " · ");

        const head = createElement(
            "div",
            "oc-mp-query-head",
            createElement("span", "oc-mp-query-number", `#${number}`),
            createElement("span", "oc-mp-call-type", callType),
        );

        if (duplicate) {
            const hint = firstNumber !== undefined ? `${format(labels.duplicateOf, String(firstNumber))} ${labels.duplicateHint ?? ""}` : (labels.duplicateHint ?? "");

            head.append(createBadge(labels.duplicate, hint.trim()));
        }

        if (errored) {
            head.append(createBadge(labels.error, labels.errorHint ?? ""));
        }

        const facts = createElement(
            "div",
            "oc-mp-facts",
            ...createFact(labels.duration, createElement("strong", "", timing[1]), " ms"),
            ...createFact(labels.startedAt, `${timing[2]} ms`),
        );

        const firstResult = /[\d.,]+/.exec(firstResultElement?.textContent ?? "");

        if (firstResult) {
            facts.append(...createFact(labels.firstResult, `${firstResult[0]} ms`));
        }

        facts.append(...createFact(labels.step, createStep((stepElement.textContent ?? "").trim())));

        cell.replaceChildren(head, facts);
        row.classList.add("oc-mp-query");
        row.classList.toggle("oc-mp-duplicate", duplicate);
        row.classList.toggle("oc-mp-errored", errored);

        const timingId = row.dataset.timingId;

        if (timingId && (duplicate || errored)) {
            const flags = flaggedTimings.get(timingId);

            flaggedTimings.set(timingId, {
                duplicate: duplicate || (flags?.duplicate ?? false),
                errored: errored || (flags?.errored ?? false),
            });
        }
    });

    return flaggedTimings;
};

// The compact views keep MiniProfiler's red "!", with a tooltip that tells what it stands for.
const flagTimings = (result: Element, flaggedTimings: Map<string, QueryFlags>) => {
    const buttonWarning = result.querySelector<HTMLElement>(".mp-button > .mp-warning");

    if (buttonWarning) {
        buttonWarning.title = labels.warnings ?? "";
    }

    flaggedTimings.forEach((flags, timingId) => {
        const link = result.querySelector(`.mp-popup .mp-timings tr[data-timing-id="${CSS.escape(timingId)}"] .mp-queries-show`);
        const flag = createElement("span", "mp-warning oc-mp-flag", "!");

        flag.title = [flags.duplicate ? labels.timingDuplicates : "", flags.errored ? labels.timingErrors : ""].filter(Boolean).join(" ");

        link?.insertAdjacentElement("afterend", flag);
    });
};

const styleResults = (container: HTMLElement) => {
    container.querySelectorAll(".mp-result:not(.oc-mp-styled)").forEach((result) => {
        result.classList.add("oc-mp-styled");

        styleTimings(result);
        flagTimings(result, styleQueries(result));
    });
};

const dock = (container: HTMLElement, toggle: HTMLButtonElement) => {
    container.classList.add("oc-mp-docked");
    container.insertAdjacentElement("afterbegin", toggle);
    toggle.hidden = false;

    applyScheme(container);
    setCollapsed(container, toggle, isCollapsed());
    showWarning(container, toggle);

    // Profiles of later AJAX requests keep arriving in the container.
    new MutationObserver(() => {
        showWarning(container, toggle);
    }).observe(container, { childList: true });

    toggle.addEventListener("click", () => {
        const collapsed = !container.classList.contains("oc-mp-collapsed");

        setCollapsed(container, toggle, collapsed);

        try {
            localStorage.setItem(storageKey, String(collapsed));
        } catch {
            // The state is only remembered when the storage is available.
        }
    });

    new MutationObserver(() => {
        applyScheme(container);
    }).observe(root, { attributes: true, attributeFilter: ["data-bs-theme"] });
};

const setup = (container: HTMLElement) => {
    const toggle = document.getElementById("oc-mp-toggle") as HTMLButtonElement | null;

    if (toggle) {
        dock(container, toggle);
    }

    styleResults(container);

    // Profiles of later AJAX requests keep arriving in the container.
    new MutationObserver(() => {
        styleResults(container);
    }).observe(container, { childList: true });
};

const findContainer = () => document.querySelector<HTMLElement>("body > .mp-results");

// MiniProfiler reads its color scheme from the include tag when it starts. Only the admin follows its theme.
if (document.getElementById("oc-mp-toggle")) {
    document.getElementById("mini-profiler")?.setAttribute("data-scheme", getScheme());
}

// MiniProfiler creates its container once the page has loaded.
const container = findContainer();

if (container) {
    setup(container);
} else {
    const observer = new MutationObserver(() => {
        const container = findContainer();

        if (container) {
            observer.disconnect();
            setup(container);
        }
    });

    observer.observe(document.body, { childList: true });
}
