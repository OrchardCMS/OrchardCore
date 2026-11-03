import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

// The DisplayDriver prefixes generated ids, so hardcoded getElementById(...) never
// matches - use attribute selectors against each field-name suffix instead, within the
// [data-task-editor="setup-tenant"] wrapper. 'observeAndInit' also covers editors injected after the page
// loaded (the workflow designer panel).
const fieldIds = [
    "TenantNameExpression",
    "SiteNameExpression",
    "AdminUsernameExpression",
    "AdminEmailExpression",
    "AdminPasswordExpression",
    "DatabaseProviderExpression",
    "DatabaseTablePrefixExpression",
    "DatabaseSchemaExpression",
    "DatabaseConnectionStringExpression",
    "RecipeNameExpression",
];

observeAndInit('[data-task-editor="setup-tenant"]', (editor) => {
    fieldIds.forEach((id) => {
        const textArea = editor.querySelector<HTMLTextAreaElement>(`textarea[id$='${id}']`);

        if (textArea) {
            bindCodeMirrorToTextArea(initLiquidPatternEditor(textArea), textArea);
        }
    });
});
