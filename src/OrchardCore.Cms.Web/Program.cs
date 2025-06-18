using System.Text;
using OrchardCore.Logging;

// Register the CodePagesEncodingProvider to support additional encodings. This is done to improve performance by preventing
// several first-chance exceptions thrown by MimeKit when it tries to load encodings that are not available by default in .NET Core.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLogHost();

builder.Services
    .AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

await app.RunAsync();
