//using System;
//using OrchardVNext.ContentManagement.Records;
//using OrchardVNext.Data;

//namespace OrchardVNext.ContentManagement.Handlers {
//    public static class StorageFilter {
//        public static StorageFilter<TRecord> For<TRecord>(IRepository<TRecord> repository) where TRecord : ContentPartRecord, new() {
//            if (typeof(TRecord).IsSubclassOf(typeof(ContentPartVersionRecord))) {
//                var filterType = typeof(StorageVersionFilter<>).MakeGenericType(typeof(TRecord));
//                return (StorageFilter<TRecord>)Activator.CreateInstance(filterType, repository);
//            }
//            return new StorageFilter<TRecord>(repository);
//        }
//    }

//    public class StorageFilter<TRecord> : StorageFilterBase<ContentPart<TRecord>> where TRecord : ContentPartRecord, new() {
//        protected readonly IRepository<TRecord> _repository;

//        public StorageFilter(IRepository<TRecord> repository) {
//            if (this.GetType() == typeof(StorageFilter<TRecord>) && typeof(TRecord).IsSubclassOf(typeof(ContentPartVersionRecord))) {
//                throw new ArgumentException(
//                    string.Format("Use {0} (or {1}.For<TRecord>()) for versionable record types", typeof(StorageVersionFilter<>).Name, typeof(StorageFilter).Name),
//                    "repository");
//            }

//            _repository = repository;
//        }

//        protected virtual TRecord GetRecordCore(ContentItemVersionRecord versionRecord) {
//            return _repository.Get(versionRecord.ContentItemRecord.Id);
//        }

//        protected virtual TRecord CreateRecordCore(ContentItemVersionRecord versionRecord, TRecord record) {
//            record.ContentItemRecord = versionRecord.ContentItemRecord;
//            _repository.Create(record);
//            return record;
//        }

//        protected override void Activated(ActivatedContentContext context, ContentPart<TRecord> instance) {
//            if (instance.Record != null) {
//                throw new InvalidOperationException(string.Format(
//                    "Having more than one storage filter for a given part ({0}) is invalid.",
//                    typeof(ContentPart<TRecord>).FullName));
//            }
//            instance.Record = new TRecord();
//        }

//        protected override void Creating(CreateContentContext context, ContentPart<TRecord> instance) {
//            CreateRecordCore(context.ContentItemVersionRecord, instance.Record);
//        }

//        protected override void Loading(LoadContentContext context, ContentPart<TRecord> instance) {
//            var versionRecord = context.ContentItemVersionRecord;
//            instance._record.Loader(prior => GetRecordCore(versionRecord) ?? CreateRecordCore(versionRecord, prior));
//        }

//        protected override void Versioning(VersionContentContext context, ContentPart<TRecord> existing, ContentPart<TRecord> building) {
//            building.Record = existing.Record;
//        }

//        protected override void Destroying(DestroyContentContext context, ContentPart<TRecord> instance) {
//            _repository.Delete(instance.Record);
//        }
//    }
//}
