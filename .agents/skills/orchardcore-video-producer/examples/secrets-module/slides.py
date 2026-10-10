"""The slides of the Secrets module video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
from PIL import ImageDraw

# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import GREEN, MUTED, TEXT, W, bullets, draw_code, font, note_cards, slide_base

TYPES = {"SecretInputViewModel", "SecretInputUpdateContext", "SecretMigration", "SecretClient", "SecretClientSettings",
         "MyServiceSettings", "MyServiceSettingsViewModel", "MyServiceConstants", "DisplayDriver", "IDisplayResult",
         "Task", "UpdateEditorContext", "MyServiceSecretMigrationDisplayDriver"}

PROBLEMS = [
    ("1", "Scattered credentials", "Each feature keeps its own credentials, in its own settings, protected its own way."),
    ("2", "No inventory", "Nothing lists them: which ones a site uses, which ones expire, which ones to rotate."),
    ("3", "No sharing, no vault", "A value can't be shared between features, or kept in a dedicated vault."),
    ("4", "Hard to move", "Moving a site to another environment means entering every credential again."),
]

SOLUTIONS = [
    ("check", "Encrypted", "Secrets are encrypted with ASP.NET Core Data Protection, or kept in Azure Key Vault."),
    ("check", "Listed in one place", "Search them, filter them, and see which ones expire."),
    ("check", "Referenced by name", "Settings reference a secret by name, wherever it's stored."),
    ("check", "Optional", "Every feature keeps working without the Secrets module."),
]

APPSETTINGS = """
{
  "AzureClients": {
    "SecretClient": {
      "VaultUri": "https://your-vault.vault.azure.net/",
      "Credential": {
        "CredentialSource": "ManagedIdentityCredential"
      }
    }
  },
  "OrchardCore": {
    "Secrets": {
      "AzureKeyVault": {
        "AzureClient": "SecretClient",
        "NamePrefix": "myapp"
      }
    }
  }
}
"""

PROGRAM = """
// Program.cs
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Registers the Secret Client with the "SecretClient" key.
builder.AddKeyedAzureClient<SecretClient, SecretClientSettings>(
    "SecretClient", "AzureClients:SecretClient");

builder.Services
    .AddOrchardCms();
"""

RECIPE = """
{
  "steps": [
    {
      "name": "Secrets",
      "Secrets": [
        { "Name": "SmtpPassword", "Store": "Database" },
        { "Name": "PaymentsApiKey", "Store": "AzureKeyVault" }
      ]
    }
  ]
}
"""

ENVIRONMENT = """
# The values are read from the configuration.
export OrchardCore__Secrets__SmtpPassword=...
export OrchardCore__Secrets__PaymentsApiKey=...
"""

SETTINGS = """
public class MyServiceSettings
{
    // The API key, protected with Data Protection.
    public string ApiKey { get; set; }

    // The name of the secret holding the API key.
    public string ApiKeySecretName { get; set; }
}

public class MyServiceSettingsViewModel
{
    public SecretInputViewModel ApiKey { get; set; } = new();
}
"""

DRIVER = """
// Edit: the editor model is created from the stored values.
model.ApiKey = SecretInputViewModel.Create(settings.ApiKey, settings.ApiKeySecretName);

// Update: validates the input, and protects a new value.
var apiKey = await model.ApiKey.UpdateAsync(new SecretInputUpdateContext(
    _serviceProvider,
    _dataProtectionProvider.CreateProtector(MyServiceConstants.ProtectorName),
    context.Updater.ModelState,
    $"{Prefix}.{nameof(model.ApiKey)}")
{
    ProtectedValue = settings.ApiKey,
    SecretName = settings.ApiKeySecretName,
});

if (apiKey.Succeeded)
{
    settings.ApiKey = apiKey.ProtectedValue;
    settings.ApiKeySecretName = apiKey.SecretName;
}
"""

VIEW = """
@addTagHelper *, OrchardCore.Secrets.Abstractions

<div class="ocat-wrapper">
    <label asp-for="ApiKey" class="ocat-label">@T["API key"]</label>
    <div class="ocat-end">
        <secret-input asp-for="ApiKey" secret-name="MyService.ApiKey" />
    </div>
