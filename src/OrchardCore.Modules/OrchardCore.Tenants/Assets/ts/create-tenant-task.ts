import initLiquidPatternEditor from "@orchardcore/bloom/components/liquid-pattern-editor";

const fieldIds = [
    "TenantNameExpression",
    "DescriptionExpression",
    "RequestUrlPrefixExpression",
    "RequestUrlHostExpression",
    "DatabaseProviderExpression",
    "TablePrefixExpression",
    "SchemaExpression",
    "ConnectionStringExpression",
    "RecipeNameExpression",
    "FeatureProfileExpression",
];

fieldIds.forEach((id) => {
    const textArea = document.getElementById(id) as HTMLTextAreaElement | null;
    if (textArea) {
        initLiquidPatternEditor(textArea);
    }
});
