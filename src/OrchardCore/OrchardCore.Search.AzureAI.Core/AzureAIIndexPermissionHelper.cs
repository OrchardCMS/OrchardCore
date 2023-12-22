using System;
using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public class AzureAIIndexPermissionHelper
{
    public static readonly Permission ManageAzureAIIndexes = new("ManageAzureAIIndexes", "Manage Azure AI Search Indexes");

    private static readonly Permission _indexPermissionTemplate = new("ManageAzureAIIndex_{0}", "Query Azure AI Search '{0}' Index", new[] { ManageAzureAIIndexes });

    private static readonly Dictionary<string, Permission> _permissions = [];

    public static Permission GetPermission(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName, nameof(indexName));

        if (!_permissions.TryGetValue(indexName, out var permission))
        {
            permission = new Permission(
                string.Format(_indexPermissionTemplate.Name, indexName),
                string.Format(_indexPermissionTemplate.Description, indexName),
                _indexPermissionTemplate.ImpliedBy
            );

            _permissions.Add(indexName, permission);
        }

        return permission;
    }
}
