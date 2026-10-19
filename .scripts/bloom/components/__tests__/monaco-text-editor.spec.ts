import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import initMonacoTextEditor from "../monaco-text-editor";
import { dispatchEditorUnmounting } from "../../helpers/editorLifecycle";

// A minimal stand-in for the Monaco API surface the component uses.
const createFakeMonaco = () => {
    const editors: ReturnType<typeof createEditor>[] = [];

    function createEditor() {
        let value = "";
        let disposed = false;
        let modelDisposed = false;
        const changeListeners: (() => void)[] = [];
        const blurListeners: (() => void)[] = [];
        const model = {
            setValue: (next: string) => {
                value = next;
            },
            isDisposed: () => modelDisposed,
            dispose: vi.fn(() => {
                modelDisposed = true;
            }),
        };

        return {
            getModel: () => model,
            getValue: () => value,
            onDidChangeModelContent: (listener: () => void) => changeListeners.push(listener),
            onDidBlurEditorText: (listener: () => void) => blurListeners.push(listener),
            dispose: vi.fn(() => {
                disposed = true;
            }),
            type: (next: string) => {
                value = next;
                changeListeners.forEach((listener) => listener());
            },
            blur: () => blurListeners.forEach((listener) => listener()),
            get disposed() {
                return disposed;
            },
            model,
        };
    }

    const monaco = {
        editor: {
            setTheme: vi.fn(),
            create: vi.fn(() => {
                const editor = createEditor();
                editors.push(editor);

                return editor;
            }),
        },
    };

    return { monaco, editors };
};

const mountEditor = (value: string) => {
    const container = document.createElement("div");
    container.innerHTML = `
        <div class="monaco-text-editor" data-language="javascript">
            <div class="monaco-text-editor-container"></div>
            <textarea class="monaco-text-editor-value"></textarea>
        </div>`;
    container.querySelector("textarea")!.value = value;
    document.body.appendChild(container);

    return container;
};

const flush = () => new Promise((resolve) => setTimeout(resolve, 0));

describe("monaco-text-editor", () => {
    let fake: ReturnType<typeof createFakeMonaco>;

    beforeEach(() => {
        fake = createFakeMonaco();
        window.__orchardCoreMonacoReady = Promise.resolve(fake.monaco as never);
    });

    afterEach(() => {
        document.body.innerHTML = "";
        delete window.__orchardCoreMonacoReady;
    });

    it("initMonacoTextEditor_Edited_KeepsTextAreaInSyncAndFiresChangeOnBlur", async () => {
        const container = mountEditor("let a = 1;");
        const textArea = container.querySelector("textarea")!;
        const onChange = vi.fn();
        container.addEventListener("change", onChange);

        initMonacoTextEditor(container.querySelector(".monaco-text-editor")!);
        await flush();
        const editor = fake.editors[0];

        expect(editor.getValue()).toBe("let a = 1;");

        editor.type("let a = 2;");
        expect(textArea.value).toBe("let a = 2;");
        expect(onChange).not.toHaveBeenCalled();

        editor.blur();
        editor.blur();
        expect(onChange).toHaveBeenCalledTimes(1);
    });

    it("initMonacoTextEditor_ContainerUnmounting_DisposesEditorAndModel", async () => {
        const container = mountEditor("");

        initMonacoTextEditor(container.querySelector(".monaco-text-editor")!);
        await flush();
        dispatchEditorUnmounting(container);
        const editor = fake.editors[0];

        expect(editor.dispose).toHaveBeenCalledTimes(1);
        expect(editor.model.dispose).toHaveBeenCalledTimes(1);
    });

    it("initMonacoTextEditor_RemovedBeforeMonacoLoads_CreatesNoEditor", async () => {
        const container = mountEditor("");
        const element = container.querySelector<HTMLElement>(".monaco-text-editor")!;

        initMonacoTextEditor(element);
        container.remove();
        await flush();

        expect(fake.monaco.editor.create).not.toHaveBeenCalled();
    });
});
