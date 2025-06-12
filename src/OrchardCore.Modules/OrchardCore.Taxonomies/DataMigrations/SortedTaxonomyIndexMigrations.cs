using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Indexing;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Taxonomies.DataMigrations;

internal sealed class SortedTaxonomyIndexMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public SortedTaxonomyIndexMigrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("TaxonomySortPart", part => part
            .Attachable()
            .WithDisplayName("Taxonomy Sort")
            .WithDescription("Provides a way to specify sort order when listing taxonomy items.")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<SortedTaxonomyIndex>(table => table
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<int>("Order", o => o.NotNull().WithDefault(0))
        );

        await SchemaBuilder.AlterIndexTableAsync<SortedTaxonomyIndex>(table => table
            .CreateIndex("IDX_SortedTaxonomyIndex_DocumentId",
                "DocumentId",
                "ContentItemId",
                "Order")
        );

        ShellScope.AddDeferredTask(async scope =>
        {
            var definitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();

            var contentTypes = new HashSet<string>();

            var contentTypeDefinitions = await definitionManager.ListTypeDefinitionsAsync();

            foreach (var contentTypeDefinition in contentTypeDefinitions)
            {
                var defaultPart = contentTypeDefinition.Parts.FirstOrDefault(part => part.PartDefinition.Name == contentTypeDefinition.Name);

                if (defaultPart is null || !defaultPart.PartDefinition.Fields.Any(part => part.FieldDefinition.Name == nameof(TaxonomyField)))
                {
                    continue;
                }

                contentTypes.Add(contentTypeDefinition.Name);
            }

            if (contentTypes.Count == 0)
            {
                return;
            }

            var store = scope.ServiceProvider.GetRequiredService<IStore>();

            var dialect = store.Configuration.SqlDialect;

            var contentItemIndexName = $"{store.Configuration.TablePrefix}{nameof(ContentItemIndex)}";

            var sortedIndexName = $"{store.Configuration.TablePrefix}{nameof(SortedTaxonomyIndex)}";

            var documentIdColumnName = dialect.QuoteForColumnName("DocumentId");
            var contentItemIdColumnName = dialect.QuoteForColumnName("ContentItemId");
            var orderColumnName = dialect.QuoteForColumnName("Order");
            var contentTypeColumnName = dialect.QuoteForColumnName("ContentType");
            var publishedColumnName = dialect.QuoteForColumnName("Published");

            var query = $"""
            insert into {sortedIndexName} ({documentIdColumnName}, {contentItemIdColumnName}, {orderColumnName})
            select {documentIdColumnName}, {contentItemIdColumnName}, 0 from {contentItemIndexName} where {publishedColumnName} = 1 and {contentTypeColumnName} {dialect.InOperator("@contentTypes")}
            """;

            var dbConnectionAccessor = scope.ServiceProvider.GetRequiredService<IDbConnectionAccessor>();

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();

            await connection.ExecuteAsync(query, new
            {
                contentTypes = contentTypes.ToArray(),
            });

            await connection.CloseAsync();
        });

        return 1;
    }
}
