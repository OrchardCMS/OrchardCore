import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import initTrumbowygEditor from "@orchardcore/bloom/components/trumbowyg-editor";
import { getDatasetBoolean } from "@orchardcore/bloom/helpers/dataset";

observeAndInit(".html-body-part-wysiwyg-editor", (wrapper) => {
    const element = wrapper.querySelector<HTMLTextAreaElement>("textarea");

    if (!element) {
        return;
    }

    initTrumbowygEditor({
        element,
        languageCode: wrapper.dataset.languageCode ?? "",
        isRtl: getDatasetBoolean(wrapper, "isRtl"),
        languageDirection: wrapper.dataset.languageDirection ?? "",
        extendDefaultButtons: true,
    });
});
