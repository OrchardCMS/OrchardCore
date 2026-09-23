import removeDiacritics from "@orchardcore/bloom/helpers/removeDiacritics";
///<reference path="@types/bootstrap/index.d.ts" />

interface QuickNavigationDestination {
    source: string;
    id: string;
    title: string;
    path: string[];
    href: string;
    target: string | null;
}

interface QuickNavigationEntry extends QuickNavigationDestination {
    normalizedTitle: string;
    normalizedPath: string;
}

interface RankedEntry {
    entry: QuickNavigationEntry;
    rank: number;
}

const pathSeparator = " › ";

let modalElement: HTMLElement | null = null;
let modal: bootstrap.Modal | null = null;
let input: HTMLInputElement | null = null;
let list: HTMLUListElement | null = null;
let status: HTMLElement | null = null;
let index: QuickNavigationEntry[] | null = null;
let results: QuickNavigationEntry[] = [];
let request: AbortController | null = null;
let loading = false;
let isOpen = false;
let closeRequested = false;
let reopenRequested = false;
let activeIndex = -1;
let maxResults = 15;

const normalize = (value: string): string => removeDiacritics(value).toLocaleLowerCase().trim();

const isDestination = (value: unknown): value is QuickNavigationDestination => {
    if (typeof value !== "object" || value === null) {
        return false;
    }

    const entry = value as Record<string, unknown>;
    return typeof entry.source === "string"
        && typeof entry.id === "string"
        && typeof entry.title === "string"
        && Array.isArray(entry.path) && entry.path.every((part: unknown) => typeof part === "string")
        && typeof entry.href === "string" && entry.href.length > 0
        && ["http:", "https:"].includes(new URL(entry.href, window.location.href).protocol)
        && (entry.target === null || typeof entry.target === "string");
};

const filter = (term: string): QuickNavigationEntry[] => {
    const entries = index ?? [];
    const normalizedTerm = normalize(term);

    if (!normalizedTerm) {
        return entries.slice(0, maxResults);
    }

    const ranked: RankedEntry[] = [];

    for (const entry of entries) {
        let rank = 0;

        if (entry.normalizedTitle.startsWith(normalizedTerm)) {
            rank = 3;
        } else if (entry.normalizedTitle.includes(normalizedTerm)) {
            rank = 2;
        } else if (entry.normalizedPath.includes(normalizedTerm)) {
            rank = 1;
        }

        if (rank > 0) {
            ranked.push({ entry, rank });
        }
    }

    // Array.prototype.sort is stable, so equal ranks keep the menu order.
    ranked.sort((a, b) => b.rank - a.rank);

    return ranked.slice(0, maxResults).map((r) => r.entry);
};

// Appends text to parent, wrapping the first match of term in a <mark>. Only DOM APIs are
// used (no innerHTML) so menu titles are never interpreted as HTML.
const appendHighlighted = (parent: HTMLElement, text: string, term: string) => {
    const normalizedText = normalize(text);
    const normalizedTerm = normalize(term);

    // Diacritics removal is not always one-to-one (e.g. "Æ" becomes "AE"). Only highlight when
    // the normalized text keeps the original length so the slice offsets stay valid.
    const start = normalizedTerm && normalizedText.length === text.length ? normalizedText.indexOf(normalizedTerm) : -1;

    if (start < 0) {
        parent.appendChild(document.createTextNode(text));
        return;
    }

    const end = start + normalizedTerm.length;
    const mark = document.createElement("mark");
    mark.textContent = text.slice(start, end);

    parent.appendChild(document.createTextNode(text.slice(0, start)));
    parent.appendChild(mark);
    parent.appendChild(document.createTextNode(text.slice(end)));
};

const setActive = (i: number) => {
    if (!list || !input) {
        return;
    }

    list.querySelectorAll<HTMLElement>("[role=option]").forEach((option) => {
        option.classList.remove("active");
        option.setAttribute("aria-selected", "false");
    });

    activeIndex = i;

    if (i < 0) {
        input.removeAttribute("aria-activedescendant");
        return;
    }

    const option = list.querySelector<HTMLElement>(`[role=option][data-index="${i}"]`);

    if (!option) {
        return;
    }

    option.classList.add("active");
    option.setAttribute("aria-selected", "true");
    input.setAttribute("aria-activedescendant", option.id);
    option.scrollIntoView({ block: "nearest" });
};

const render = (term: string) => {
    if (!list || !modalElement || !status) {
        return;
    }

    list.replaceChildren();

    status.hidden = results.length > 0;
    status.textContent = results.length === 0 ? modalElement.dataset.noResults ?? "" : "";

    if (results.length === 0) {
        setActive(-1);
        return;
    }

    results.forEach((entry, i) => {
        const option = document.createElement("li");
        option.id = `adminQuickNavigationOption${i}`;
        option.setAttribute("role", "option");
        option.dataset.index = String(i);

        const link = document.createElement("a");
        link.className = "admin-quick-navigation-item";
        link.href = entry.href;
        link.tabIndex = -1;

        if (entry.target) {
            link.target = entry.target;
            link.rel = "noopener";
        }

        const title = document.createElement("span");
        title.className = "admin-quick-navigation-title";
        appendHighlighted(title, entry.title, term);
        link.appendChild(title);

        if (entry.path.length > 0) {
            const path = document.createElement("span");
            path.className = "admin-quick-navigation-path";
            appendHighlighted(path, entry.path.join(pathSeparator), term);
            link.appendChild(path);
        }

        option.appendChild(link);
        list!.appendChild(option);
    });

    setActive(0);
};

const navigate = (entry: QuickNavigationEntry) => {
    if (entry.target && entry.target !== "_self") {
        window.open(entry.href, entry.target, "noopener");
        return;
    }

    window.location.href = entry.href;
};

