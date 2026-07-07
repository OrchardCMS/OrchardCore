// Shared by OpenId's Application Create/Edit forms - both wire the same credential helpers,
// server-settings-driven forbidden-flow collapsing, and flow-dependent field visibility. The two
// forms differ only in how the Client Secret field is revealed (collapse vs plain display) and
// whether switching away from "Confidential" clears an existing secret value (Edit does, since
// it may already hold a real secret; Create never has one yet).
interface OpenIdApplicationSettings {
    configured: boolean;
    allowAuthorizationCodeFlow: boolean;
    allowClientCredentialsFlow: boolean;
    allowImplicitFlow: boolean;
    allowHybridFlow: boolean;
    allowPasswordFlow: boolean;
    allowRefreshTokenFlow: boolean;
    logoutEndpointConfigured: boolean;
}

type ClientSecretToggleMode = "collapse" | "display";

const getElement = <T extends HTMLElement = HTMLElement>(id: string) => document.getElementById(id) as T | null;
const getField = <T extends HTMLElement>(name: string) => document.querySelector<T>(`[data-openid-field="${name}"]`);

const setDisplay = (element: HTMLElement | null, show: boolean) => {
    if (element) {
        element.style.display = show ? "" : "none";
    }
};

const setCollapse = (element: Element | null, action: "show" | "hide") => {
    if (element) {
        bootstrap.Collapse.getOrCreateInstance(element, { toggle: false })[action]();
    }
};

const wireCredentialHelpers = () => {
    const clientIdElement = getField<HTMLInputElement>("ClientId");
    const clientSecretElement = getField<HTMLInputElement>("ClientSecret");
    const toggleClientSecretElement = getElement("toggleClientSecret");
    const copyClientSecretElement = getElement("copyClientSecret");
    const copyClientIdElement = getElement("copyClientId");
    const generateClientIdElement = getElement("generateClientId");
    const generateClientSecretElement = getElement("generateClientSecret");

    if (
        !clientIdElement ||
        !clientSecretElement ||
        !toggleClientSecretElement ||
        !copyClientSecretElement ||
        !copyClientIdElement ||
        !generateClientIdElement ||
        !generateClientSecretElement
    ) {
        return;
    }

    toggleClientSecretElement.addEventListener("click", () => {
        togglePasswordVisibility(clientSecretElement, toggleClientSecretElement);
    });

    copyClientIdElement.addEventListener("click", () => {
        copyToClipboard(clientIdElement.value);
    });

    copyClientSecretElement.addEventListener("click", () => {
        copyToClipboard(clientSecretElement.value);
    });

    generateClientIdElement.addEventListener("click", () => {
        clientIdElement.value = randomUUID({ includeHyphens: false });
    });

    generateClientSecretElement.addEventListener("click", () => {
        clientSecretElement.value = generateStrongPassword();
    });
};