</div>
"""

RUNTIME = """
// The secret when one is selected, the protected value otherwise.
var apiKey = await _serviceProvider.GetSecretValueAsync(
    settings.ApiKeySecretName,
    settings.ApiKey,
    _dataProtectionProvider.CreateProtector(MyServiceConstants.ProtectorName),
    _logger);
"""

MIGRATION = """
public sealed class MyServiceSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    // Lists the API key on Migrate Secrets while it's kept in the settings, and moves it:
    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        // ...
        await migration.MoveToSecretAsync(_secretManager, protector,
            settings.ApiKey, model.SecretName, S["My service: API key"],
            async secretName =>
            {
                // The settings reference the secret once every selected credential is saved.
                settings.ApiKeySecretName = secretName;
                settings.ApiKey = null;
                await SaveSettingsAsync(settings);
            });
        // ...
    }
}
"""


def slide(step_id, chapter_title, label):
    if step_id.startswith("why-"):
        index = int(step_id[-1])
        if index < 3:
            image = slide_base(chapter_title, label, "Without a central place for credentials")
            bullets(image, PROBLEMS, active={0, 1} if index == 1 else {2, 3})
        else:
            image = slide_base(chapter_title, label, "The Secrets module")
            bullets(image, SOLUTIONS)
        return image

    if step_id == "keyvault-1":
        image = slide_base(chapter_title, label, "Connecting Azure Key Vault")
        draw_code(image, (96, 240, 900, 1030), "appsettings.json", "json", APPSETTINGS, types=TYPES, size=24)
        draw_code(image, (940, 240, W - 96, 700), "Program.cs", "csharp", PROGRAM, types=TYPES, size=22)
        draw = ImageDraw.Draw(image)
        notes = [
            "Register a Secret Client in the host.",
            "Select its key in AzureClient.",
            "NamePrefix and the tenant name keep",
            "each tenant in its own namespace.",
        ]
        y = 760
        for index, note in enumerate(notes):
            if index != 3:
                draw.ellipse((950, y - 7, 964, y + 7), fill=GREEN)
            draw.text((986, y), note, font=font(30), fill=TEXT, anchor="lm")
            y += 64 if index != 2 else 44
        return image

    if step_id == "deploy-2":
        image = slide_base(chapter_title, label, "The Secrets recipe step")
        draw_code(image, (96, 240, 1150, 790), "recipe.json", "json", RECIPE, types=TYPES, size=25)
        draw_code(image, (96, 830, 1150, 1030), "Environment variables", "bash", ENVIRONMENT, types=TYPES, size=25)
        note_cards(image, [
            ("Creates secrets", "when a site is set up."),
            ("Values come from", "the configuration."),
            ("They never appear", "in the recipe."),
        ], (1200, 300, W - 96, 1030))
        return image

    if step_id == "adopt-1":
        image = slide_base(chapter_title, label, "1. Keep the value, add a secret name")
        draw_code(image, (96, 240, W - 96, 1030), "MyServiceSettings.cs", "csharp", SETTINGS, types=TYPES, size=28)
        return image

    if step_id == "adopt-2":
        image = slide_base(chapter_title, label, "2. Edit it in the display driver")
        draw_code(image, (96, 240, W - 96, 1030), "MyServiceSettingsDisplayDriver.cs", "csharp", DRIVER, types=TYPES, size=25)
        return image

    if step_id == "adopt-3":
        image = slide_base(chapter_title, label, "3. Render it, and read it at runtime")
        draw_code(image, (96, 240, W - 96, 600), "MyServiceSettings.Edit.cshtml", "html", VIEW, types=TYPES, size=26)
        draw_code(image, (96, 640, W - 96, 1030), "MyService.cs", "csharp", RUNTIME, types=TYPES, size=26)
        return image

    if step_id == "adopt-4":
        image = slide_base(chapter_title, label, "4. Optionally, list it on Migrate Secrets")
        draw_code(image, (96, 240, W - 96, 1030), "MyServiceSecretMigrationDisplayDriver.cs", "csharp", MIGRATION, types=TYPES, size=24)
        return image

    raise SystemExit(f"No slide for {step_id}")
