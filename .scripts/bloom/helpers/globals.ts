import removeDiacritics from "./removeDiacritics";

const getTenantName = () => document.documentElement.getAttribute("data-tenant") || "default";

const getTechnicalName = (name: string) => {
    let result = "",
        c;

    if (!name || name.length == 0) {
        return "";
    }

    name = removeDiacritics(name);

    for (let i = 0; i < name.length; i++) {
        c = name[i];
        if (isLetter(c) || (isNumber(c) && i > 0)) {
            result += c;
        }
    }

    return result;
};

const isLetter = (str: string) => {
    return str.length === 1 && str.match(/[a-z]/i);
};

const isNumber = (str: string) => {
    return str.length === 1 && str.match(/[0-9]/i);
};

const getAntiForgeryToken = (): string | null => {
    const input = document.querySelector<HTMLInputElement>(
        'input[name="__RequestVerificationToken"]'
    );
    return input?.value ?? null;
};

/**
 * Returns the tenant path base (e.g. "/authserver", "/tenant") from the current page.
 * Reads from a `<script type="application/json">` element whose id ends with `-data`
 * and contains a `pathBase` property, or falls back to an empty string (root tenant).
 */
const getTenantPathBase = (dataElementId?: string): string => {
    if (dataElementId) {
        const el = document.getElementById(dataElementId);
        if (el) {
            try { return JSON.parse(el.textContent || "{}").pathBase || ""; }
            catch { return ""; }
        }
    }

    // Fallback: scan for any script[type="application/json"][id$="-data"] with a pathBase.
    const scripts = document.querySelectorAll<HTMLScriptElement>('script[type="application/json"][id$="-data"]');
    for (const script of scripts) {
        try {
            const data = JSON.parse(script.textContent || "{}");
            if (typeof data.pathBase === "string") {
                return data.pathBase;
            }
        }
        catch { /* skip malformed JSON */ }
    }

    return "";
};

export { getAntiForgeryToken, getTenantName, getTenantPathBase, getTechnicalName, isLetter, isNumber };
