import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import initTrumbowygEditor from "@orchardcore/bloom/components/trumbowyg-editor";
import { getDatasetBoolean, getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

observeAndInit(".html-body-part-trumbowyg-editor", (wrapper) => {
    const element = wrapper.querySelector<HTMLTextAreaElement>("textarea");

    if (!element) {
        return;
    }

    initTrumbowygEditor({
        element,
        languageCode: wrapper.dataset.languageCode ?? "",
        isRtl: getDatasetBoolean(wrapper, "isRtl"),
        languageDirection: wrapper.dataset.languageDirection ?? "",
        customOptions: getDatasetJson(wrapper, "options"),
    });
});
