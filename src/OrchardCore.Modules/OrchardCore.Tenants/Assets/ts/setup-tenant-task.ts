import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

// The DisplayDriver prefixes generated ids, so hardcoded getElementById(...) never
// matches - use attribute selectors against each field-name suffix instead.
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

fieldIds.forEach((id) => {
    const textArea = document.querySelector<HTMLTextAreaElement>(`textarea[id$='${id}']`);
    if (textArea) {
        initLiquidPatternEditor(textArea);
    }
});