const update = () => {
    if (!input || loading || index === null) {
        return;
    }

    results = filter(input.value);
    render(input.value);
};

const loadIndex = async () => {
    if (!modalElement || !list || !status) {
        return;
    }

    request?.abort();
    const controller = new AbortController();
    request = controller;
    loading = true;
    index = null;
    results = [];
    list.replaceChildren();
    list.setAttribute("aria-busy", "true");
    setActive(-1);
    status.hidden = false;
    status.textContent = modalElement.dataset.loading ?? "";

    try {
        const url = modalElement.dataset.indexUrl;
        if (!url) {
            throw new Error("The quick navigation index URL is missing.");
        }

        const response = await fetch(url, { credentials: "same-origin", cache: "no-store", signal: controller.signal });
        if (!response.ok) {
            throw new Error(`Unable to load the quick navigation index (${response.status}).`);
        }

        const destinations: unknown = await response.json();
        if (!Array.isArray(destinations) || !destinations.every(isDestination)) {
            throw new Error("The quick navigation index is invalid.");
        }

        if (controller.signal.aborted) {
            return;
        }

        index = destinations.map((entry) => ({
            ...entry,
            normalizedTitle: normalize(entry.title),
            normalizedPath: normalize(entry.path.join(pathSeparator)),
        }));
        loading = false;
        update();
    } catch (error) {
        if (!controller.signal.aborted) {
            console.error("Unable to load quick navigation.", error);
            status.textContent = modalElement.dataset.loadError ?? "";
        }
    } finally {
        if (request === controller) {
            request = null;
            loading = false;
            list.setAttribute("aria-busy", "false");
        }
    }
};

const open = () => {
    if (!modal) {
        return;
    }

    if (isOpen) {
        reopenRequested = closeRequested;
        return;
    }

    isOpen = true;
    closeRequested = false;
    modal.show();
    void loadIndex();
};

// Rich text and code editors bind Ctrl+K themselves (Monaco chords, CodeMirror and
// Trumbowyg insert-link), so the shortcut is left to them when focus is inside one.
const isEditorTarget = (target: EventTarget | null): boolean => {
    const element = target as HTMLElement | null;

    if (!element || typeof element.closest !== "function") {
        return false;
    }

    return element.isContentEditable
        || element.tagName === "TEXTAREA"
        || element.closest(".monaco-editor, .CodeMirror, .cm-editor, .trumbowyg-box") !== null;
};

const onDocumentKeydown = (e: KeyboardEvent) => {
    // Bootstrap ignores hide() during its opening transition. Defer an early Escape until shown.
    if (e.key === "Escape" && modal && isOpen) {
        closeRequested = true;
        modal.hide();
        return;
    }

    if (!(e.ctrlKey || e.metaKey) || e.altKey || e.shiftKey || (e.key ?? "").toLowerCase() !== "k") {
        return;
    }

    if (isEditorTarget(e.target)) {
        return;
    }

    e.preventDefault();
    open();
};

const onInputKeydown = (e: KeyboardEvent) => {
    switch (e.key) {
        case "ArrowDown":
            e.preventDefault();
            if (results.length > 0) {
                setActive((activeIndex + 1) % results.length);
            }
            break;
        case "ArrowUp":
            e.preventDefault();
            if (results.length > 0) {
                setActive((activeIndex - 1 + results.length) % results.length);
            }
            break;
        case "Enter":
            e.preventDefault();
            if (activeIndex >= 0 && activeIndex < results.length) {
                navigate(results[activeIndex]);
            }
            break;
        // Escape is handled by Bootstrap (modal keyboard option).
    }
};

const initializeQuickNavigation = () => {
    modalElement = document.getElementById("adminQuickNavigationModal");

    if (!modalElement || typeof bootstrap === "undefined") {
        return;
    }

    // Move the modal out of the navbar so it escapes its fixed-top stacking context.
    document.body.appendChild(modalElement);

    input = modalElement.querySelector<HTMLInputElement>("#adminQuickNavigationInput");
    list = modalElement.querySelector<HTMLUListElement>("#adminQuickNavigationResults");
    status = modalElement.querySelector<HTMLElement>("#adminQuickNavigationStatus");

    if (!input || !list || !status) {
        return;
    }

    maxResults = parseInt(modalElement.dataset.maxResults ?? "", 10) || 15;
    modal = new bootstrap.Modal(modalElement);

    document.getElementById("adminQuickNavigationToggle")?.addEventListener("click", open);
    document.addEventListener("keydown", onDocumentKeydown);

    modalElement.addEventListener("shown.bs.modal", () => {
        if (closeRequested) {
            modal?.hide();
            return;
        }

        input?.focus();
        input?.select();
    });

    modalElement.addEventListener("hide.bs.modal", () => {
        closeRequested = true;
        request?.abort();
        request = null;
        loading = false;
        list?.setAttribute("aria-busy", "false");
    });

    modalElement.addEventListener("hidden.bs.modal", () => {
        isOpen = false;
        closeRequested = false;

        if (input) {
            input.value = "";
        }

        results = [];
        activeIndex = -1;
        list?.replaceChildren();
        setActive(-1);

        if (reopenRequested) {
            reopenRequested = false;
            open();
        }
    });

    input.addEventListener("input", update);
    input.addEventListener("keydown", onInputKeydown);

    list.addEventListener("mousemove", (e) => {
        const option = (e.target as HTMLElement).closest<HTMLElement>("[role=option]");

        if (option?.dataset.index !== undefined) {
            const i = Number(option.dataset.index);

            if (i !== activeIndex) {
                setActive(i);
            }
        }
    });
};

export { initializeQuickNavigation };
