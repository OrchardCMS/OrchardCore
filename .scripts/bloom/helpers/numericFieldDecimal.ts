// The server's current UI culture may use a different decimal separator than the JS/browser
// locale (fixed to "."), so numeric field widgets (Range/Slider/Spinner) need to convert between
// the two - both directions read from data-js-decimal-separator/data-server-decimal-separator on
// the field's own markup rather than being baked into a compiled bundle at build time, since the
// separator genuinely varies per-request (per current locale), not just per compile.
export const toServerDecimal = (value: string, element: HTMLElement): string => {
    const jsSeparator = element.dataset.jsDecimalSeparator;
    const serverSeparator = element.dataset.serverDecimalSeparator;
    return jsSeparator && serverSeparator && jsSeparator !== serverSeparator
        ? value.replace(jsSeparator, serverSeparator)
        : value;
};

export const toJsDecimal = (value: string, element: HTMLElement): string => {
    const jsSeparator = element.dataset.jsDecimalSeparator;
    const serverSeparator = element.dataset.serverDecimalSeparator;
    return jsSeparator && serverSeparator && jsSeparator !== serverSeparator
        ? value.replace(serverSeparator, jsSeparator)
        : value;
};
