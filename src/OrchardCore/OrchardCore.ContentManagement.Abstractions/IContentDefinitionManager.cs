using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace OrchardCore.ContentManagement.Metadata
{
    /// <summary>
    /// This interface provides each client with
    /// a different copy of <see cref="ContentTypeDefinition"/> to work with in case
    /// multiple clients do modifications.
    /// </summary>
    public interface IContentDefinitionManager
    {
        IEnumerable<ContentTypeDefinition> ListTypeDefinitions();
        IEnumerable<ContentPartDefinition> ListPartDefinitions();

        ContentTypeDefinition GetTypeDefinition(string name);
        ContentPartDefinition GetPartDefinition(string name);
        void DeleteTypeDefinition(string name);
        void DeletePartDefinition(string name);

        void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition);
        void StorePartDefinition(ContentPartDefinition contentPartDefinition);

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
        public static void AlterTypeDefinition(this IContentDefinitionManager manager, string name, Action<ContentTypeDefinitionBuilder> alteration)
        {
            var typeDefinition = manager.GetTypeDefinition(name) ?? new ContentTypeDefinition(name, name.CamelFriendly());
            var builder = new ContentTypeDefinitionBuilder(typeDefinition);
            alteration(builder);
            manager.StoreTypeDefinition(builder.Build());
        }
        public static void AlterPartDefinition(this IContentDefinitionManager manager, string name, Action<ContentPartDefinitionBuilder> alteration)
        {
            var partDefinition = manager.GetPartDefinition(name) ?? new ContentPartDefinition(name);
            var builder = new ContentPartDefinitionBuilder(partDefinition);
            alteration(builder);
            manager.StorePartDefinition(builder.Build());
        }

        public static void MigratePartSettings<TPart, TSettings>(this IContentDefinitionManager manager)
            where TPart : ContentPart where TSettings : class
        {
            var contentTypes = manager.ListTypeDefinitions();

            foreach (var contentType in contentTypes)
            {
                var partDefinition = contentType.Parts.FirstOrDefault(x => x.PartDefinition.Name == typeof(TPart).Name);
                if (partDefinition != null)
                {
                    var existingSettings = partDefinition.Settings.ToObject<TSettings>();

                    // Remove existing properties from JObject
                    var properties = typeof(TSettings).GetProperties();
                    foreach (var property in properties)
                    {
                        partDefinition.Settings.Remove(property.Name);
                    }

                    // Apply existing settings to type definition WithSettings<T>
                    manager.AlterTypeDefinition(contentType.Name, typeBuilder =>
                    {
                        typeBuilder.WithPart(partDefinition.Name, partBuilder =>
                        {
                            partBuilder.WithSettings(existingSettings);
                        });
                    });
                }
            }
        }
    }
}