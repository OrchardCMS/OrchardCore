using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.Indexing
{
    /// <summary>
    /// An implementation of <see cref="ContentFieldIndexHandler&lt;TField&gt;"/> is able to take part in the rendering of
    /// a <see cref="TField"/> instance.
    /// </summary>
    public abstract class ContentFieldIndexHandler<TField> : IContentFieldIndexHandler where TField : ContentField
    {
        Task IContentFieldIndexHandler.BuildIndexAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, BuildIndexContext context, ContentIndexSettings settings)
        {
            if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
               !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return Task.CompletedTask;
            }

            var field = contentPart.Get<TField>(partFieldDefinition.Name);
            if (field != null)
            {
                var buildFieldIndexContext = new BuildFieldIndexContext(context.DocumentIndex, context.ContentItem, $"{typePartDefinition.Name}.{partFieldDefinition.Name}", contentPart, typePartDefinition, partFieldDefinition, settings);

                return BuildIndexAsync(field, buildFieldIndexContext);
            }

            return Task.CompletedTask;
        }

        public abstract Task BuildIndexAsync(TField field, BuildFieldIndexContext context);
    }
}
