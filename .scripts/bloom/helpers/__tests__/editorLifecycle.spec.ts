import { afterEach, describe, expect, it, vi } from "vitest";
import {
    EDITOR_UNMOUNTING_EVENT,
    bindCodeMirrorToTextArea,
    dispatchEditorUnmounting,
    onEditorUnmounting,
} from "../editorLifecycle";

type Handler = (editor: CodeMirrorEditor) => void;

const createFakeCodeMirror = () => {
    const handlers = new Map<string, Handler[]>();
    const editor = {
        on: vi.fn((event: string, handler: Handler) => handlers.set(event, [...(handlers.get(event) ?? []), handler])),
        save: vi.fn(),
        toTextArea: vi.fn(),
        getValue: vi.fn(() => ""),
        setValue: vi.fn(),
        setOption: vi.fn(),
        display: { wrapper: document.createElement("div") },
    };
    const emit = (event: string) => handlers.get(event)?.forEach((handler) => handler(editor));

    return { editor, emit };
};

const mount = (html: string) => {
    const container = document.createElement("div");
    container.innerHTML = html;
    document.body.appendChild(container);

    return container;
};

describe("editorLifecycle", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("onEditorUnmounting_ContainerDispatches_DisposesOnce", () => {
        const container = mount('<div class="editor"></div>');
        const dispose = vi.fn();
        onEditorUnmounting(container.querySelector(".editor")!, dispose);

        dispatchEditorUnmounting(container);
        dispatchEditorUnmounting(container);

        expect(dispose).toHaveBeenCalledTimes(1);
    });

    it("onEditorUnmounting_OtherContainerDispatches_KeepsEditor", () => {
        const first = mount('<div class="editor"></div>');
        const second = mount("<div></div>");
        const dispose = vi.fn();
        onEditorUnmounting(first.querySelector(".editor")!, dispose);

        dispatchEditorUnmounting(second);

        expect(dispose).not.toHaveBeenCalled();
    });

    it("onEditorUnmounting_StoppedBeforeDispatch_DoesNotDispose", () => {
        const container = mount('<div class="editor"></div>');
        const dispose = vi.fn();
        const stop = onEditorUnmounting(container.querySelector(".editor")!, dispose);

        stop();
        dispatchEditorUnmounting(container);

        expect(dispose).not.toHaveBeenCalled();
    });

    it("dispatchEditorUnmounting_Always_BubblesToDocument", () => {
        const container = mount("<div></div>");
        const listener = vi.fn();
        document.addEventListener(EDITOR_UNMOUNTING_EVENT, listener);

        dispatchEditorUnmounting(container);
        document.removeEventListener(EDITOR_UNMOUNTING_EVENT, listener);

        expect(listener).toHaveBeenCalledTimes(1);
    });

    it("bindCodeMirrorToTextArea_EditedThenBlurred_SavesAndFiresOneChange", () => {
        const container = mount("<textarea></textarea>");
        const textArea = container.querySelector("textarea")!;
        const { editor, emit } = createFakeCodeMirror();
        const onChange = vi.fn();
        container.addEventListener("change", onChange);

        bindCodeMirrorToTextArea(editor, textArea);
        emit("blur");
        emit("change");
        emit("change");
        emit("blur");
        emit("blur");

        expect(editor.save).toHaveBeenCalledTimes(2);
        expect(onChange).toHaveBeenCalledTimes(1);
        expect(onChange.mock.calls[0][0].target).toBe(textArea);
    });

    it("bindCodeMirrorToTextArea_ContainerUnmounting_RestoresTextArea", () => {
        const container = mount("<textarea></textarea>");
        const { editor } = createFakeCodeMirror();

        bindCodeMirrorToTextArea(editor, container.querySelector("textarea")!);
        dispatchEditorUnmounting(container);

        expect(editor.toTextArea).toHaveBeenCalledTimes(1);
    });
});
