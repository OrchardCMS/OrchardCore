import removeDiacritics from "./removeDiacritics";
import { textElements } from "./text";

const fold = (value: string) => removeDiacritics(value).toLocaleLowerCase();

// Only DOM APIs are used so titles are never interpreted as HTML.
export const appendHighlighted = (parent: HTMLElement, text: string, term: string) => {
    const normalizedText = fold(text).trim();
    const normalizedTerm = fold(term).trim();
    const start = normalizedTerm ? normalizedText.indexOf(normalizedTerm) : -1;
    const end = start + normalizedTerm.length;
    const boundaries = new Set([text.length]);
    let offsetsMatch = text.trim() === text && normalizedText.length === text.length;

    // Equal total lengths are insufficient: expansions and contractions can cancel out.
    for (const { segment, index } of textElements(text)) {
        boundaries.add(index);
        offsetsMatch &&= fold(segment).length === segment.length;
    }

    if (start < 0 || !offsetsMatch || !boundaries.has(start) || !boundaries.has(end)) {
        parent.appendChild(document.createTextNode(text));
        return;
    }

    const mark = document.createElement("mark");
    mark.textContent = text.slice(start, end);
    parent.appendChild(document.createTextNode(text.slice(0, start)));
    parent.appendChild(mark);
    parent.appendChild(document.createTextNode(text.slice(end)));
};
