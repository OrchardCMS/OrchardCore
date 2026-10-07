import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import initEasyMdeEditor from "@orchardcore/bloom/components/easymde-editor";
import { getDatasetBoolean, getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

observeAndInit(".markdown-body-part-wysiwyg-editor", (wrapper) => {
    const markdownElement = wrapper.querySelector<HTMLTextAreaElement>("textarea");

    initEasyMdeEditor(markdownElement, getDatasetBoolean(wrapper, "isRtl"), getDatasetJson(wrapper, "options"));
});
