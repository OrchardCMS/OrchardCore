import { getDatasetJson } from "@orchardcore/bloom/helpers/dataset";

const setCollapse = (element: Element | null, action: "show" | "hide") => {
    if (element) {
        bootstrap.Collapse.getOrCreateInstance(element, { toggle: false })[action]();
    }
};

const responseModeElement = document.querySelector<HTMLSelectElement>(".openid-response-mode");
const useCodeFlowElement = document.querySelector<HTMLInputElement>(".openid-use-code-flow");
const useCodeIdTokenFlowElement = document.querySelector<HTMLInputElement>(".openid-use-code-id-token-flow");
const useCodeTokenFlowElement = document.querySelector<HTMLInputElement>(".openid-use-code-token-flow");
const useCodeIdTokenTokenFlowElement = document.querySelector<HTMLInputElement>(
    ".openid-use-code-id-token-token-flow",
);
const storeExternalTokensElement = document.querySelector<HTMLInputElement>(".openid-store-external-tokens");
const clientSecretElement = document.querySelector<HTMLElement>(".openid-client-secret");
const queryOptionElement = document.querySelector<HTMLOptionElement>(".openid-response-mode-query");

const refreshFlows = () => {
    if (!useCodeFlowElement || !responseModeElement || !queryOptionElement) {
        return;
    }

    if (useCodeFlowElement.checked) {
        queryOptionElement.disabled = false;
    } else {
        queryOptionElement.disabled = true;
        responseModeElement.value = responseModeElement.dataset.formPostValue ?? "";
    }

    const showSecret =
        useCodeFlowElement.checked ||
        (useCodeIdTokenFlowElement?.checked ?? false) ||
        (useCodeIdTokenTokenFlowElement?.checked ?? false) ||
        (useCodeTokenFlowElement?.checked ?? false);

    const clientSecretContainer = clientSecretElement?.closest(".collapse") ?? null;

    setCollapse(clientSecretContainer, showSecret ? "show" : "hide");
};

refreshFlows();

document
    .querySelectorAll<HTMLInputElement | HTMLSelectElement>(
        ".openid-response-mode, .openid-use-code-flow, .openid-use-code-id-token-flow, .openid-use-code-token-flow, .openid-use-code-id-token-token-flow, .openid-use-id-token-flow, .openid-use-id-token-token-flow",
    )
    .forEach((element) => {
        element.addEventListener("change", function (this: HTMLInputElement | HTMLSelectElement) {
            const isResponseMode = this === responseModeElement;
            const survivor = isResponseMode ? useCodeFlowElement : this;

            document.querySelectorAll<HTMLInputElement>('input[type="checkbox"]').forEach((checkbox) => {
                if (checkbox !== survivor && checkbox !== storeExternalTokensElement) {
                    checkbox.checked = false;
                }
            });

            refreshFlows();
        });
    });

const parametersEditor = document.querySelector<HTMLElement>(".openid-parameters-editor");

if (parametersEditor) {
    const parameters = getDatasetJson(parametersEditor, "parameters") ?? [];
    const modalBodyElements = document.getElementsByClassName("openid-parameters-editor-modal-body");
    initializeParametersEditor(parametersEditor, parameters, modalBodyElements);
}
