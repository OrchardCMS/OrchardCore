using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// An implementation of <see cref="ContentFieldIndexHandler&lt;TField&gt;"/> is able to take part in the rendering of
    /// a <see typeparamref="TField"/> instance.
    /// </summary>
    public abstract class ContentFieldIndexHandler<TField> : IContentFieldIndexHandler where TField : ContentField
    {
        Task IContentFieldIndexHandler.BuildIndexAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, BuildIndexContext context, IContentIndexSettings settings)
        {
            if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
               !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return Task.CompletedTask;
            }

            var field = contentPart.Get<TField>(partFieldDefinition.Name);
            if (field != null)
            {
                var keys = new List<string>();
                foreach (var key in context.Keys)
                {
                    keys.Add($"{key}.{partFieldDefinition.Name}");
                }

                if (!keys.Contains($"{typePartDefinition.Name}.{partFieldDefinition.Name}"))
                {
                    keys.Add($"{typePartDefinition.Name}.{partFieldDefinition.Name}");
                }

                var buildFieldIndexContext = new BuildFieldIndexContext(context.DocumentIndex, context.ContentItem, keys, contentPart, typePartDefinition, partFieldDefinition, settings);

                return BuildIndexAsync(field, buildFieldIndexContext);
            }

            return Task.CompletedTask;
        }

        public abstract Task BuildIndexAsync(TField field, BuildFieldIndexContext context);
    }
}
