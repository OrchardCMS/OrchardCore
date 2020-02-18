using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Indexing;
using YesSql;

namespace OrchardCore.Contents.Recipes
{
    /// <summary>
    /// This recipe step creates a set of content items.
    /// </summary>
    public class ContentStep : IRecipeStepHandler
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IContentItemIdGenerator _idGenerator;
        private readonly IContentManagerSession _contentManagerSession;
        private readonly IIndexingTaskManager _indexingTaskManager;

        public ContentStep(
            IContentManager contentManager,
            IContentManagerSession contentManagerSession,
            ISession session,
            IContentItemIdGenerator idGenerator,
            IIndexingTaskManager indexingTaskManager)
        {
            _contentManager = contentManager;
            _contentManagerSession = contentManagerSession;
            _session = session;
            _idGenerator = idGenerator;
            _indexingTaskManager = indexingTaskManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Content", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<ContentStepModel>();

            foreach (JObject token in model.Data)
            {
                var contentItem = token.ToObject<ContentItem>();
                var modifiedUtc = contentItem.ModifiedUtc;
                var publishedUtc = contentItem.PublishedUtc;
                var isPublished = contentItem.Published;
                var isLatest = contentItem.Latest;

                var existing = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);

                if (existing == null)
                {
                    // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
                    contentItem.Id = 0;

                    if (String.IsNullOrEmpty(contentItem.ContentItemVersionId))
                    {
                        contentItem.ContentItemVersionId = _idGenerator.GenerateUniqueId(contentItem);
                        contentItem.Published = true;
                        contentItem.Latest = true;
                    }

                    _session.Save(contentItem);
                    _contentManagerSession.Store(contentItem);
                    await _indexingTaskManager.CreateTaskAsync(contentItem, IndexingTaskTypes.Update);

                    // Resolve previous published or draft items or they will continue to be listed as published or draft.
                    var relatedContentItems = await _session
                        .Query<ContentItem, ContentItemIndex>(x =>
                            x.ContentItemId == contentItem.ContentItemId && (x.Published || x.Latest))
                        .ListAsync();

                    // Alter previous items depending on published and latest values of imported item.
                    foreach (var relatedItem in relatedContentItems)
                    {
                        // CreateAsync calls session.Save so the importing item is now resolved as part of the query.
                        if (String.Equals(relatedItem.ContentItemVersionId, contentItem.ContentItemVersionId))
                        {
                            continue;
                        }

                        if (isPublished)
                        {
                            relatedItem.Published = false;
                        }

                        if (isLatest)
                        {
                            relatedItem.Latest = false;
                        }

                        _session.Save(relatedItem);
                        _contentManagerSession.Store(relatedItem);
                        await _indexingTaskManager.CreateTaskAsync(relatedItem, IndexingTaskTypes.Update);
                    }
                }
            }
        }
    }

    public class ContentStepModel
    {
        public JArray Data { get; set; }
    }
}