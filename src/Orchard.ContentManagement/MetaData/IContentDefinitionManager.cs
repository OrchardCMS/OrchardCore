using System;
using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;
using Orchard.Utility;

namespace Orchard.ContentManagement.MetaData
{
    /// <summary>
    /// This interface is an <see cref="IDependency"/> so that each client gets
    /// a different copy of <see cref="ContentDefinition"/> to work with in case
    /// multiple clients do modifications.
    /// </summary>
    public interface IContentDefinitionManager : IDependency
    {
        IEnumerable<ContentTypeDefinition> ListTypeDefinitions();
        IEnumerable<ContentPartDefinition> ListPartDefinitions();
        IEnumerable<ContentFieldDefinition> ListFieldDefinitions();

        ContentTypeDefinition GetTypeDefinition(string name);
        ContentPartDefinition GetPartDefinition(string name);
        void DeleteTypeDefinition(string name);
        void DeletePartDefinition(string name);

        void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition);
        void StorePartDefinition(ContentPartDefinition contentPartDefinition);
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
    }
}