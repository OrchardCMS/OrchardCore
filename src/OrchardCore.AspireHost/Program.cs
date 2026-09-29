using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using OrchardCore.AspireHost;

var builder = DistributedApplication.CreateBuilder(args);

var clamAv = builder.AddClamAV("antivirus")
    .WithDataVolume("clamavdb");

builder.AddProject<Projects.OrchardCore_Cms_Web>("OrchardCoreCms")
    .WithExternalHttpEndpoints()
    .WithReference(clamAv)
    .WithEnvironment("OrchardCore__Antivirus__ClamAV__Host", clamAv.Resource.PrimaryEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("OrchardCore__Antivirus__ClamAV__Port", clamAv.Resource.PrimaryEndpoint.Property(EndpointProperty.Port))
    .WithEnvironment("OrchardCore__Antivirus__ClamAV__ConnectTimeoutSeconds", "5")
    .WithEnvironment("OrchardCore__Antivirus__ClamAV__TransferTimeoutSeconds", "30");

var app = builder.Build();

await app.RunAsync();
