import { afterEach, describe, expect, it, vi } from "vitest";
import observeAndInit from "../observeAndInit";

const flush = () => new Promise((resolve) => setTimeout(resolve, 0));

describe("observeAndInit", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("observeAndInit_ElementsInjectedLater_InitializesEachOnce", async () => {
        document.body.innerHTML = '<div class="spec-existing"></div>';
        const init = vi.fn();

        observeAndInit(".spec-existing, .spec-injected", init);

        const host = document.createElement("div");
        host.innerHTML = '<div class="spec-injected"></div>';
        document.body.appendChild(host);
        await flush();

        // Moving an initialized element re-adds it to the document, but it isn't initialized again.
        document.body.appendChild(host.querySelector(".spec-injected")!);
        await flush();

        expect(init).toHaveBeenCalledTimes(2);
        expect(init.mock.calls.map((call) => (call[0] as HTMLElement).className)).toEqual(["spec-existing", "spec-injected"]);
    });

    it("observeAndInit_ReplacedContent_InitializesNewElements", async () => {
        const host = document.createElement("div");
        document.body.appendChild(host);
        const init = vi.fn();

        observeAndInit(".spec-replaced", init);

        host.innerHTML = '<div class="spec-replaced"></div>';
        await flush();
        host.innerHTML = '<div class="spec-replaced"></div>';
        await flush();

        expect(init).toHaveBeenCalledTimes(2);
    });
});
