import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import ServerFormHost from "../ServerFormHost.vue";
import type { FormFragment } from "../../api/types";
import type { FormApplyResult } from "../types";

const fragment = (content: string, styles = ""): FormFragment => ({ valid: true, content, scripts: "", styles });

const mountHost = (options: { load?: () => Promise<FormFragment>; submit?: (form: FormData) => Promise<FormApplyResult>; formKey?: string } = {}) => {
    const load = vi.fn(options.load ?? (() => Promise.resolve(fragment('<input name="Activity.Message" value="Hello" />'))));
    const submit = vi.fn(options.submit ?? (() => Promise.resolve({ valid: true } as FormApplyResult)));
    const wrapper = mount(ServerFormHost, {
        props: { formKey: options.formKey ?? "a", load, submit, label: "Form" },
        attachTo: document.body,
    });

    return { wrapper, load, submit };
};

describe("ServerFormHost", () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
        document.head.innerHTML = "";
        document.body.innerHTML = "";
    });

    it("change_FieldCommitted_AppliesOnceAfterTheDebounce", async () => {
        const { wrapper, submit } = mountHost();
        await flushPromises();
        const input = wrapper.get("input[name='Activity.Message']");

        (input.element as HTMLInputElement).value = "Hi";
        await input.trigger("input");
        await input.trigger("change");
        await input.trigger("change");

        vi.advanceTimersByTime(599);
        expect(submit).not.toHaveBeenCalled();

        vi.advanceTimersByTime(1);
        await flushPromises();

        expect(submit).toHaveBeenCalledTimes(1);
        expect((submit.mock.calls[0][0] as FormData).get("Activity.Message")).toBe("Hi");
        expect(wrapper.emitted("applied")).toHaveLength(1);
    });

    it("change_FieldThatReloads_LoadsTheFormAgainOnceApplied", async () => {
        const { wrapper, load, submit } = mountHost({
            load: () => Promise.resolve(fragment('<select name="Task.WorkflowTypeId" data-dpd-reload><option value="a">A</option><option value="b">B</option></select><input name="Task.Note" />')),
        });
        await flushPromises();

        // Other fields don't reload the form.
        await wrapper.get("input[name='Task.Note']").trigger("change");
        vi.advanceTimersByTime(600);
        await flushPromises();

        expect(submit).toHaveBeenCalledTimes(1);
        expect(load).toHaveBeenCalledTimes(1);

        await wrapper.get("select").setValue("b");
        vi.advanceTimersByTime(600);
        await flushPromises();

        expect(submit).toHaveBeenCalledTimes(2);
        expect(load).toHaveBeenCalledTimes(2);
    });

    it("input_WithoutChange_DoesNotApplyUntilAsked", async () => {
        const { wrapper, submit } = mountHost();
        await flushPromises();

        await wrapper.get("input").trigger("input");
        vi.advanceTimersByTime(5000);
        expect(submit).not.toHaveBeenCalled();

        // Leaving the activity applies the pending input (apply on blur / selection change).
        const exposed = wrapper.vm as unknown as { apply: () => Promise<boolean> };
        await expect(exposed.apply()).resolves.toBe(true);
        expect(submit).toHaveBeenCalledTimes(1);
    });

    it("apply_NothingChanged_DoesNotPost", async () => {
        const { wrapper, submit } = mountHost();
        await flushPromises();

        await expect((wrapper.vm as unknown as { apply: () => Promise<boolean> }).apply()).resolves.toBe(true);
        expect(submit).not.toHaveBeenCalled();
    });

    it("apply_RichEditors_DispatchesSubmitBeforeCollectingTheForm", async () => {
        const order: string[] = [];
        const { wrapper } = mountHost({
            submit: () => {
                order.push("posted");

                return Promise.resolve({ valid: true });
            },
        });
        await flushPromises();
        window.addEventListener("submit", () => order.push("synced"), { once: true });

        await wrapper.get("input").trigger("input");
        await (wrapper.vm as unknown as { apply: () => Promise<boolean> }).apply();

        expect(order).toEqual(["synced", "posted"]);
    });

    it("apply_InvalidResult_RendersErrorsAndReportsInvalid", async () => {
        const { wrapper } = mountHost({
            submit: () => Promise.resolve({ valid: false, content: '<input name="Activity.Message" value="Bad" /><span class="field-validation-error">Broken</span>', scripts: "", styles: "" }),
        });
        await flushPromises();

        await wrapper.get("input").trigger("input");
        const applied = await (wrapper.vm as unknown as { apply: () => Promise<boolean> }).apply();
        await flushPromises();

        expect(applied).toBe(false);
        expect(wrapper.get(".field-validation-error").text()).toBe("Broken");
        expect((wrapper.vm as unknown as { isInvalid: () => boolean }).isInvalid()).toBe(true);
        expect(wrapper.text()).toContain("Fix the errors to apply the changes.");
        expect(wrapper.emitted("applied")).toBeUndefined();
    });

    it("reload_SameStylesheet_IsInjectedOnce", async () => {
        const load = () => Promise.resolve(fragment("<input name='A' />", '<link href="/codemirror.css" rel="stylesheet" />'));
        const { wrapper } = mountHost({ load });
        await flushPromises();

        await wrapper.setProps({ formKey: "b" });
        await flushPromises();

        expect(document.head.querySelectorAll('link[href="/codemirror.css"]')).toHaveLength(1);
    });

    it("reload_ExistingContent_DispatchesUnmountingBeforeReplacingIt", async () => {
        let call = 0;
        const load = () => Promise.resolve(fragment(`<div class="editor" data-call="${++call}"></div>`));
        const { wrapper } = mountHost({ load });
        await flushPromises();
        const seen: (string | null)[] = [];

        document.addEventListener("oc:editor-unmounting", (event) => {
            seen.push((event.target as Element).querySelector(".editor")?.getAttribute("data-call") ?? null);
        });

        await wrapper.setProps({ formKey: "b" });
        await flushPromises();

        // The event fired while the first editor was still in the container.
        expect(seen).toEqual(["1"]);
        expect(wrapper.get(".editor").attributes("data-call")).toBe("2");
    });

    it("load_Failure_ShowsErrorAndEmits", async () => {
        const { wrapper } = mountHost({ load: () => Promise.reject(new Error("boom")) });
        await flushPromises();

        expect(wrapper.get("[role=alert]").text()).toBe("The form could not be loaded.");
        expect(wrapper.emitted("error")).toHaveLength(1);
    });

    it("apply_ServerAsksToReloadTheEditor_LoadsTheFormAgain", async () => {
        const { wrapper, load } = mountHost({ submit: () => Promise.resolve({ valid: true, reloadEditor: true }) });
        await flushPromises();
        expect(load).toHaveBeenCalledTimes(1);

        await wrapper.get("input").trigger("input");
        await (wrapper.vm as unknown as { apply: () => Promise<boolean> }).apply();
        await flushPromises();

        expect(load).toHaveBeenCalledTimes(2);
    });

    it("change_DispatchedOnTheFormByAListWidget_AppliesAfterTheDebounce", async () => {
        const { wrapper, submit } = mountHost();
        await flushPromises();

        // A list widget adds a row, then reports it with a bubbling "change" on the form.
        wrapper.get("form").element.dispatchEvent(new Event("change", { bubbles: true }));
        vi.advanceTimersByTime(600);
        await flushPromises();

        expect(submit).toHaveBeenCalledTimes(1);
    });
});
