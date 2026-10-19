// Docks the MiniProfiler widget to the middle of the right edge of admin pages, behind a toggle that tucks it away,
// and follows the admin's light and dark themes.
interface MiniProfilerWindow extends Window {
    MiniProfiler?: {
        options?: {
            colorScheme?: string;
        };
    };
}

const storageKey = "OrchardCore.MiniProfiler.Collapsed";
const root = document.documentElement;

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

// Timing names look like "Controller Action: ...", "Shape: ..." or "View: ...". Split off the kind and tag each row
// with it, so that the pipeline steps can stand out from the shapes and views they render.
const styleTimings = (container: HTMLElement) => {
    container.querySelectorAll(".mp-result:not(.oc-mp-styled)").forEach((result) => {
        result.classList.add("oc-mp-styled");

        result.querySelectorAll<HTMLElement>(".mp-popup .mp-timings tr[data-timing-id] .mp-label").forEach((label) => {
            const name = (label.textContent ?? "").trim();
            const separator = name.indexOf(": ");
            const kind = separator > 0 ? name.substring(0, separator) : "";
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

            const kindElement = document.createElement("span");
            kindElement.className = "oc-mp-kind";
            kindElement.textContent = `${kind}: `;

            const nameElement = document.createElement("span");
            nameElement.className = "oc-mp-name";
            nameElement.textContent = name.substring(separator + 2);

            // "/Areas/{module}/Views/{file}" mostly repeats itself; the full path stays in the tooltip.
            const view = kind === "View" ? /^\/Areas\/([^/]+)\/Views\/(.+)$/.exec(nameElement.textContent) : null;

            if (view) {
                const moduleElement = document.createElement("span");
                moduleElement.className = "oc-mp-kind";
                moduleElement.textContent = `${view[1]}/`;

                nameElement.replaceChildren(moduleElement, view[2]);
            }

            label.replaceChildren(kindElement, nameElement);
        });
    });
};

const dock = (container: HTMLElement, toggle: HTMLButtonElement) => {
    container.classList.add("oc-mp-docked");
    container.insertAdjacentElement("afterbegin", toggle);
    toggle.hidden = false;

    applyScheme(container);
    setCollapsed(container, toggle, isCollapsed());
    showWarning(container, toggle);
    styleTimings(container);

    // Profiles of later AJAX requests keep arriving in the container.
    new MutationObserver(() => {
        showWarning(container, toggle);
        styleTimings(container);
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

const findContainer = () => document.querySelector<HTMLElement>("body > .mp-results");

const toggle = document.getElementById("oc-mp-toggle") as HTMLButtonElement | null;
const includes = document.getElementById("mini-profiler");

// MiniProfiler reads its color scheme from the include tag when it starts.
includes?.setAttribute("data-scheme", getScheme());

if (toggle) {
    // MiniProfiler creates its container once the page has loaded.
    const container = findContainer();

    if (container) {
        dock(container, toggle);
    } else {
        const observer = new MutationObserver(() => {
            const container = findContainer();

            if (container) {
                observer.disconnect();
                dock(container, toggle);
            }
        });

        observer.observe(document.body, { childList: true });
    }
}
