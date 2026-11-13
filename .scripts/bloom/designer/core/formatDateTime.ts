/**
 * Formats a UTC date and time from the server in the page's language and the browser's time zone. A value
 * without a time zone designator is read as UTC.
 */
export const formatDateTime = (utc: string | null | undefined) => {
    if (!utc) {
        return "";
    }

    const date = new Date(/(Z|[+-]\d{2}:?\d{2})$/i.test(utc) ? utc : `${utc}Z`);

    if (Number.isNaN(date.getTime())) {
        return utc;
    }

    return new Intl.DateTimeFormat(document.documentElement.lang || undefined, { dateStyle: "medium", timeStyle: "short" }).format(date);
};
