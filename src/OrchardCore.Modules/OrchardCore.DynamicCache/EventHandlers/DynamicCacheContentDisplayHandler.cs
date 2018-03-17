using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.EventHandlers
{
    // todo: Requires OrchardCore.ContentManagement.Display. Should this live elsewhere?
    public class DynamicCacheContentDisplayHandler : IContentDisplayHandler
    {
        private readonly ICacheScopeManager _cacheScopeManager;

        public DynamicCacheContentDisplayHandler(ICacheScopeManager cacheScopeManager)
        {
            _cacheScopeManager = cacheScopeManager;
        }

        public Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context)
        {
            _cacheScopeManager.AddTag($"contentitemid:{contentItem.ContentItemId}");

            return Task.CompletedTask;
        }

        public Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context)
        {
            return Task.CompletedTask;
        }

        public Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context)
        {
            return Task.CompletedTask;
        }
    }
}