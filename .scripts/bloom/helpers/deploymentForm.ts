type DeploymentFormFields = Record<string, string | string[]>;

const appendHiddenInput = (form: HTMLFormElement, name: string, value: string) => {
    const input = document.createElement("input");
    input.type = "hidden";
    input.name = name;
    input.value = value;
    form.appendChild(input);
};

// Builds a throwaway <form> POSTing to targetUrl (its own query string becomes hidden fields -
// "trusting hrefs in the page here", matching the original inline comment) plus the given extra
// fields, then submits it immediately. Shared by every "add/export this content to a deployment
// plan/target" modal, which otherwise all duplicated this exact ~30-line form-building dance.
//
// The antiforgery token is attached if present but its absence no longer blocks the submit (one
// of the 4 original copies aborted entirely without it, the other 3 didn't) - unified to the
// lenient behavior since the real enforcement is server-side ValidateAntiForgeryToken regardless
// of what the client attaches.
const submitDeploymentForm = (targetUrl: string, fields: DeploymentFormFields) => {
    const magicToken = document.querySelector<HTMLInputElement>("input[name=__RequestVerificationToken]");
    const [action, queryString] = targetUrl.split("?");
    const form = document.createElement("form");
    form.action = action;
    form.method = "POST";

    if (magicToken) {
        form.appendChild(magicToken.cloneNode(true) as HTMLInputElement);
    }

    queryString?.split("&").forEach((pair) => {
        const [key, value] = pair.split("=");

        appendHiddenInput(form, decodeURIComponent(key), decodeURIComponent(value));
    });

    Object.entries(fields).forEach(([name, value]) => {
        if (Array.isArray(value)) {
            value.forEach((item, index) => appendHiddenInput(form, `${name}[${index}]`, item));
        } else {
            appendHiddenInput(form, name, value);
        }
    });

    document.body.appendChild(form);
    form.submit();
};

// Wires the "when this modal is shown, attach a one-time click handler to each of its select
// buttons that builds and submits the deployment form" pattern shared by every deployment modal.
// `getFields` computes the extra form fields from the triggering show.bs.modal event (e.g. the
// single content item id from the button that opened the modal, or the currently-checked bulk
// item ids) - returning null/undefined skips wiring the buttons entirely for this showing.
const wireDeploymentModalButtons = (
    modalId: string,
    buttonSelector: string,
    getFields: (event: Event) => DeploymentFormFields | null | undefined,
) => {
    const modal = document.getElementById(modalId);

    modal?.addEventListener("show.bs.modal", (event) => {
        const fields = getFields(event);

        if (!fields) {
            return;
        }

        document.querySelectorAll<HTMLElement>(buttonSelector).forEach((button) => {
            button.addEventListener(
                "click",
                () => {
                    const targetUrl = button.dataset.targetUrl;

                    if (targetUrl) {
                        submitDeploymentForm(targetUrl, fields);
                    }
                },
                { once: true },
            );
        });
    });
};

export { submitDeploymentForm, wireDeploymentModalButtons };
