var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services
    .AddSingleton(configuration)
    .AddOrchardCore()
    .AddMvc();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

app.Run();

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1050: Declare types in namespaces",
    Justification = "As described here: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0.")]
public partial class Program
{
    protected Program()
    {
        // Nothing to do here.
    }
}
