import { afterEach, describe, expect, it, vi } from "vitest";
import { insertAtCursor } from "../insertion";

describe("insertAtCursor", () => {
    afterEach(() => {
        document.body.innerHTML = "";
        delete window.__orchardCoreMonacoReady;
    });

    it("insertAtCursor_Input_ReplacesTheSelectionAndReportsTheChange", async () => {
        document.body.innerHTML = '<form><input type="text" value="Hello world" /></form>';
        const input = document.querySelector("input")!;
        const changes = vi.fn();
        document.querySelector("form")!.addEventListener("change", changes);
        input.setSelectionRange(6, 11);

        expect(await insertAtCursor(input, "{{ Workflow.Input.Owner }}")).toBe(true);

        expect(input.value).toBe("Hello {{ Workflow.Input.Owner }}");
        expect(changes).toHaveBeenCalledTimes(1);
    });

    it("insertAtCursor_TextArea_InsertsAtTheCursor", async () => {
        document.body.innerHTML = "<textarea>ab</textarea>";
        const textarea = document.querySelector("textarea")!;
        textarea.setSelectionRange(1, 1);

        expect(await insertAtCursor(textarea, "X")).toBe(true);
        expect(textarea.value).toBe("aXb");
    });

    it("insertAtCursor_CodeMirror_ReplacesItsSelection", async () => {
        document.body.innerHTML = '<div class="CodeMirror"><textarea></textarea></div>';
        const codeMirror = { replaceSelection: vi.fn(), focus: vi.fn() };
        Object.assign(document.querySelector(".CodeMirror")!, { CodeMirror: codeMirror });

        expect(await insertAtCursor(document.querySelector("textarea"), "input(\"Owner\")")).toBe(true);
        expect(codeMirror.replaceSelection).toHaveBeenCalledWith('input("Owner")');
    });

    it("insertAtCursor_Monaco_EditsTheEditorThatHadTheFocus", async () => {
        document.body.innerHTML = '<div id="one" class="monaco-editor"><textarea></textarea></div><div id="two" class="monaco-editor"><textarea></textarea></div>';
        const editor = (id: string) => ({
            getContainerDomNode: () => document.getElementById(id)!,
            getSelection: () => `selection-${id}`,
            executeEdits: vi.fn(),
            focus: vi.fn(),
        });
        const one = editor("one");
        const two = editor("two");
        window.__orchardCoreMonacoReady = Promise.resolve({ editor: { getEditors: () => [one, two] } });

        expect(await insertAtCursor(document.querySelector("#two textarea"), "lastResult()")).toBe(true);

        expect(one.executeEdits).not.toHaveBeenCalled();
        expect(two.executeEdits).toHaveBeenCalledWith("workflow-designer", [{ range: "selection-two", text: "lastResult()", forceMoveMarkers: true }]);
    });

    it("insertAtCursor_NoTextField_ReturnsFalse", async () => {
        document.body.innerHTML = '<select><option>a</option></select><input type="checkbox" /><input type="text" readonly />';

        expect(await insertAtCursor(null, "x")).toBe(false);
        expect(await insertAtCursor(document.querySelector("select"), "x")).toBe(false);
        expect(await insertAtCursor(document.querySelector("input[type=checkbox]"), "x")).toBe(false);
        expect(await insertAtCursor(document.querySelector("input[readonly]"), "x")).toBe(false);
    });
});
