//using System.Linq;
//using OrchardVNext.ContentManagement.Records;
//using OrchardVNext.Data;

//namespace OrchardVNext.ContentManagement.Handlers {
//    public class StorageVersionFilter<TRecord> : StorageFilter<TRecord> where TRecord : ContentPartVersionRecord, new() {
//        public StorageVersionFilter(IRepository<TRecord> repository)
//            : base(repository) {
//        }

//        protected override TRecord GetRecordCore(ContentItemVersionRecord versionRecord) {
//            return _repository.Get(versionRecord.Id);
//        }

//        protected override TRecord CreateRecordCore(ContentItemVersionRecord versionRecord, TRecord record) {
//            record.ContentItemRecord = versionRecord.ContentItemRecord;
//            record.ContentItemVersionRecord = versionRecord;
//            _repository.Create(record);
//            return record;
//        }

//        protected override void Versioning(VersionContentContext context, ContentPart<TRecord> existing, ContentPart<TRecord> building) {
//            // move known ORM values over
//            _repository.Copy(existing.Record, building.Record);

//            // only the up-reference to the particular version differs at this point
//            building.Record.ContentItemVersionRecord = context.BuildingItemVersionRecord;

//            // push the new instance into the transaction and session
//            _repository.Create(building.Record);
//        }

//        protected override void Destroying(DestroyContentContext context, ContentPart<TRecord> instance) {
//            // Get all content item version records.
//            var allVersions = context.ContentItem.Record.Versions.ToArray();

//            // For each version record, delete its part record (ID of versioned part records is the same as the ID of a version record).
//            foreach (var versionRecord in allVersions) {
//                var partRecord = _repository.Get(versionRecord.Id);

//                if (partRecord != null)
//                    _repository.Delete(partRecord);
//            }
//        }
//    }

//}