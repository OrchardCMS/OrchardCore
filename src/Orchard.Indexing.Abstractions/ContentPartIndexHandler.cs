using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.Indexing
{
    /// <summary>
    /// An implementation of <see cref="ContentPartIndexHandler&lt;TPart&gt;"/> is able to take part in the rendering of
    /// a <see cref="TPart"/> instance.
    /// </summary>
    public abstract class ContentPartIndexHandler<TPart> : IContentPartIndexHandler where TPart : ContentPart
    {
        Task IContentPartIndexHandler.BuildIndexAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildIndexContext context, ContentIndexSettings settings)
        {
            var part = contentPart as TPart;

            if (part == null)
            {
                return Task.CompletedTask;
            }

            var buildPartIndexContext = new BuildPartIndexContext(context.DocumentIndex, context.ContentItem, typePartDefinition.Name, typePartDefinition, settings);

            return BuildIndexAsync(part, buildPartIndexContext);
        }

        public abstract Task BuildIndexAsync(TPart part, BuildPartIndexContext context);
    }
}
