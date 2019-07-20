using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentManagement.Metadata
{
    /// <summary>
    /// This interface provides each client with
    /// a different copy of <see cref="ContentTypeDefinition"/> to work with in case
    /// multiple clients do modifications.
    /// </summary>
    public interface IContentDefinitionManager
    {
        Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync();
        Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync();

        Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name);
        Task<ContentPartDefinition> GetPartDefinitionAsync(string name);
        Task DeleteTypeDefinitionAsync(string name);
        Task DeletePartDefinitionAsync(string name);

        Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition);
        Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition);

        /// <summary>
        /// Returns a serial number representing the list of types and settings for the current tenant.
        /// </summary>
        /// <returns>
        /// An <see cref="int"/> value that changes every time the list of types changes.
        /// The implementation is efficient in order to be called frequently.
        /// </returns>
        Task<int> GetTypesHashAsync();

        IChangeToken ChangeToken { get; }
    }

    public static class ContentDefinitionManagerExtensions
    {
        public static async Task AlterTypeDefinitionAsync(this IContentDefinitionManager manager, string name, Action<ContentTypeDefinitionBuilder> alteration)
        {
            var typeDefinition = await manager.GetTypeDefinitionAsync(name) ?? new ContentTypeDefinition(name, name.CamelFriendly());
            var builder = new ContentTypeDefinitionBuilder(typeDefinition);
            alteration(builder);
            await manager.StoreTypeDefinitionAsync(builder.Build());
        }
        public static async Task AlterPartDefinitionAsync(this IContentDefinitionManager manager, string name, Action<ContentPartDefinitionBuilder> alteration)
        {
            var partDefinition = await manager.GetPartDefinitionAsync(name) ?? new ContentPartDefinition(name);
            var builder = new ContentPartDefinitionBuilder(partDefinition);
            alteration(builder);
            await manager.StorePartDefinitionAsync(builder.Build());
        }
    }
}
