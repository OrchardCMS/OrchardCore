using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// An implementation of <see cref="ContentPartIndexHandler&lt;TPart&gt;"/> is able to take part in the rendering of
    /// a <see typeparamref="TPart"/> instance.
    /// </summary>
    public abstract class ContentPartIndexHandler<TPart> : IContentPartIndexHandler where TPart : ContentPart
    {
        Task IContentPartIndexHandler.BuildIndexAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildIndexContext context, IContentIndexSettings settings)
        {
            var part = contentPart as TPart;

            if (part == null)
            {
                return Task.CompletedTask;
            }

            var keys = new List<string>();
            keys.Add(typePartDefinition.Name);
            foreach (var key in context.Keys)
            {
                keys.Add($"{key}.{typePartDefinition.Name}");
            }

            var buildPartIndexContext = new BuildPartIndexContext(context.DocumentIndex, context.ContentItem, keys, typePartDefinition, settings);

            return BuildIndexAsync(part, buildPartIndexContext);
        }

        public abstract Task BuildIndexAsync(TPart part, BuildPartIndexContext context);
    }
}
