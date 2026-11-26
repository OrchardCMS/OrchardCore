import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { initSelectPicker, SelectpickerOptionData } from "@orchardcore/bloom/components/select-picker";

interface CreatedTag {
    contentItemId: string;
    displayText: string;
}

// Creates a tag term in the taxonomy of the field, and returns it as an option of the picker, or null when it can't be
// created.
const createTag = async (select: HTMLSelectElement, displayText: string): Promise<SelectpickerOptionData | null> => {
    const token = document.querySelector<HTMLInputElement>("input[name='__RequestVerificationToken']")?.value ?? "";

    try {
        const response = await fetch(select.dataset.createTagUrl ?? "", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({
                __RequestVerificationToken: token,
                taxonomyContentItemId: select.dataset.taxonomyContentItemId ?? "",
                displayText,
            }),
        });

        if (!response.ok) {
            throw new Error(`The tag couldn't be created (${response.status}).`);
        }

        const tag: CreatedTag = await response.json();

        return { value: tag.contentItemId, text: tag.displayText };
    } catch {
        alert(select.dataset.createTagErrorMessage);

        return null;
    }
};

// The Tags editor of the taxonomy field: a bootstrap-select of the terms of the taxonomy. When the field is open, a tag
// typed in the search box that doesn't exist is created in the taxonomy, and selected.
observeAndInit("select[data-tags-editor]", (element) => {
    const select = element as HTMLSelectElement;
    const open = select.dataset.openOptions === "true";

    initSelectPicker(select, open ? { source: { create: (_callback: unknown, searchValue: string) => createTag(select, searchValue) } } : {});
});
