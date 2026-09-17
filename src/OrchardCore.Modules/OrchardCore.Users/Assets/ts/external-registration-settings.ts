const useScriptCheckbox = document.querySelector<HTMLInputElement>(".external-registration-use-script");
const scriptTextArea = document.querySelector<HTMLTextAreaElement>(".external-registration-script-value");
const resetScriptButton = document.getElementById("ResetScriptButton");
const wrapper = document.getElementById("ScriptToGenerateUsername_Wrapper");

const defaultScript = `/* Uncomment to map AzureAd
// Uncomment to output the context variable in the logs
// log("warning", JSON.stringify(context));
switch (context.loginProvider) {
    case "AzureAd":
        context.userName = "azad" + uuid();
        break;
    default:
        log("Warning", "Provider " + context.loginProvider + " was not handled");
        // Uncomment to generate a username as a uuid
        // context.userName = "u" + uuid();
        break;
}
*/
`;

const toggleEditorState = () => {
    wrapper?.classList.toggle("d-none", !useScriptCheckbox?.checked);
    scriptTextArea?.classList.toggle("d-none", !useScriptCheckbox?.checked);
};

useScriptCheckbox?.addEventListener("change", toggleEditorState);
toggleEditorState();

if (scriptTextArea) {
    const editor = CodeMirror.fromTextArea(scriptTextArea, {
        autoRefresh: true,
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        autoCloseTags: true,
        mode: "javascript",
    });

    const resetScript = () => {
        if (editor.getValue() !== "") {
            return;
        }

        editor.setValue(defaultScript);
    };

    resetScriptButton?.addEventListener("click", resetScript);
    resetScript();
}
