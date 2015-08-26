namespace Orchard.ContentManagement.Handlers {
    public interface IContentActivatingFilter : IContentFilter {
        void Activating(ActivatingContentContext context);
    }
}
