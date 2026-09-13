const getField = <T extends HTMLElement>(name: string) =>
    document.querySelector<T>(`[data-openid-field="${name}"]`);

const setDisplay = (element: HTMLElement | null, show: boolean) => {
    if (element) {
        element.style.display = show ? "" : "none";
    }
};

const setCollapse = (element: Element | null | undefined, action: "show" | "hide") => {
    if (element) {
        bootstrap.Collapse.getOrCreateInstance(element, { toggle: false })[action]();
    }
};

const enableTokenEndpoint = getField<HTMLInputElement>("EnableTokenEndpoint");
const enableAuthorizationEndpoint = getField<HTMLInputElement>("EnableAuthorizationEndpoint");
const allowPasswordFlow = getField<HTMLInputElement>("AllowPasswordFlow");
const allowAuthorizationCodeFlow = getField<HTMLInputElement>("AllowAuthorizationCodeFlow");
const allowImplicitFlow = getField<HTMLInputElement>("AllowImplicitFlow");
const allowHybridFlow = getField<HTMLInputElement>("AllowHybridFlow");
const allowRefreshTokenFlow = getField<HTMLInputElement>("AllowRefreshTokenFlow");
const allowClientCredentialsFlow = getField<HTMLInputElement>("AllowClientCredentialsFlow");
const accessTokenFormat = getField<HTMLSelectElement>("AccessTokenFormat");
const disableAccessTokenEncryption = getField<HTMLInputElement>("DisableAccessTokenEncryption");
const encryptionCertificateStoreLocation = getField<HTMLSelectElement>("EncryptionCertificateStoreLocation");
const encryptionCertificateStoreName = getField<HTMLSelectElement>("EncryptionCertificateStoreName");
const encryptionCertificateThumbprint = getField<HTMLSelectElement>("EncryptionCertificateThumbprint");
const signingCertificateStoreLocation = getField<HTMLSelectElement>("SigningCertificateStoreLocation");
const signingCertificateStoreName = getField<HTMLSelectElement>("SigningCertificateStoreName");
const signingCertificateThumbprint = getField<HTMLSelectElement>("SigningCertificateThumbprint");

const jsonWebTokenValue = accessTokenFormat?.querySelector<HTMLOptionElement>("[data-is-json-web-token]")?.value;

function refreshEncryptionCertificates() {
    if (!encryptionCertificateStoreLocation || !encryptionCertificateStoreName || !encryptionCertificateThumbprint) {
        return;
    }

    const storeNameField = document.getElementById("EncryptionCertificateStoreNameField");
    const thumbprintField = document.getElementById("EncryptionCertificateThumbprintField");

    if (encryptionCertificateStoreLocation.value) {
        setDisplay(storeNameField, true);

        if (encryptionCertificateStoreName.value) {
            setDisplay(thumbprintField, true);
            encryptionCertificateThumbprint.querySelectorAll<HTMLOptionElement>("option").forEach((option) => {
                if (option.value !== "") {
                    setDisplay(option, false);
                }

                if (
                    option.getAttribute("data-StoreLocation") === encryptionCertificateStoreLocation.value &&
                    option.getAttribute("data-StoreName") === encryptionCertificateStoreName.value
                ) {
                    setDisplay(option, true);
                }
            });
        } else {
            setDisplay(thumbprintField, false);
            encryptionCertificateThumbprint.value = "";
        }
    } else {
        setDisplay(storeNameField, false);
        encryptionCertificateStoreName.value = "";
        setDisplay(thumbprintField, false);
        encryptionCertificateThumbprint.value = "";
    }
}

function refreshSigningCertificates() {
    if (!signingCertificateStoreLocation || !signingCertificateStoreName || !signingCertificateThumbprint) {
        return;
    }

    const storeNameField = document.getElementById("SigningCertificateStoreNameField");
    const thumbprintField = document.getElementById("SigningCertificateThumbprintField");

    if (signingCertificateStoreLocation.value) {
        setDisplay(storeNameField, true);

        if (signingCertificateStoreName.value) {
            setDisplay(thumbprintField, true);
            signingCertificateThumbprint.querySelectorAll<HTMLOptionElement>("option").forEach((option) => {
                if (option.value !== "") {
                    setDisplay(option, false);
                }

                if (
                    option.getAttribute("data-StoreLocation") === signingCertificateStoreLocation.value &&
                    option.getAttribute("data-StoreName") === signingCertificateStoreName.value
                ) {
                    setDisplay(option, true);
                }
            });
        } else {
            setDisplay(thumbprintField, false);
            signingCertificateThumbprint.value = "";
        }
    } else {
        setDisplay(storeNameField, false);
        signingCertificateStoreName.value = "";
        setDisplay(thumbprintField, false);
        signingCertificateThumbprint.value = "";
    }
}

