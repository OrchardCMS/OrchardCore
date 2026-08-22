import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";
import initOptionsTableEditor, { setTranslations } from "@orchardcore/bloom/components/options-table-editor";

observeAndInit(".seo-meta-part-custom-tags", (element) => {
    const rows = getDatasetJson<Record<string, string>[]>(element, "customMetaTags") ?? [];
    const translations = getDatasetJson<Record<string, string>>(element, "translations");
    const modalBodyElements = document.getElementsByClassName(`${element.id}-ModalBody`);

    if (!translations) {
        return;
    }

    setTranslations(translations);

    initOptionsTableEditor({
        element,
        rows,
        columns: [
            { key: "content", labelKey: "ContentColumn" },
            { key: "name", labelKey: "NameColumn" },
            { key: "property", labelKey: "PropertyColumn" },
            { key: "httpEquiv", labelKey: "HttpEquivColumn" },
            { key: "charset", labelKey: "CharsetColumn" },
        ],
        addKey: "AddACustomMetaTag",
        editDataKey: "EditData",
        okKey: "Ok",
        cancelKey: "Cancel",
        removeRowKey: "RemoveElementFromList",
        jsonTextareaLabelKey: "CustomMetaTags",
        jsonTextareaHintKey: "CustomMetaTagsJsonHint",
        hiddenInputId: element.dataset.customMetaTagsInputId ?? "",
        hiddenInputName: element.dataset.customMetaTagsInputName ?? "",
        modalBodyElements,
    });
});
