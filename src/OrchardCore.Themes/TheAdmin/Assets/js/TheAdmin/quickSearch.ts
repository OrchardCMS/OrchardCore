import removeDiacritics from "@orchardcore/bloom/helpers/removeDiacritics";
///<reference path="@types/bootstrap/index.d.ts" />

// Admin quick search: a command palette (Ctrl+K / Cmd+K) that filters the admin menu items
// rendered in the left navigation. The index is built lazily from the DOM on first open, so
// it only contains the items the current user is allowed to see.

interface QuickSearchEntry {
    title: string;
    path: string[];
    href: string;
    target: string | null;
    normalizedTitle: string;
    normalizedPath: string;
}

interface RankedEntry {
    entry: QuickSearchEntry;
    rank: number;
}

const pathSeparator = " › ";

let modalElement: HTMLElement | null = null;
let modal: bootstrap.Modal | null = null;
let input: HTMLInputElement | null = null;
let list: HTMLUListElement | null = null;
let index: QuickSearchEntry[] | null = null;
let results: QuickSearchEntry[] = [];
let activeIndex = -1;
let maxResults = 15;

const normalize = (value: string): string => removeDiacritics(value).toLocaleLowerCase().trim();

const readTitle = (item: Element): string =>
    item.querySelector<HTMLElement>(":scope > figure > figcaption > .item-label > span.title")?.textContent?.trim() ?? "";

const buildIndex = (): QuickSearchEntry[] => {
    const menu = document.querySelector<HTMLElement>("nav#left-nav > ul#adminMenu");

    if (!menu) {
        return [];
    }

    const entries: QuickSearchEntry[] = [];
    const seen = new Set<string>();

    menu.querySelectorAll<HTMLAnchorElement>("a.item-label[href]").forEach((link) => {
        const href = link.getAttribute("href") ?? "";

        if (!href || href === "#") {
            return;
        }

        const item = link.closest("li");

        if (!item) {
            return;
        }

        const title = readTitle(item);

        if (!title) {
            return;
        }

        const path: string[] = [];
        let parent = item.parentElement?.closest("li") ?? null;

        while (parent) {
            const parentTitle = readTitle(parent);

            if (parentTitle) {
                path.unshift(parentTitle);
            }

            parent = parent.parentElement?.closest("li") ?? null;
        }

        const key = `${path.join("/")}/${title}|${href}`;

        if (seen.has(key)) {
            return;
        }

        seen.add(key);

        entries.push({
            title,
            path,
            href,
            target: link.getAttribute("target"),
            normalizedTitle: normalize(title),
            normalizedPath: normalize(path.join(pathSeparator)),
        });
    });

    return entries;
};

const filter = (term: string): QuickSearchEntry[] => {
    index ??= buildIndex();

    const normalizedTerm = normalize(term);

    if (!normalizedTerm) {
        return index.slice(0, maxResults);
    }

    const ranked: RankedEntry[] = [];

    for (const entry of index) {
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
    if (!list || !modalElement) {
        return;
    }

    list.replaceChildren();

    if (results.length === 0) {
        const empty = document.createElement("li");
        empty.className = "admin-quick-search-empty";
        empty.textContent = modalElement.dataset.noResults ?? "";
        list.appendChild(empty);
        setActive(-1);
        return;
    }

    results.forEach((entry, i) => {
        const option = document.createElement("li");
        option.id = `adminQuickSearchOption${i}`;
        option.setAttribute("role", "option");
        option.dataset.index = String(i);

        const link = document.createElement("a");
        link.className = "admin-quick-search-item";
        link.href = entry.href;
        link.tabIndex = -1;

        if (entry.target) {
            link.target = entry.target;
        }

        const title = document.createElement("span");
        title.className = "admin-quick-search-title";
        appendHighlighted(title, entry.title, term);
        link.appendChild(title);

        if (entry.path.length > 0) {
            const path = document.createElement("span");
            path.className = "admin-quick-search-path";
            appendHighlighted(path, entry.path.join(pathSeparator), term);
            link.appendChild(path);
        }

        option.appendChild(link);
        list!.appendChild(option);
    });

    setActive(0);
};

const navigate = (entry: QuickSearchEntry) => {
    if (entry.target && entry.target !== "_self") {
        window.open(entry.href, entry.target);
        return;
    }

    window.location.href = entry.href;
};

const update = () => {
    if (!input) {
        return;
    }

    results = filter(input.value);
    render(input.value);
};

const open = () => {
    if (!modal) {
        return;
    }

    update();
    modal.show();
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
    // Bootstrap only handles Escape when the event originates inside the modal. Cover the case
    // where the key is pressed while the modal is still fading in and the input is not focused yet.
    if (e.key === "Escape" && modal && modalElement?.classList.contains("show")) {
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

const initializeQuickSearch = () => {
    modalElement = document.getElementById("adminQuickSearchModal");

    if (!modalElement || typeof bootstrap === "undefined") {
        return;
    }

    // Move the modal out of the navbar so it escapes its fixed-top stacking context.
    document.body.appendChild(modalElement);

    input = modalElement.querySelector<HTMLInputElement>("#adminQuickSearchInput");
    list = modalElement.querySelector<HTMLUListElement>("#adminQuickSearchResults");

    if (!input || !list) {
        return;
    }

    maxResults = parseInt(modalElement.dataset.maxResults ?? "", 10) || 15;
    modal = new bootstrap.Modal(modalElement);

    document.getElementById("adminQuickSearchToggle")?.addEventListener("click", open);
    document.addEventListener("keydown", onDocumentKeydown);

    modalElement.addEventListener("shown.bs.modal", () => {
        input?.focus();
        input?.select();
    });

    modalElement.addEventListener("hidden.bs.modal", () => {
        if (input) {
            input.value = "";
        }

        results = [];
        activeIndex = -1;
        list?.replaceChildren();
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

export { initializeQuickSearch };