function refreshDisableAccessTokenEncryption() {
    if (!accessTokenFormat || !disableAccessTokenEncryption) {
        return;
    }

    if (accessTokenFormat.value === jsonWebTokenValue) {
        disableAccessTokenEncryption.disabled = false;
    } else {
        disableAccessTokenEncryption.disabled = true;
        disableAccessTokenEncryption.checked = false;
    }
}

function refreshEnableTokenEndpoint() {
    if (!enableTokenEndpoint || !allowPasswordFlow || !allowClientCredentialsFlow) {
        return;
    }

    if (!enableTokenEndpoint.checked) {
        allowPasswordFlow.checked = false;
        allowClientCredentialsFlow.checked = false;
    }

    const showOrHide = enableTokenEndpoint.checked ? "show" : "hide";
    setCollapse(allowPasswordFlow.closest(".collapse"), showOrHide);
    setCollapse(allowClientCredentialsFlow.closest(".collapse"), showOrHide);
}

function refreshEnableAuthorizationEndpoint() {
    if (!enableAuthorizationEndpoint || !allowImplicitFlow) {
        return;
    }

    if (!enableAuthorizationEndpoint.checked) {
        allowImplicitFlow.checked = false;
    }

    setCollapse(allowImplicitFlow.closest(".collapse"), enableAuthorizationEndpoint.checked ? "show" : "hide");
}

function refreshAllowAuthorizationCodeFlowVisibility() {
    if (!allowAuthorizationCodeFlow || !enableTokenEndpoint || !enableAuthorizationEndpoint) {
        return;
    }

    if (enableTokenEndpoint.checked && enableAuthorizationEndpoint.checked) {
        setCollapse(allowAuthorizationCodeFlow.closest(".collapse"), "show");
    } else {
        allowAuthorizationCodeFlow.checked = false;
        setCollapse(allowAuthorizationCodeFlow.closest(".collapse"), "hide");
    }
}

function refreshAllowHybridFlowVisibility() {
    if (!allowHybridFlow || !enableTokenEndpoint || !enableAuthorizationEndpoint) {
        return;
    }

    if (enableTokenEndpoint.checked && enableAuthorizationEndpoint.checked) {
        setCollapse(allowHybridFlow.closest(".collapse"), "show");
    } else {
        allowHybridFlow.checked = false;
        setCollapse(allowHybridFlow.closest(".collapse"), "hide");
    }
}

function refreshAllowRefreshTokenFlowVisibility() {
    if (!allowRefreshTokenFlow || !enableTokenEndpoint || !allowPasswordFlow || !allowAuthorizationCodeFlow || !allowHybridFlow) {
        return;
    }

    if (
        enableTokenEndpoint.checked &&
        (allowPasswordFlow.checked || allowAuthorizationCodeFlow.checked || allowHybridFlow.checked)
    ) {
        setCollapse(allowRefreshTokenFlow.closest(".collapse"), "show");
    } else {
        allowRefreshTokenFlow.checked = false;
        setCollapse(allowRefreshTokenFlow.closest(".collapse"), "hide");
    }
}

function refreshEndpoints() {
    refreshEnableTokenEndpoint();
    refreshAllowAuthorizationCodeFlowVisibility();
    refreshEnableAuthorizationEndpoint();
    refreshAllowHybridFlowVisibility();
    refreshAllowRefreshTokenFlowVisibility();
}

refreshEncryptionCertificates();
refreshSigningCertificates();
refreshEndpoints();
refreshDisableAccessTokenEncryption();

accessTokenFormat?.addEventListener("change", refreshDisableAccessTokenEncryption);
encryptionCertificateStoreLocation?.addEventListener("change", refreshEncryptionCertificates);
encryptionCertificateStoreName?.addEventListener("change", refreshEncryptionCertificates);
signingCertificateStoreLocation?.addEventListener("change", refreshSigningCertificates);
signingCertificateStoreName?.addEventListener("change", refreshSigningCertificates);

[enableTokenEndpoint, enableAuthorizationEndpoint, allowPasswordFlow, allowAuthorizationCodeFlow, allowHybridFlow]
    .filter((element) => element !== null)
    .forEach((element) => {
        element.addEventListener("change", refreshEndpoints);
    });
