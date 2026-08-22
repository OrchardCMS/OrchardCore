import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const htmlBody = document.getElementById("HtmlBody") as HTMLTextAreaElement | null;
const textBody = document.getElementById("TextBody") as HTMLTextAreaElement | null;

if (htmlBody) {
    initLiquidPatternEditor(htmlBody);
}

if (textBody) {
    initLiquidPatternEditor(textBody);
}

const addClass = (element: HTMLElement | null, className: string) => {
    element?.classList.add(className);
};

const removeClass = (element: HTMLElement | null, className: string) => {
    element?.classList.remove(className);
};

const changeBodyFormat = () => {
    const select = document.querySelector<HTMLSelectElement>("select");

    if (!select) {
        return;
    }

    const textBodyDiv = document.getElementById("textBodyDiv");
    const htmlBodyDiv = document.getElementById("htmlBodyDiv");
    const allValue = select.dataset.allValue;
    const textValue = select.dataset.textValue;
    const htmlValue = select.dataset.htmlValue;

    if (select.value === allValue) {
        removeClass(textBodyDiv, "d-none");
        removeClass(htmlBodyDiv, "d-none");
    } else if (select.value === textValue) {
        removeClass(textBodyDiv, "d-none");
        addClass(htmlBodyDiv, "d-none");
    } else if (select.value === htmlValue) {
        addClass(textBodyDiv, "d-none");
        removeClass(htmlBodyDiv, "d-none");
    }
};

changeBodyFormat();

document.querySelectorAll("select").forEach((select) => {
    select.addEventListener("change", changeBodyFormat);
});
