using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.Data {
    public interface IContentDocumentStore : IDependency {
        void Store<T>(T document) where T : DocumentRecord;
        void Remove<T>(T document) where T : DocumentRecord;
    }
}