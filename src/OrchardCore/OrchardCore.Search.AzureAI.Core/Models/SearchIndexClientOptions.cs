namespace OrchardCore.Search.AzureAI.Models;

public enum AzureAIAuthenticationType
{
    Default = 0,
    ApiKey = 1,
}

public enum AzureAIConfigurationType
{
    UI = 0,
    File = 1,
    UIThenFile = 2,
}
