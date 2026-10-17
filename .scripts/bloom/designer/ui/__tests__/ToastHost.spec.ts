import { afterEach, describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import ToastHost from "../ToastHost.vue";
import { clearToasts, showToast } from "../toasts";

describe("ToastHost", () => {
    afterEach(() => clearToasts());

    it("render_Toasts_AnnouncesErrorsAssertivelyAndTheRestPolitely", async () => {
        const wrapper = mount(ToastHost);

        showToast({ message: "Saved", variant: "success", timeout: 0 });
        showToast({ message: "Broken", variant: "danger", timeout: 0 });
        showToast({ message: "Careful", variant: "warning", timeout: 0 });
        await wrapper.vm.$nextTick();

        expect(wrapper.get("[data-cy=toasts]").attributes("aria-live")).toBe("polite");
        const toasts = wrapper.findAll("[data-cy=toast]");
        expect(toasts.map((toast) => toast.attributes("role"))).toEqual(["status", "alert", "status"]);
        // The close button stays dark on the light warning background.
        expect(toasts[2].get(".btn-close").classes()).not.toContain("btn-close-white");
        expect(toasts[1].get(".btn-close").classes()).toContain("btn-close-white");
    });
});
