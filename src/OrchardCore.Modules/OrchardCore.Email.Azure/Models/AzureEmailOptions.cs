namespace OrchardCore.Email.Azure.Models;

public class AzureEmailOptions
{
    public string DefaultSender { get; set; }

    public string ConnectionString { get; set; }

    public bool PreventUIConnectionChange { get; set; }
}
