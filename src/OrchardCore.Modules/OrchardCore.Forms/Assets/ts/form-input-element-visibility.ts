// Defined by OrchardCore.Forms (Assets/js/form-visibility.js), consumed here.
declare const formVisibilityGroups: {
    initialize(options: {
        appElementSelector: Element;
        prefix: string | null;
        widgetId: string | null;
        groupOptions: unknown[];
        operatorOptions: unknown[];
        caseSensitive: unknown;
    }): void;
};

declare const Vue: {
    config: { productionTip: boolean; devtools: boolean };
};

// Zero Razor-computed values in this file's original inline script, so this is a purely
// mechanical move. The MutationObserver's own semantics (re-running actionVisibilitySettings on
// EVERY .main-group-container on every mutation batch, not just newly-added ones) are preserved
// exactly rather than tightened to observeAndInit's stricter once-per-element behavior, since
// that would be a behavior change beyond extraction.
function processContainers(containers: Element[]) {
    containers.forEach((container) => {
        initializeOrUpdateVue(container);
        actionVisibilitySettings(container);
    });
}

const flowWidgetObserver = new MutationObserver((mutationsList) => {
    for (const mutation of mutationsList) {
        if (mutation.type === "childList") {
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === 1) {
                    const element = node as Element;
                    const containers = element.matches(".main-group-container")
                        ? [element]
                        : [...element.querySelectorAll(".main-group-container")];

                    processContainers(containers);
                }
            });
        }
    }
    document.querySelectorAll(".main-group-container").forEach((container) => actionVisibilitySettings(container));
});

flowWidgetObserver.observe(document.body, { childList: true, subtree: true });

Vue.config.productionTip = false;
Vue.config.devtools = false;

const containers = [...document.querySelectorAll(".main-group-container")];

processContainers(containers);

function initializeOrUpdateVue(container: Element) {
    const rawConfig = container.getAttribute("data-model");
    const cfg = rawConfig ? JSON.parse(rawConfig) : {};

    formVisibilityGroups.initialize({
        appElementSelector: container,
        prefix: container.getAttribute("data-prefix"),
        widgetId: container.getAttribute("data-prefix"),
        groupOptions: cfg.groups || [],
        operatorOptions: cfg.operators || [],
        caseSensitive: cfg.caseSensitive,
    });
}

function actionVisibilitySettings(container: Element) {
    const prefix = container.getAttribute("data-prefix");

    if (!prefix) {
        console.log("No data-prefix found; exiting.");
        return;
    }

    const findPrefix = `${prefix}.Action`;

    const actionElement = document.querySelector<HTMLSelectElement>(`select[name="${findPrefix}"]`);

    if (!actionElement) {
        return;
    }

    const visibilityContainer = document.querySelector<HTMLElement>(
        `.input-visibility-settings[data-prefix="${prefix}"]`,
    );

    if (!visibilityContainer) {
        return;
    }

    actionElement.addEventListener("change", (e) => {
        if ((e.target as HTMLSelectElement).value === "None") {
            visibilityContainer.classList.add("d-none");
        } else {
            visibilityContainer.classList.remove("d-none");
        }
    });

    actionElement.dispatchEvent(new Event("change", { bubbles: true }));
}
