import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

const initHtmlBodyPart = (element: HTMLElement) => {
    const textArea = element as HTMLTextAreaElement;
    const editor = CodeMirror.fromTextArea(textArea, {
        autoCloseTags: true,
        autoRefresh: true,
        lineNumbers: true,
        lineWrapping: true,
        matchBrackets: true,
        styleActiveLine: true,
        mode: { name: "htmlmixed" },
    });

    // Unlike Monaco's theme (a single global setting), CodeMirror's theme is set per editor
    // instance, so each instance genuinely needs its own observer here.
    const applyTheme = () => {
        const theme = document.documentElement.dataset.bsTheme;
        const isDark =
            theme === "dark" || (theme === "auto" && window.matchMedia("(prefers-color-scheme: dark)").matches);

        editor.setOption("theme", isDark ? "monokai" : "default");
    };

    new MutationObserver(applyTheme).observe(document.documentElement, { attributes: true });
    applyTheme();

    initializeCodeMirrorShortcodeWrapper(editor);

    editor.on("change", (cmEditor) => {
        cmEditor.save();
        document.dispatchEvent(new Event("contentpreview:render"));
    });
};

observeAndInit(".html-body-part-codemirror", initHtmlBodyPart);
