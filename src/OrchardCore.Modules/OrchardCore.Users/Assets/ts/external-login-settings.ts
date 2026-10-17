import syncMonacoTheme from "@orchardcore/bloom/helpers/monacoTheme";
import waitForMonaco from "@orchardcore/bloom/helpers/monaco";
import type * as Monaco from "monaco-editor";

declare global {
    interface Window {
        resetScript?: () => void;
    }
}

const scriptEditorElement = document.querySelector<HTMLElement>(".external-login-script-editor");
const scriptTextArea = document.querySelector<HTMLTextAreaElement>(".external-login-script-value");
const docEditorElement = document.getElementById("doc_editor");
const useScriptCheckbox = document.querySelector<HTMLInputElement>(".external-login-use-script");

// don't format here.
const suggestion = `/**
* Use the loginProvider and externalClaims properties of the context variable to inspect
* who authenticated the user and with what claims. Check currentRoles property and apply
* your business logic to fill the rolesToAdd and rolesToRemove arrays in order to update
*/
const context = {} as Context;
type Context = {
    readonly user: {
        userName: string;
        userRoles: string[];
        userClaims: { claimType: string; claimValue: string; }[];
        userProperties: { [key: string]: string };
    },
    readonly loginProvider: string,
    rolesToAdd: string[];
    rolesToRemove: string[];
    claimsToUpdate: { claimType: string; claimValue: string; }[];
    claimsToRemove: { claimType: string; claimValue: string; }[];
    propertiesToUpdate: { [key: string]: string };
    externalClaims: readonly [{
        valueType: string;
        type: string;
        value: string;
        subject: string;
        issuer: string;
        originalIssuer: string;
        properties: { [key: string]: string };
    }]
}`;

if (scriptEditorElement && scriptTextArea) {
    let codeEditor: Monaco.editor.IStandaloneCodeEditor;

    window.resetScript = function resetScript() {
        codeEditor.getModel()?.setValue(`/* Uncomment to map AzureAd
switch (context.loginProvider) {
    case "AzureAd":
        context.claimsToUpdate={"displayName":"UserDisplayName"}
        context.propertiesToUpdate={"UserProfile":{"UserProfile":{"DisplayName":"UserDisplayNameValue"}}}
        context.externalClaims.forEach(claim => {
            if (claim.type === "http://schemas.microsoft.com/ws/2008/06/identity/claims/role") {
                switch (claim.value) {
                    case "Writer":
                        context.rolesToAdd.push("Author");
                        break;
                    case "Admin":
                        context.rolesToAdd.push("Administrator");
                        break;
                    default:
                        log("Warning", "Role " + claim.value + " was not handled");
                        break;
                }
            }
        });
        break;
    default:
        log("Warning", "Provider " + context.loginProvider + " was not handled");
        break;
}
*/
`);
    };

    waitForMonaco()
        .then((monaco) => {
            syncMonacoTheme(monaco);

            const editor = monaco.editor.create(scriptEditorElement, {
                automaticLayout: true,
                language: "javascript",
            });
            codeEditor = editor;
            monaco.languages.typescript.javascriptDefaults.addExtraLib(
                `${suggestion}const context = {} as Context`,
                "suggestion.d.ts",
            );

            if (docEditorElement) {
                monaco.editor.create(docEditorElement, {
                    value: suggestion,
                    automaticLayout: true,
                    language: "typescript",
                    readOnly: true,
                    lineNumbers: "off",
                    glyphMargin: false,
                    lineDecorationsWidth: 0,
                    minimap: { enabled: false },
                    scrollbar: { horizontal: "hidden", vertical: "hidden", handleMouseWheel: false },
                    overviewRulerLanes: 0,
                    overviewRulerBorder: false,
                });
            }

            if (!scriptTextArea.value) {
                window.resetScript?.();
            } else {
                editor.getModel()?.setValue(scriptTextArea.value);
            }

            window.addEventListener("submit", () => {
                scriptTextArea.value = editor.getValue();
            });
        })
        .catch((error: unknown) => console.error(error));
}

if (useScriptCheckbox) {
    const editorWrappers = document.querySelectorAll<HTMLElement>("#SyncRolesScript_wrapper, #doc_editor_wrapper");

    const toggleEditorState = () => {
        editorWrappers.forEach((element) => {
            element.style.display = useScriptCheckbox.checked ? "" : "none";
        });
    };

    useScriptCheckbox.addEventListener("change", toggleEditorState);
    toggleEditorState();
}
