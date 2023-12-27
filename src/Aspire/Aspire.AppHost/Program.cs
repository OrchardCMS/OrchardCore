var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Aspire_OrchardCore_Cms_Web>("aspire.orchardcore.cms.web");

builder.Build().Run();
