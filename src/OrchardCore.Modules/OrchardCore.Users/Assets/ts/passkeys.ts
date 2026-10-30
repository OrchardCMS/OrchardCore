// Runs the passkey ceremonies of the login page, the two-factor login page and the passkeys page.
// The server exchanges the options and the credentials as JSON, with binary values encoded with Base64Url.

interface CredentialDescriptorJson {
    id: string;
    type: PublicKeyCredentialType;
    transports?: AuthenticatorTransport[];
}

interface CreationOptionsJson extends Omit<PublicKeyCredentialCreationOptions, "challenge" | "user" | "excludeCredentials"> {
    challenge: string;
    user: { id: string; name: string; displayName: string };
    excludeCredentials?: CredentialDescriptorJson[];
}

interface RequestOptionsJson extends Omit<PublicKeyCredentialRequestOptions, "challenge" | "allowCredentials"> {
    challenge: string;
    allowCredentials?: CredentialDescriptorJson[];
}

const base64UrlToBuffer = (value: string): ArrayBuffer => {
    const base64 = value.replace(/-/g, "+").replace(/_/g, "/").padEnd(Math.ceil(value.length / 4) * 4, "=");
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);

    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }

    return bytes.buffer;
};

const bufferToBase64Url = (buffer: ArrayBuffer): string => {
    let binary = "";

    for (const byte of new Uint8Array(buffer)) {
        binary += String.fromCharCode(byte);
    }

    return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
};

const isSupported = (): boolean => typeof window.PublicKeyCredential === "function" && !!navigator.credentials;

const toDescriptors = (descriptors?: CredentialDescriptorJson[]): PublicKeyCredentialDescriptor[] | undefined =>
    descriptors?.map((descriptor) => ({ ...descriptor, id: base64UrlToBuffer(descriptor.id) }));

const parseCreationOptions = (json: CreationOptionsJson): PublicKeyCredentialCreationOptions => ({
    ...json,
    challenge: base64UrlToBuffer(json.challenge),
    user: { ...json.user, id: base64UrlToBuffer(json.user.id) },
    excludeCredentials: toDescriptors(json.excludeCredentials),
});

const parseRequestOptions = (json: RequestOptionsJson): PublicKeyCredentialRequestOptions => ({
    ...json,
    challenge: base64UrlToBuffer(json.challenge),
    allowCredentials: toDescriptors(json.allowCredentials),
});

// Browsers that support 'PublicKeyCredential.toJSON()' produce the same shape.
const serializeCredential = (credential: PublicKeyCredential): string => {
    const toJSON = (credential as { toJSON?: () => unknown }).toJSON;

    if (typeof toJSON === "function") {
        return JSON.stringify(toJSON.call(credential));
    }

    const response = credential.response;
    const json: Record<string, unknown> = {
        id: credential.id,
        rawId: bufferToBase64Url(credential.rawId),
        type: credential.type,
        authenticatorAttachment: credential.authenticatorAttachment,
        clientExtensionResults: credential.getClientExtensionResults(),
    };

    if (response instanceof AuthenticatorAttestationResponse) {
        json.response = {
            clientDataJSON: bufferToBase64Url(response.clientDataJSON),
            attestationObject: bufferToBase64Url(response.attestationObject),
            transports: typeof response.getTransports === "function" ? response.getTransports() : [],
        };
    } else {
        const assertion = response as AuthenticatorAssertionResponse;

        json.response = {
            clientDataJSON: bufferToBase64Url(assertion.clientDataJSON),
            authenticatorData: bufferToBase64Url(assertion.authenticatorData),
            signature: bufferToBase64Url(assertion.signature),
            userHandle: assertion.userHandle ? bufferToBase64Url(assertion.userHandle) : null,
        };
    }

    return JSON.stringify(json);
};

// Fetches the options from the URL of the element, which also holds the antiforgery token.
const fetchOptions = async <T>(element: HTMLElement): Promise<T> => {
    const body = new FormData();
    body.append(element.dataset.antiforgeryName ?? "__RequestVerificationToken", element.dataset.antiforgeryToken ?? "");

    const response = await fetch(element.dataset.optionsUrl ?? "", {
        method: "POST",
        body,
        credentials: "same-origin",
    });

    if (!response.ok) {
        throw new Error(`The passkey options couldn't be loaded (${response.status}).`);
    }

    return (await response.json()) as T;
};

const showError = (element: HTMLElement, message?: string): void => {
    const error = element.querySelector<HTMLElement>("[data-passkey-error]");

    if (error) {
        error.textContent = message ?? element.dataset.errorMessage ?? "";
        error.classList.remove("d-none");
    }
};

const hideError = (element: HTMLElement): void => {
    element.querySelector<HTMLElement>("[data-passkey-error]")?.classList.add("d-none");
};

const isAbortError = (error: unknown): boolean => error instanceof DOMException && error.name === "AbortError";

// Shows the error of a ceremony, unless it was aborted to make way for another one.
const handleError = (element: HTMLElement, error: unknown): void => {
    if (isAbortError(error)) {
        return;
    }

    console.warn("The passkey ceremony failed.", error);
    showError(element);
};

const addField = (form: HTMLFormElement, name: string, value: string): void => {
    const input = document.createElement("input");
    input.type = "hidden";
    input.name = name;
    input.value = value;
    form.appendChild(input);
};

