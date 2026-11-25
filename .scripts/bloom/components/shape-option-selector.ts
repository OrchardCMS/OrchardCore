// Shared by ContentTypes' EditField.cshtml (prefix "field") and EditTypePart.cshtml
// (prefix "type-part") - both toggle an editor-shape group and a display-shape group based on a
// sibling <select>, using the exact same data-attribute/id/class-family shape, differing only in prefix.
// The <select> is found through a data attribute (e.g. data-field-editor-select) rather than an id
// so that its asp-for generated id still matches its <label asp-for>.
// The settings shapes of the editors are grouped in cards: a card whose shapes are all hidden, e.g. the "Editor" card
// when the selected editor has no settings of its own, is hidden too.
const updateCards = (container: HTMLElement | null) => {
    container?.querySelectorAll<HTMLElement>(".ocat-card").forEach((card) => {
        const body = card.querySelector<HTMLElement>(":scope > .ocat-card-body");
        if (!body) {
            return;
        }

        const hasVisibleContent = Array.from(body.children).some(
            (child) =>
                child instanceof HTMLElement &&
                child.tagName !== "SCRIPT" &&
                !(child instanceof HTMLInputElement && child.type === "hidden") &&
                child.style.display !== "none",
        );

        card.style.display = hasVisibleContent ? "" : "none";
    });
};

const initToggleGroup = (selectAttribute: string, containerId: string, classPrefix: string) => {
    document.querySelectorAll<HTMLElement>(`.${classPrefix}`).forEach((element) => {
        element.style.display = "none";
    });

    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = "";
    }

    const select = document.querySelector<HTMLSelectElement>(`[data-${selectAttribute}]`);
    if (!select) {
        updateCards(container);
        return;
    }

    const showForValue = () => {
        document.querySelectorAll<HTMLElement>(`.${classPrefix}-${select.value.toLowerCase()}`).forEach((element) => {
            element.style.display = "";
        });

        updateCards(container);
    };

    showForValue();

    select.addEventListener("change", () => {
        document.querySelectorAll<HTMLElement>(`.${classPrefix}`).forEach((element) => {
            element.style.display = "none";
        });
        showForValue();
    });
};

const initShapeOptionSelector = (prefix: string) => {
    initToggleGroup(`${prefix}-editor-select`, `${prefix}-editor-container`, `${prefix}-editor`);
    initToggleGroup(`${prefix}-display-select`, `${prefix}-display-container`, `${prefix}-display`);
};

export default initShapeOptionSelector;
