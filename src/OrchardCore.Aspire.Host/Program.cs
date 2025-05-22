using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var elasticsearch = builder.AddElasticsearch("Elasticsearch")
    .WithDataVolume();

var redis = builder.AddRedis("Redis");

builder.AddProject<Projects.OrchardCore_Cms_Web>("OrchardCoreCMS")
    .WithReference(redis)
    .WaitFor(redis)
    .WithHttpsEndpoint(5001)
    .WithEnvironment((options) =>
    {
        // Configure the Redis connection.
        options.EnvironmentVariables.Add("OrchardCore__OrchardCore_Redis__Configuration", "localhost,allowAdmin=true");

        // Configure the Elasticsearch connection.
        options.EnvironmentVariables.Add("OrchardCore__OrchardCore_Elasticsearch__ConnectionType", "SingleNodeConnectionPool");
        options.EnvironmentVariables.Add("OrchardCore__OrchardCore_Elasticsearch__Url", elasticsearch.GetEndpoint("http").Url);
    });

var app = builder.Build();

await app.RunAsync();
