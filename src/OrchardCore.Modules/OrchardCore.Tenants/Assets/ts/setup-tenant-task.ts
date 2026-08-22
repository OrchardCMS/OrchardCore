import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

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
    const textArea = document.getElementById(id) as HTMLTextAreaElement | null;
    if (textArea) {
        initLiquidPatternEditor(textArea);
    }
});