// When settings are configured, mirrors the original per-flow branching exactly - including two
// pre-existing dead-code quirks, preserved rather than silently fixed: the Hybrid Flow branch
// looks up "AllowHybridFlowFlow" (a typo, always null, so its checkbox is never actually cleared
// here), and the Logout Endpoint branch sets `.checked` on the fieldset *div* itself rather than
// the actual checkbox (a no-op, since divs have no such property).
const refreshForbiddenFlows = (settings: OpenIdApplicationSettings) => {
    if (!settings.configured) {
        setCollapse(getElement("AllowAuthorizationCodeFlowFieldSet"), "show");
        setCollapse(getElement("AllowClientCredentialsFlowFieldSet"), "show");
        setCollapse(getElement("AllowImplicitFlowFieldSet"), "show");
        setCollapse(getElement("AllowPasswordFlowFieldSet"), "show");
        setCollapse(getElement("AllowRefreshTokenFlowFieldSet"), "show");
        setCollapse(getElement("AllowLogoutEndpointFieldSet"), "show");
        return;
    }

    if (settings.allowAuthorizationCodeFlow) {
        setCollapse(getElement("AllowAuthorizationCodeFlowFieldSet"), "show");
    } else {
        setCollapse(getElement("AllowAuthorizationCodeFlowFieldSet"), "hide");
        const checkbox = getElement<HTMLInputElement>("AllowAuthorizationCodeFlow");
        if (checkbox) {
            checkbox.checked = false;
        }
    }

    if (settings.allowClientCredentialsFlow) {
        setCollapse(getElement("AllowClientCredentialsFlowFieldSet"), "show");
    } else {
        setCollapse(getElement("AllowClientCredentialsFlowFieldSet"), "hide");
        const checkbox = getElement<HTMLInputElement>("AllowClientCredentialsFlow");
        if (checkbox) {
            checkbox.checked = false;
        }
    }

    if (settings.allowImplicitFlow) {
        setCollapse(getElement("AllowImplicitFlowFieldSet"), "show");
    } else {
        setCollapse(getElement("AllowImplicitFlowFieldSet"), "hide");
        const checkbox = getElement<HTMLInputElement>("AllowImplicitFlow");
        if (checkbox) {
            checkbox.checked = false;
        }
    }

    if (settings.allowHybridFlow) {
        setCollapse(getElement("AllowHybridFlowFieldSet"), "show");
    } else {
        setCollapse(getElement("AllowHybridFlowFieldSet"), "hide");
        getElement("AllowHybridFlowFlow");
    }

    if (settings.allowPasswordFlow) {
        setCollapse(getElement("AllowPasswordFlowFieldSet"), "show");
    } else {
        setCollapse(getElement("AllowPasswordFlowFieldSet"), "hide");
        const checkbox = getElement<HTMLInputElement>("AllowPasswordFlow");
        if (checkbox) {
            checkbox.checked = false;
        }
    }

    if (settings.allowRefreshTokenFlow) {
        setCollapse(getElement("AllowRefreshTokenFlowFieldSet"), "show");
    } else {
        setCollapse(getElement("AllowRefreshTokenFlowFieldSet"), "hide");
        const checkbox = getElement<HTMLInputElement>("AllowRefreshTokenFlow");
        if (checkbox) {
            checkbox.checked = false;
        }
    }

    setCollapse(getElement("AllowLogoutEndpointFieldSet"), settings.logoutEndpointConfigured ? "show" : "hide");
};

const refreshClientSecret = (
    defaultType: string | undefined,
    confidentialValue: string,
    toggleMode: ClientSecretToggleMode,
    clearSecretOnNonConfidential: boolean,
) => {
    const type = getField<HTMLSelectElement>("Type");
    const secret = getField<HTMLInputElement>("ClientSecret");
    const secretWrapper = getElement("ClientSecretWrapper");
    const allowClientCredentialsFlow = getElement<HTMLInputElement>("AllowClientCredentialsFlow");

    if (!type || !allowClientCredentialsFlow) {
        return;
    }

    const isConfidential = type.value === confidentialValue;

    if (toggleMode === "collapse") {
        setCollapse(secretWrapper, isConfidential ? "show" : "hide");
    } else {
        setDisplay(secretWrapper, isConfidential);
    }

    if (isConfidential) {
        allowClientCredentialsFlow.disabled = false;
    } else {
        if (clearSecretOnNonConfidential && secret) {
            secret.value = "";
        }
        allowClientCredentialsFlow.disabled = true;
        allowClientCredentialsFlow.checked = false;
    }

    const clientSecretHints = document.querySelectorAll<HTMLElement>(
        "#AllowPasswordFlowRecommendedHint, #AllowAuthorizationCodeFlowRecommendedHint, #AllowImplicitFlowRecommendedHint, #AllowRefreshTokenFlowRecommendedHint",
    );

    if (defaultType === type.value) {
        return;
    }

    if (isConfidential) {
        clientSecretHints.forEach((element) => {
            element.innerText = element.innerText.replace("client_id, ", "client_id, client_secret, ");
        });
    } else {
        clientSecretHints.forEach((element) => {
            element.innerText = element.innerText.replace("client_id, client_secret, ", "client_id, ");
        });
    }
};

const refreshOfflineAccessTip = (defaultValue?: boolean) => {
    const offlineAccessHints = document.querySelectorAll<HTMLElement>(
        "#AllowPasswordFlowRecommendedHint, #AllowAuthorizationCodeFlowRecommendedHint",
    );
    const allowRefreshTokenFlow = getElement<HTMLInputElement>("AllowRefreshTokenFlow");

    if (!allowRefreshTokenFlow || defaultValue === allowRefreshTokenFlow.checked) {
        return;
    }

    if (allowRefreshTokenFlow.checked) {
        offlineAccessHints.forEach((element) => {
            element.innerText = element.innerText.replace("roles", "roles, offline_access");
        });
    } else {
        offlineAccessHints.forEach((element) => {
            element.innerText = element.innerText.replace(", offline_access", "");
        });
    }
};