const initLogin = (container: HTMLElement): void => {
    const button = container.querySelector<HTMLButtonElement>("[data-passkey-button]");

    if (!isSupported() || !button) {
        return;
    }

    container.classList.remove("d-none");

    const loginForm = container.closest("form");
    let conditionalRequest: AbortController | null = null;

    const signIn = (credential: PublicKeyCredential): void => {
        const rememberMe = loginForm?.querySelector<HTMLInputElement>('input[type="checkbox"][name$="RememberMe"]');

        // The login form also posts the username and the password, so the passkey is sent with a form of its own.
        const form = document.createElement("form");
        form.method = "post";
        form.action = container.dataset.loginUrl ?? "";
        form.hidden = true;

        addField(form, container.dataset.antiforgeryName ?? "__RequestVerificationToken", container.dataset.antiforgeryToken ?? "");
        addField(form, "Credential", serializeCredential(credential));
        addField(form, "RememberMe", rememberMe?.checked ? "true" : "false");

        document.body.appendChild(form);
        form.submit();
    };

    const requestCredential = async (mediation?: CredentialMediationRequirement, signal?: AbortSignal): Promise<PublicKeyCredential | null> => {
        const options = parseRequestOptions(await fetchOptions<RequestOptionsJson>(container));

        // A newer request may have started while the options were loading.
        if (signal?.aborted) {
            return null;
        }

        return (await navigator.credentials.get({ publicKey: options, mediation, signal })) as PublicKeyCredential | null;
    };

    // Lets the browser offer the passkeys in the autofill of the username field.
    const startConditionalRequest = async (): Promise<void> => {
        const userName = loginForm?.querySelector<HTMLInputElement>('input[name$="UserName"]');
        const publicKeyCredential = window.PublicKeyCredential as unknown as { isConditionalMediationAvailable?: () => Promise<boolean> };

        if (!userName || typeof publicKeyCredential.isConditionalMediationAvailable !== "function" || !(await publicKeyCredential.isConditionalMediationAvailable())) {
            return;
        }

        userName.setAttribute("autocomplete", "username webauthn");

        const request = new AbortController();
        conditionalRequest = request;

        try {
            const credential = await requestCredential("conditional", request.signal);

            if (credential) {
                signIn(credential);
            }
        } catch (error) {
            if (!isAbortError(error)) {
                console.debug("The passkey autofill isn't available.", error);
            }
        }
    };

    button.addEventListener("click", async () => {
        // Only one request can be pending, so the autofill one makes way for the one the user asked for.
        conditionalRequest?.abort();
        conditionalRequest = null;

        hideError(container);
        button.disabled = true;

        try {
            const credential = await requestCredential();

            if (credential) {
                signIn(credential);

                return;
            }
        } catch (error) {
            handleError(container, error);
        }

        button.disabled = false;
        void startConditionalRequest();
    });

    void startConditionalRequest();
};

const initRegistration = (form: HTMLFormElement): void => {
    const button = form.querySelector<HTMLButtonElement>("[data-passkey-button]");
    const credentialInput = form.querySelector<HTMLInputElement>('input[name="Credential"]');

    if (!button || !credentialInput) {
        return;
    }

    if (!isSupported()) {
        document.querySelector("[data-passkey-unsupported]")?.classList.remove("d-none");

        return;
    }

    button.disabled = false;

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        hideError(form);
        button.disabled = true;

        try {
            const options = parseCreationOptions(await fetchOptions<CreationOptionsJson>(form));
            const credential = (await navigator.credentials.create({ publicKey: options })) as PublicKeyCredential | null;

            if (credential) {
                credentialInput.value = serializeCredential(credential);
                form.submit();

                return;
            }
        } catch (error) {
            handleError(form, error);
        }

        button.disabled = false;
    });
};

const initTwoFactor = (form: HTMLFormElement): void => {
    const button = form.querySelector<HTMLButtonElement>("[data-passkey-button]");
    const codeInput = form.querySelector<HTMLInputElement>('input[name="VerificationCode"]');

    if (!button || !codeInput) {
        return;
    }

    if (!isSupported()) {
        showError(form);

        return;
    }

    button.disabled = false;

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        hideError(form);
        button.disabled = true;

        try {
            const options = parseRequestOptions(await fetchOptions<RequestOptionsJson>(form));
            const credential = (await navigator.credentials.get({ publicKey: options })) as PublicKeyCredential | null;

            if (credential) {
                codeInput.value = serializeCredential(credential);
                form.submit();

                return;
            }
        } catch (error) {
            handleError(form, error);
        }

        button.disabled = false;
    });
};

document.querySelectorAll<HTMLElement>("[data-passkey-login]").forEach(initLogin);
document.querySelectorAll<HTMLFormElement>("form[data-passkey-register]").forEach(initRegistration);
document.querySelectorAll<HTMLFormElement>("form[data-passkey-two-factor]").forEach(initTwoFactor);

document.querySelectorAll<HTMLFormElement>("form[data-passkey-confirm]").forEach((form) => {
    form.addEventListener("submit", (event) => {
        if (!window.confirm(form.dataset.passkeyConfirm ?? "")) {
            event.preventDefault();
        }
    });
});

export {};
