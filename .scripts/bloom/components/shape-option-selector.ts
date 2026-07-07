// Shared by ContentTypes' EditField.cshtml (prefix "field") and EditTypePart.cshtml
// (prefix "type-part") - both toggle an editor-shape group and a display-shape group based on a
// sibling <select>, using the exact same fixed id/class-family shape, differing only in prefix.
const initToggleGroup = (selectId: string, containerId: string, classPrefix: string) => {
    document.querySelectorAll<HTMLElement>(`.${classPrefix}`).forEach((element) => {
        element.style.display = "none";
    });

    const container = document.getElementById(containerId);
    if (container) {
        container.style.display = "";
    }

    const select = document.getElementById(selectId) as HTMLSelectElement | null;
    if (!select) {
        return;
    }

    const showForValue = () => {
        document.querySelectorAll<HTMLElement>(`.${classPrefix}-${select.value.toLowerCase()}`).forEach((element) => {
            element.style.display = "";
        });
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