const refreshRoleGroup = () => {
    const allowClientCredentialsFlow = getElement<HTMLInputElement>("AllowClientCredentialsFlow");
    setCollapse(getElement("RoleGroup"), allowClientCredentialsFlow?.checked ? "show" : "hide");
};

const refreshAllowRefreshTokenFlowVisibility = () => {
    const allowRefreshTokenFlow = getElement<HTMLInputElement>("AllowRefreshTokenFlow");
    const allowPasswordFlow = getElement<HTMLInputElement>("AllowPasswordFlow");
    const allowAuthorizationCodeFlow = getElement<HTMLInputElement>("AllowAuthorizationCodeFlow");
    const allowHybridFlow = getElement<HTMLInputElement>("AllowHybridFlow");

    if (!allowRefreshTokenFlow) {
        return;
    }

    if (allowPasswordFlow?.checked || allowAuthorizationCodeFlow?.checked || allowHybridFlow?.checked) {
        allowRefreshTokenFlow.disabled = false;
    } else {
        allowRefreshTokenFlow.disabled = true;
        allowRefreshTokenFlow.checked = false;
        setCollapse(getElement("AllowRefreshTokenFlowRecommendedHint"), "hide");
        refreshOfflineAccessTip();
    }
};

const refreshRedirectSettings = () => {
    const redirectSection = getElement("RedirectSection");
    const skipConsent = getElement<HTMLInputElement>("SkipConsent");
    const postLogoutRedirectUris = getElement("postLogoutRedirectUris");
    const allowImplicitFlow = getElement<HTMLInputElement>("AllowImplicitFlow");
    const allowAuthorizationCodeFlow = getElement<HTMLInputElement>("AllowAuthorizationCodeFlow");
    const allowHybridFlow = getElement<HTMLInputElement>("AllowHybridFlow");
    const allowLogoutEndpoint = getElement<HTMLInputElement>("AllowLogoutEndpoint");

    if (allowImplicitFlow?.checked || allowAuthorizationCodeFlow?.checked || allowHybridFlow?.checked) {
        setCollapse(redirectSection, "show");
        setCollapse(postLogoutRedirectUris, allowLogoutEndpoint?.checked ? "show" : "hide");
    } else {
        if (skipConsent) {
            skipConsent.checked = false;
        }
        setCollapse(redirectSection, "hide");
        if (allowLogoutEndpoint) {
            allowLogoutEndpoint.checked = false;
        }
        setCollapse(postLogoutRedirectUris, "hide");
    }
};

const refreshFlows = () => {
    refreshRoleGroup();
    refreshAllowRefreshTokenFlowVisibility();
    refreshRedirectSettings();
};

export interface OpenIdApplicationEditorOptions {
    settings: OpenIdApplicationSettings;
    clientSecretToggleMode: ClientSecretToggleMode;
    clearSecretOnNonConfidential: boolean;
}

const initOpenIdApplicationEditor = (options: OpenIdApplicationEditorOptions) => {
    const confidentialOption = document.querySelector<HTMLOptionElement>("[data-is-confidential]");
    const confidentialValue = confidentialOption?.value ?? "";
    const type = getField<HTMLSelectElement>("Type");

    wireCredentialHelpers();

    refreshForbiddenFlows(options.settings);
    refreshClientSecret(
        confidentialValue,
        confidentialValue,
        options.clientSecretToggleMode,
        options.clearSecretOnNonConfidential,
    );
    refreshFlows();
    refreshOfflineAccessTip(false);

    type?.addEventListener("change", () => {
        refreshClientSecret(undefined, confidentialValue, options.clientSecretToggleMode, options.clearSecretOnNonConfidential);
    });

    getElement<HTMLInputElement>("AllowRefreshTokenFlow")?.addEventListener("change", () => {
        refreshOfflineAccessTip();
    });

    ["AllowClientCredentialsFlow", "AllowPasswordFlow", "AllowAuthorizationCodeFlow", "AllowImplicitFlow", "AllowHybridFlow", "AllowRefreshTokenFlow"]
        .map((id) => getElement(id))
        .filter((element) => element !== null)
        .forEach((element) => {
            element.addEventListener("change", refreshFlows);
        });
};

export default initOpenIdApplicationEditor;
