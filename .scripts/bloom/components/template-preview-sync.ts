// Shared by Template Create/Edit - stores the live-editing form data to localStorage so the
// separate Preview popup window (Templates/Preview/Index.cshtml) can pick it up and re-render.
// The two callers pass different trigger event names (Create's own copy used "change"; Edit's
// second, largely-vestigial copy used "propertychange", a legacy IE-only event that doesn't fire
// in any modern browser) - preserved exactly rather than unified, since Edit's *actual* live
// preview already runs through its own debounced onDidChangeContent wiring elsewhere.
const toQueryString = (data: Record<string, string>) =>
    Object.keys(data)
        .map((key) => `${encodeURIComponent(key)}=${encodeURIComponent(data[key])}`)
        .join("&");

export const initializeTemplatePreview = (element: HTMLInputElement | HTMLTextAreaElement | null, changeEventName: string) => {
    if (!element) {
        return;
    }

    const antiforgerytoken = document.querySelector<HTMLInputElement>("[name='__RequestVerificationToken']")?.value ?? "";

    const sendFormData = (target: HTMLInputElement | HTMLTextAreaElement) => {
        const formData = {
            Name: target.name,
            Content: target.value,
            __RequestVerificationToken: antiforgerytoken,
        };
        localStorage.setItem("OrchardCore.templates", JSON.stringify(toQueryString(formData)));
    };

    window.addEventListener("storage", (ev) => {
        if (ev.key !== "OrchardCore.templates:ready") {
            return;
        }

        sendFormData(element);
    });

    element.addEventListener(changeEventName, () => sendFormData(element));

    window.addEventListener("unload", () => {
        localStorage.removeItem("OrchardCore.templates");
        localStorage.setItem("OrchardCore.templates:not-connected", "");
        localStorage.removeItem("OrchardCore.templates:not-connected");
    });
};
