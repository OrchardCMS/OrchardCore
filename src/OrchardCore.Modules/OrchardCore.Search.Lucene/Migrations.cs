using System.Text.Json.Nodes;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Search.Lucene.Model;
using YesSql;

namespace OrchardCore.Search.Lucene;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ShellDescriptor _shellDescriptor;

    public Migrations(
        IContentDefinitionManager contentDefinitionManager,
        ShellDescriptor shellDescriptor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _shellDescriptor = shellDescriptor;
    }

    // New installations don't need to be upgraded, but because there is no initial migration record,
    // 'UpgradeAsync()' is called in a new 'CreateAsync()' but only if the feature was already installed.
    public async Task<int> CreateAsync()
    {
        if (_shellDescriptor.WasFeatureAlreadyInstalled("OrchardCore.Search.Lucene"))
        {
            await UpgradeAsync();
        }

        // Shortcut other migration steps on new content definition schemas.
        return 1;
    }

    // Upgrade an existing installation.
    private async Task UpgradeAsync()
    {
        var contentTypeDefinitions = await _contentDefinitionManager.LoadTypeDefinitionsAsync();

        foreach (var contentTypeDefinition in contentTypeDefinitions)
        {
            foreach (var partDefinition in contentTypeDefinition.Parts)
            {
                await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
                {
                    if (partDefinition.Settings.TryGetPropertyValue("ContentIndexSettings", out var existingPartSettings) &&
                        !partDefinition.Settings.ContainsKey(nameof(LuceneContentIndexSettings)))
                    {
                        var included = existingPartSettings["Included"];
                        var analyzed = existingPartSettings["Analyzed"];

                        if (included is not null)
                        {
                            if (analyzed is not null)
                            {
                                if ((bool)included && !(bool)analyzed)
                                {
                                    existingPartSettings["Keyword"] = true;
                                }
                            }
                            else
                            {
                                if ((bool)included)
                                {
                                    existingPartSettings["Keyword"] = true;
                                }
                            }
                        }

                        var jExistingPartSettings = existingPartSettings.AsObject();

                        // We remove unnecessary properties from old releases.
                        jExistingPartSettings.Remove("Analyzed");
                        jExistingPartSettings.Remove("Tokenized");
                        jExistingPartSettings.Remove("Template");

                        partDefinition.Settings[nameof(LuceneContentIndexSettings)] = jExistingPartSettings.Clone();
                    }

                    partDefinition.Settings.Remove("ContentIndexSettings");
                });
            }
        }

        var partDefinitions = await _contentDefinitionManager.LoadPartDefinitionsAsync();

        foreach (var partDefinition in partDefinitions)
        {
            await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
            {
                if (partDefinition.Settings.TryGetPropertyValue("ContentIndexSettings", out var existingPartSettings) &&
                    !partDefinition.Settings.ContainsKey(nameof(LuceneContentIndexSettings)))
                {
                    var included = existingPartSettings["Included"];
                    var analyzed = existingPartSettings["Analyzed"];

                    if (included != null)
                    {
                        if (analyzed != null)
                        {
                            if ((bool)included && !(bool)analyzed)
                            {
                                existingPartSettings["Keyword"] = true;
                            }
                        }
                        else
                        {
                            if ((bool)included)
                            {
                                existingPartSettings["Keyword"] = true;
                            }
                        }
                    }

                    var jExistingPartSettings = existingPartSettings.AsObject();

                    // We remove unnecessary properties from old releases.
                    jExistingPartSettings.Remove("Analyzed");
                    jExistingPartSettings.Remove("Tokenized");
                    jExistingPartSettings.Remove("Template");
                    partDefinition.Settings[nameof(LuceneContentIndexSettings)] = jExistingPartSettings.Clone();
                }

                partDefinition.Settings.Remove("ContentIndexSettings");

                foreach (var fieldDefinition in partDefinition.Fields)
                {
                    if (fieldDefinition.Settings.TryGetPropertyValue("ContentIndexSettings", out var existingFieldSettings) &&
                        !fieldDefinition.Settings.TryGetPropertyValue(nameof(LuceneContentIndexSettings), out _))
                    {
                        var included = existingFieldSettings["Included"];
                        var analyzed = existingFieldSettings["Analyzed"];

                        if (included != null)
                        {
                            if (analyzed != null)
                            {
                                if ((bool)included && !(bool)analyzed)
                                {
                                    existingFieldSettings["Keyword"] = true;
                                }
                            }
                            else
                            {
                                if ((bool)included)
                                {
                                    existingFieldSettings["Keyword"] = true;
                                }
                            }
                        }

                        var jExistingFieldSettings = existingFieldSettings.AsObject();

                        // We remove unnecessary properties from old releases.
                        jExistingFieldSettings.Remove("Analyzed");
                        jExistingFieldSettings.Remove("Tokenized");
                        jExistingFieldSettings.Remove("Template");

                        fieldDefinition.Settings.Add(nameof(LuceneContentIndexSettings), jExistingFieldSettings.Clone());
                    }

                    fieldDefinition.Settings.Remove("ContentIndexSettings");
                }
            });
        }

        // Defer this until after the subsequent migrations have succeeded as the schema has changed.
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var dbConnectionAccessor = scope.ServiceProvider.GetService<IDbConnectionAccessor>();
            var logger = scope.ServiceProvider.GetService<ILogger<Migrations>>();
            var tablePrefix = session.Store.Configuration.TablePrefix;
            var documentTableName = session.Store.Configuration.TableNameConvention.GetDocumentTable();
            var table = $"{session.Store.Configuration.TablePrefix}{documentTableName}";

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync(session.Store.Configuration.IsolationLevel);
            var dialect = session.Store.Configuration.SqlDialect;

            try
            {
                logger.LogDebug("Updating Lucene indices settings and queries");

                var quotedTableName = dialect.QuoteForTableName(table, session.Store.Configuration.Schema);
                var quotedContentColumnName = dialect.QuoteForColumnName("Content");
                var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

                var updateCmd = $"UPDATE {quotedTableName} SET {quotedContentColumnName} = REPLACE({quotedContentColumnName}, '\"$type\":\"OrchardCore.Lucene.LuceneQuery, OrchardCore.Lucene\"', '\"$type\":\"OrchardCore.Search.Lucene.LuceneQuery, OrchardCore.Search.Lucene\"') WHERE {quotedTypeColumnName}  = 'OrchardCore.Queries.Services.QueriesDocument, OrchardCore.Queries'";

                await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                updateCmd = $"UPDATE {quotedTableName} SET {quotedContentColumnName} = REPLACE({quotedContentColumnName}, '\"$type\":\"OrchardCore.Lucene.Deployment.LuceneIndexDeploymentStep, OrchardCore.Lucene\"', '\"$type\":\"OrchardCore.Search.Lucene.Deployment.LuceneIndexDeploymentStep, OrchardCore.Search.Lucene\"') WHERE {quotedTypeColumnName}  = 'OrchardCore.Deployment.DeploymentPlan, OrchardCore.Deployment.Abstractions'";

                await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                updateCmd = $"UPDATE {quotedTableName} SET {quotedContentColumnName} = REPLACE({quotedContentColumnName}, '\"$type\":\"OrchardCore.Lucene.Deployment.LuceneSettingsDeploymentStep, OrchardCore.Lucene\"', '\"$type\":\"OrchardCore.Search.Lucene.Deployment.LuceneSettingsDeploymentStep, OrchardCore.Search.Lucene\"') WHERE {quotedTypeColumnName}  = 'OrchardCore.Deployment.DeploymentPlan, OrchardCore.Deployment.Abstractions'";

                await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                updateCmd = $"UPDATE {quotedTableName} SET {quotedContentColumnName} = REPLACE({quotedContentColumnName}, '\"$type\":\"OrchardCore.Lucene.Deployment.LuceneIndexResetDeploymentStep, OrchardCore.Lucene\"', '\"$type\":\"OrchardCore.Search.Lucene.Deployment.LuceneIndexResetDeploymentStep, OrchardCore.Search.Lucene\"') WHERE {quotedTypeColumnName}  = 'OrchardCore.Deployment.DeploymentPlan, OrchardCore.Deployment.Abstractions'";

                await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                updateCmd = $"UPDATE {quotedTableName} SET {quotedContentColumnName} = REPLACE({quotedContentColumnName}, '\"$type\":\"OrchardCore.Lucene.Deployment.LuceneIndexRebuildDeploymentStep, OrchardCore.Lucene\"', '\"$type\":\"OrchardCore.Search.Lucene.Deployment.LuceneIndexRebuildDeploymentStep, OrchardCore.Search.Lucene\"') WHERE {quotedTypeColumnName}  = 'OrchardCore.Deployment.DeploymentPlan, OrchardCore.Deployment.Abstractions'";

                await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                updateCmd = $"UPDATE {quotedTableName} SET {quotedTypeColumnName} = 'OrchardCore.Search.Lucene.Model.LuceneIndexSettingsDocument, OrchardCore.Search.Lucene' WHERE {quotedTypeColumnName}  = 'OrchardCore.Lucene.Model.LuceneIndexSettingsDocument, OrchardCore.Lucene'";

                await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                logger.LogError(e, "An error occurred while updating Lucene indices settings and queries");

                throw;
            }
        });
    }
}
