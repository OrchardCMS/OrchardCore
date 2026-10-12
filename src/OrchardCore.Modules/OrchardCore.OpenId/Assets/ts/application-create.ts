import initOpenIdApplicationEditor from "@orchardcore/bloom/components/openid-application-editor";
import { getDatasetBoolean } from "@orchardcore/bloom/helpers/dataset";

const form = document.querySelector<HTMLElement>(".openid-application-editor");

if (form) {
    initOpenIdApplicationEditor({
        settings: {
            configured: getDatasetBoolean(form, "settingsConfigured"),
            allowAuthorizationCodeFlow: getDatasetBoolean(form, "allowAuthorizationCodeFlow"),
            allowClientCredentialsFlow: getDatasetBoolean(form, "allowClientCredentialsFlow"),
            allowImplicitFlow: getDatasetBoolean(form, "allowImplicitFlow"),
            allowHybridFlow: getDatasetBoolean(form, "allowHybridFlow"),
            allowPasswordFlow: getDatasetBoolean(form, "allowPasswordFlow"),
            allowRefreshTokenFlow: getDatasetBoolean(form, "allowRefreshTokenFlow"),
            logoutEndpointConfigured: getDatasetBoolean(form, "logoutEndpointConfigured"),
        },
        clientSecretToggleMode: "collapse",
        clearSecretOnNonConfidential: false,
    });
}
