// A copy of .scripts/bloom/helpers/loadAssets.ts (added by the Workflows designer).
// TODO: import it from @bloom/helpers/loadAssets once that helper is on main.

// Loads the scripts and stylesheets of a server-rendered fragment (for example an editor returned as
// { content, scripts, styles }) into the current page. Unlike `evalScripts`, scripts run in document order:
// script-inserted external classic scripts are asynchronous by default, so a module that depends on a
// global set by an earlier classic script (Monaco's loader, CodeMirror and its modes) could otherwise run
// first. Classic scripts and stylesheets that the page already has are skipped, because running a library
// twice resets its global state; module scripts are run once per page by the browser anyway.

const parse = (html: string) => {
    const template = document.createElement("template");
    template.innerHTML = html;

    return template.content;
};

const hasScript = (src: string) => Array.from(document.scripts).some((script) => script.getAttribute("src") === src);

const hasStylesheet = (href: string) =>
    Array.from(document.querySelectorAll<HTMLLinkElement>("link[rel=stylesheet]")).some((link) => link.getAttribute("href") === href);

const hasInlineStyle = (css: string) =>
    Array.from(document.querySelectorAll<HTMLStyleElement>("style[data-oc-fragment-style]")).some((style) => style.textContent === css);

const waitForLoad = (script: HTMLScriptElement) =>
    new Promise<void>((resolve) => {
        script.addEventListener("load", () => resolve(), { once: true });
        script.addEventListener("error", () => resolve(), { once: true });
    });

export interface LoadScriptsOptions {
    /**
     * Where the scripts are appended. Defaults to the document body.
     */
    target?: HTMLElement;
    /**
     * Resolves when an external classic script has loaded (or failed). Replaceable for tests.
     */
    waitFor?: (script: HTMLScriptElement) => Promise<void>;
}

/**
 * Runs the <script> elements of `html` in order. Resolves once every external classic script has loaded.
 */
export const loadScripts = async (html: string, options: LoadScriptsOptions = {}) => {
    const target = options.target ?? document.body;
    const waitFor = options.waitFor ?? waitForLoad;

    for (const source of Array.from(parse(html).querySelectorAll("script"))) {
        const src = source.getAttribute("src");

        if (src && hasScript(src)) {
            continue;
        }

        const script = document.createElement("script");

        for (const attribute of Array.from(source.attributes)) {
            script.setAttribute(attribute.name, attribute.value);
        }

        script.textContent = source.textContent;

        const isExternalClassic = !!src && script.type !== "module";

        if (isExternalClassic) {
            script.async = false;
            const loaded = waitFor(script);
            target.appendChild(script);
            await loaded;
        } else {
            target.appendChild(script);
        }
    }
};

/**
 * Adds the stylesheets of `html` to the page head, each one once.
 */
export const loadStyles = (html: string, target: HTMLElement = document.head) => {
    for (const element of Array.from(parse(html).querySelectorAll<HTMLElement>("link[rel=stylesheet], style"))) {
        if (element instanceof HTMLLinkElement) {
            const href = element.getAttribute("href");

            if (href && !hasStylesheet(href)) {
                target.appendChild(element.cloneNode(true));
            }
        } else {
            const css = element.textContent ?? "";

            if (!hasInlineStyle(css)) {
                const style = element.cloneNode(true) as HTMLStyleElement;
                style.dataset.ocFragmentStyle = "";
                target.appendChild(style);
            }
        }
    }
};
