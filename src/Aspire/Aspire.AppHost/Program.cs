var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedisContainer("Redis", 50963);

builder.AddProject<Projects.Aspire_OrchardCore_Cms_Web>("OrchardCore CMS App")
    .WithReference(redis);

builder.AddProject<Projects.Aspire_OrchardCore_Mvc_Web>("OrchardCore MVC App");

await builder.Build().RunAsync();
