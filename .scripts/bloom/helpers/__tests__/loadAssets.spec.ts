import { afterEach, describe, expect, it } from "vitest";
import { loadScripts, loadStyles } from "../loadAssets";

describe("loadAssets", () => {
    afterEach(() => {
        document.head.innerHTML = "";
        document.body.innerHTML = "";
    });

    it("loadScripts_ClassicThenModule_WaitsForClassicScriptsInOrder", async () => {
        const events: string[] = [];
        const waitFor = async (script: HTMLScriptElement) => {
            events.push(`wait:${script.getAttribute("src")}`);
            await Promise.resolve();
            events.push(`loaded:${script.getAttribute("src")}`);
        };

        await loadScripts('<script src="/a.js"></script><script src="/b.js"></script><script src="/c.js" type="module"></script>', { waitFor });

        expect(events).toEqual(["wait:/a.js", "loaded:/a.js", "wait:/b.js", "loaded:/b.js"]);
        expect(Array.from(document.scripts).map((script) => script.getAttribute("src"))).toEqual(["/a.js", "/b.js", "/c.js"]);
        expect(document.querySelector<HTMLScriptElement>('script[src="/a.js"]')!.async).toBe(false);
        expect(document.querySelector('script[src="/c.js"]')!.getAttribute("type")).toBe("module");
    });

    it("loadScripts_ScriptAlreadyOnPage_IsNotAddedAgain", async () => {
        const waitFor = () => Promise.resolve();

        await loadScripts('<script src="/lib.js"></script>', { waitFor });
        await loadScripts('<script src="/lib.js"></script><script src="/editor.js" type="module"></script>', { waitFor });
        await loadScripts('<script src="/editor.js" type="module"></script>', { waitFor });

        expect(Array.from(document.scripts).map((script) => script.getAttribute("src"))).toEqual(["/lib.js", "/editor.js"]);
    });

    it("loadScripts_InlineScript_RunsEveryTime", async () => {
        // jsdom runs scripts in the context of its own window, so the script reports to the shared document.
        let runs = 0;
        const onRun = () => runs++;
        const inline = '<script>document.dispatchEvent(new Event("inline-script-ran"));</script>';
        document.addEventListener("inline-script-ran", onRun);

        try {
            await loadScripts(inline);
            await loadScripts(inline);

            expect(runs).toBe(2);
            expect(Array.from(document.scripts).filter((script) => !script.src)).toHaveLength(2);
        } finally {
            document.removeEventListener("inline-script-ran", onRun);
        }
    });

    it("loadStyles_SameStylesheetTwice_AddsItOnce", () => {
        loadStyles('<link href="/codemirror.css" rel="stylesheet" type="text/css" /><style>.a{color:red}</style>');
        loadStyles('<link href="/codemirror.css" rel="stylesheet" type="text/css" /><style>.a{color:red}</style>');

        expect(document.head.querySelectorAll("link[rel=stylesheet]")).toHaveLength(1);
        expect(document.head.querySelectorAll("style")).toHaveLength(1);
    });
});
