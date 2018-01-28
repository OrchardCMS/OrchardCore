using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers
{
    public abstract class ContentPartHandler<TPart> : IContentPartHandler where TPart : ContentPart, new()
    {
        async Task IContentPartHandler.ActivatedAsync(ActivatedContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await ActivatedAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.ActivatingAsync(ActivatingContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await ActivatingAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.InitializingAsync(InitializingContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await InitializingAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.InitializedAsync(InitializingContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await InitializedAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.CreatingAsync(CreateContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await CreatingAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.CreatedAsync(CreateContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await CreatedAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.LoadingAsync(LoadContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await LoadingAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.LoadedAsync(LoadContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await LoadedAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.UpdatingAsync(UpdateContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await UpdatingAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.UpdatedAsync(UpdateContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await UpdatedAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.VersioningAsync(VersionContentContext context, ContentPart existing, ContentPart building)
        {
            if (existing is TPart && building is TPart)
            {
                await VersioningAsync(context, (TPart)existing, (TPart)building);
            }
        }

        async Task IContentPartHandler.VersionedAsync(VersionContentContext context, ContentPart existing, ContentPart building)
        {
            if (existing is TPart && building is TPart)
            {
                await VersionedAsync(context, (TPart)existing, (TPart)building);
            }
        }

        async Task IContentPartHandler.PublishingAsync(PublishContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await PublishingAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.PublishedAsync(PublishContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await PublishedAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.UnpublishingAsync(PublishContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await UnpublishingAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.UnpublishedAsync(PublishContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await UnpublishedAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.RemovingAsync(RemoveContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await RemovingAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.RemovedAsync(RemoveContentContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await RemovedAsync(context, (TPart)part);
            }
        }

        async Task IContentPartHandler.GetContentItemAspectAsync(ContentItemAspectContext context, ContentPart part)
        {
            if (part is TPart)
            {
                await GetContentItemAspectAsync(context, (TPart)part);
            }
        }

        public virtual Task ActivatedAsync(ActivatedContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task ActivatingAsync(ActivatingContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task InitializingAsync(InitializingContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task InitializedAsync(InitializingContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task CreatingAsync(CreateContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task CreatedAsync(CreateContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task LoadingAsync(LoadContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task LoadedAsync(LoadContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task UpdatingAsync(UpdateContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task UpdatedAsync(UpdateContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task VersioningAsync(VersionContentContext context, TPart existing, TPart building) => Task.CompletedTask;
        public virtual Task VersionedAsync(VersionContentContext context, TPart existing, TPart building) => Task.CompletedTask;
        public virtual Task PublishingAsync(PublishContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task PublishedAsync(PublishContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task UnpublishingAsync(PublishContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task UnpublishedAsync(PublishContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task RemovingAsync(RemoveContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task RemovedAsync(RemoveContentContext context, TPart instance) => Task.CompletedTask;
        public virtual Task GetContentItemAspectAsync(ContentItemAspectContext context, TPart part) => Task.CompletedTask;
    }
}