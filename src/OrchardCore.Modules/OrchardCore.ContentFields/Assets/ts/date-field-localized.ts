export {};

import flatpickr from "flatpickr";
import "flatpickr/dist/flatpickr.css";
// Parcel can't code-split a dynamic import() whose path is only known at
// runtime (the language code), so all locales are pulled in via this single
// bundled index rather than lazily importing "flatpickr/dist/l10n/<lang>.js"
// per language - the combined size is modest once minified/gzipped, and it
// avoids a broken relative path a per-language dynamic import would need.
import localeMap from "flatpickr/dist/l10n/index.js";
import type { CustomLocale } from "flatpickr/dist/types/locale";

declare global {
    interface Window {
        initDateFieldLocalized: (inputId: string, language: string, shortDatePattern: string) => void;
    }
}

// Maps .NET custom date format tokens (e.g. "dd/MM/yyyy", the current UI
// culture's ShortDatePattern) to flatpickr's own format-token syntax (e.g.
// "d/m/Y"), so the field displays dates the way the rest of the admin
// already does. A single pass with longest-token-first alternation avoids
// a token replacement (e.g. "dd" -> "d") being re-matched by a later,
// shorter rule (e.g. "d" -> "j") meant for the original pattern.
const NET_TO_FLATPICKR_TOKENS: Record<string, string> = {
    dddd: "l",
    ddd: "D",
    dd: "d",
    d: "j",
    MMMM: "F",
    MMM: "M",
    MM: "m",
    M: "n",
    yyyy: "Y",
    yy: "y",
};

const NET_TOKEN_PATTERN = /dddd|ddd|dd|d|MMMM|MMM|MM|M|yyyy|yy/g;

function toFlatpickrFormat(pattern: string): string {
    return pattern.replace(NET_TOKEN_PATTERN, (token) => NET_TO_FLATPICKR_TOKENS[token]);
}

window.initDateFieldLocalized = function initDateFieldLocalized(inputId: string, language: string, shortDatePattern: string) {
    const input = document.getElementById(inputId) as HTMLInputElement | null;

    if (!input) {
        return;
    }

    const altFormat = toFlatpickrFormat(shortDatePattern);
    const locale: CustomLocale | undefined = (localeMap as Record<string, CustomLocale>)[language];

    flatpickr(input, {
        // The underlying <input>'s posted value always stays ISO 8601 (matching
        // what a native <input type="date"> submits), regardless of locale, so
        // it round-trips through ASP.NET Core's default DateTime model binder
        // unambiguously. altInput shows a second, locale-formatted text field
        // for the admin to actually read/type.
        dateFormat: "Y-m-d",
        altInput: true,
        altFormat,
        locale,
    });
};
