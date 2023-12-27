var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Aspire_OrchardCore_Cms_Web>("OrchardCore CMS App");

builder.AddProject<Projects.Aspire_OrchardCore_Mvc_Web>("OrchardCore MVC App");

builder.Build().Run();
