using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using OrchardVNext.ContentManagement.Handlers;
using OrchardVNext.ContentManagement.MetaData;
using OrchardVNext.ContentManagement.MetaData.Builders;
using OrchardVNext.ContentManagement.MetaData.Models;
using OrchardVNext.ContentManagement.Records;
using OrchardVNext.Data;
using OrchardVNext.Data.EF;
using OrchardVNext.Data.Providers;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Logging;

namespace OrchardVNext.ContentManagement {
    public class DefaultContentManager : IContentManager {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentHandler> _handlers;
        private readonly IContentStore _contentStore;
        private readonly DataContext _dataContext;
        private readonly ShellSettings _shellSettings;

        private const string Published = "Published";
        private const string Draft = "Draft";

        public DefaultContentManager(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentHandler> handlers,
            IContentStore contentStore,
            DataContext dataContext,
            ShellSettings shellSettings) {
            _contentDefinitionManager = contentDefinitionManager;
            _shellSettings = shellSettings;
            _handlers = handlers;
            _contentStore = contentStore;
            _dataContext = dataContext;
            }
        
        public IEnumerable<IContentHandler> Handlers => _handlers;

        public IEnumerable<ContentTypeDefinition> GetContentTypeDefinitions() {
            return _contentDefinitionManager.ListTypeDefinitions();
        }

        public virtual ContentItem New(string contentType) {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
            if (contentTypeDefinition == null) {
                contentTypeDefinition = new ContentTypeDefinitionBuilder().Named(contentType).Build();
            }

            // create a new kernel for the model instance
            var context = new ActivatingContentContext {
                ContentType = contentTypeDefinition.Name,
                Definition = contentTypeDefinition,
                Builder = new ContentItemBuilder(contentTypeDefinition)
            };

            // invoke handlers to weld aspects onto kernel
            Handlers.Invoke(handler => handler.Activating(context));

            var context2 = new ActivatedContentContext {
                ContentType = contentType,
                ContentItem = context.Builder.Build()
            };

            // back-reference for convenience (e.g. getting metadata when in a view)
            context2.ContentItem.ContentManager = this;

            Handlers.Invoke(handler => handler.Activated(context2));

            var context3 = new InitializingContentContext {
                ContentType = context2.ContentType,
                ContentItem = context2.ContentItem,
            };

            Handlers.Invoke(handler => handler.Initializing(context3));
            Handlers.Invoke(handler => handler.Initialized(context3));

            // composite result is returned
            return context3.ContentItem;
        }

        public virtual ContentItem Get(int id) {
            return Get(id, VersionOptions.Published);
        }

        public virtual ContentItem Get(int id, VersionOptions options) {
            ContentItem contentItem = null;

            //ContentItemVersionRecord versionRecord = null;

            //        // obtain the root records based on version options
            //        if (options.VersionRecordId != 0) {
            //            // short-circuit if item held in session
            //            if (session.RecallVersionRecordId(options.VersionRecordId, out contentItem)) {
            //                return contentItem;
            //            }

            //            versionRecord = _contentItemVersionRepository.Get(options.VersionRecordId);
            //        }
            //        else if (options.VersionNumber != 0) {
            //            // short-circuit if item held in session
            //            if (session.RecallVersionNumber(id, options.VersionRecordId, out contentItem)) {
            //                return contentItem;
            //            }

            //            versionRecord = _contentItemVersionRepository.Get(x => x.ContentItemRecord.Id == id && x.Number == options.VersionNumber);
            //        }
            //        else if (session.RecallContentRecordId(id, out contentItem)) {
            //            // try to reload a previously loaded published content item

            //            if (options.IsPublished) {
            //                return contentItem;
            //            }

            //            versionRecord = contentItem.VersionRecord;
            //        }
            //        else {
            //            // do a query to load the records in case Get is called directly
            //            var contentItemVersionRecords = GetManyImplementation(hints,
            //                (contentItemCriteria, contentItemVersionCriteria) => {
            //                    contentItemCriteria.Add(Restrictions.Eq("Id", id));
            //                    if (options.IsPublished) {
            //                        contentItemVersionCriteria.Add(Restrictions.Eq("Published", true));
            //                    }
            //                    else if (options.IsLatest) {
            //                        contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
            //                    }
            //                    else if (options.IsDraft && !options.IsDraftRequired) {
            //                        contentItemVersionCriteria.Add(
            //                            Restrictions.And(Restrictions.Eq("Published", false),
            //                                            Restrictions.Eq("Latest", true)));
            //                    }
            //                    else if (options.IsDraft || options.IsDraftRequired) {
            //                        contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
            //                    }

            //                    contentItemVersionCriteria.SetFetchMode("ContentItemRecord", FetchMode.Eager);
            //                    contentItemVersionCriteria.SetFetchMode("ContentItemRecord.ContentType", FetchMode.Eager);
            //                    //contentItemVersionCriteria.SetMaxResults(1);
            //                });


            //            if (options.VersionNumber != 0) {
            //                versionRecord = contentItemVersionRecords.FirstOrDefault(
            //                    x => x.Number == options.VersionNumber) ??
            //                       _contentItemVersionRepository.Get(
            //                           x => x.ContentItemRecord.Id == id && x.Number == options.VersionNumber);
            //            }
            //            else {
            //                versionRecord = contentItemVersionRecords.LastOrDefault();
            //            }
            //        }

            //        // no record means content item is not in db
            //        if (versionRecord == null) {
            //            // check in memory
            //            var record = _contentItemRepository.Get(id);
            //            if (record == null) {
            //                return null;
            //            }

            //            versionRecord = GetVersionRecord(options, record);

            //            if (versionRecord == null) {
            //                return null;
            //            }
            //        }

            //        // return item if obtained earlier in session
            //        if (session.RecallVersionRecordId(versionRecord.Id, out contentItem)) {
            //            if (options.IsDraftRequired && versionRecord.Published) {
            //                return BuildNewVersion(contentItem);
            //            }
            //            return contentItem;
            //        }

            //        // allocate instance and set record property
            //        contentItem = New(versionRecord.ContentItemRecord.ContentType.Name);
            //        contentItem.VersionRecord = versionRecord;

            //        // store in session prior to loading to avoid some problems with simple circular dependencies
            //        session.Store(contentItem);

            //        // create a context with a new instance to load            
            //        var context = new LoadContentContext(contentItem);

            //        // invoke handlers to acquire state, or at least establish lazy loading callbacks
            //        Handlers.Invoke(handler => handler.Loading(context), Logger);
            //        Handlers.Invoke(handler => handler.Loaded(context), Logger);

            //        // when draft is required and latest is published a new version is appended 
            //        if (options.IsDraftRequired && versionRecord.Published) {
            //            contentItem = BuildNewVersion(context.ContentItem);
            //        }

            return contentItem;
        }

        //    private ContentItemVersionRecord GetVersionRecord(VersionOptions options, ContentItemRecord itemRecord) {
        //        if (options.IsPublished) {
        //            return itemRecord.Versions.FirstOrDefault(
        //                x => x.Published) ??
        //                   _contentItemVersionRepository.Get(
        //                       x => x.ContentItemRecord == itemRecord && x.Published);
        //        }
        //        if (options.IsLatest || options.IsDraftRequired) {
        //            return itemRecord.Versions.FirstOrDefault(
        //                x => x.Latest) ??
        //                   _contentItemVersionRepository.Get(
        //                       x => x.ContentItemRecord == itemRecord && x.Latest);
        //        }
        //        if (options.IsDraft) {
        //            return itemRecord.Versions.FirstOrDefault(
        //                x => x.Latest && !x.Published) ??
        //                   _contentItemVersionRepository.Get(
        //                       x => x.ContentItemRecord == itemRecord && x.Latest && !x.Published);
        //        }
        //        if (options.VersionNumber != 0) {
        //            return itemRecord.Versions.FirstOrDefault(
        //                x => x.Number == options.VersionNumber) ??
        //                   _contentItemVersionRepository.Get(
        //                       x => x.ContentItemRecord == itemRecord && x.Number == options.VersionNumber);
        //        }
        //        return null;
        //    }

        //    public virtual IEnumerable<ContentItem> GetAllVersions(int id) {
        //        return _contentItemVersionRepository
        //            .Fetch(x => x.ContentItemRecord.Id == id)
        //            .OrderBy(x => x.Number)
        //            .Select(x => Get(x.Id, VersionOptions.VersionRecord(x.Id)));
        //    }

        //    public IEnumerable<T> GetMany<T>(IEnumerable<int> ids, VersionOptions options) where T : class, IContent {
        //        var contentItemVersionRecords = GetManyImplementation(hints, (contentItemCriteria, contentItemVersionCriteria) => {
        //            contentItemCriteria.Add(Restrictions.In("Id", ids.ToArray()));
        //            if (options.IsPublished) {
        //                contentItemVersionCriteria.Add(Restrictions.Eq("Published", true));
        //            }
        //            else if (options.IsLatest) {
        //                contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
        //            }
        //            else if (options.IsDraft && !options.IsDraftRequired) {
        //                contentItemVersionCriteria.Add(
        //                    Restrictions.And(Restrictions.Eq("Published", false),
        //                                    Restrictions.Eq("Latest", true)));
        //            }
        //            else if (options.IsDraft || options.IsDraftRequired) {
        //                contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
        //            }
        //        });

        //        var itemsById = contentItemVersionRecords
        //            .Select(r => Get(r.ContentItemRecord.Id, options.IsDraftRequired ? options : VersionOptions.VersionRecord(r.Id)))
        //            .GroupBy(ci => ci.Id)
        //            .ToDictionary(g => g.Key);

        //        return ids.SelectMany(id => {
        //                IGrouping<int, ContentItem> values;
        //                return itemsById.TryGetValue(id, out values) ? values : Enumerable.Empty<ContentItem>();
        //            }).AsPart<T>().ToArray();
        //    }


        //    public IEnumerable<ContentItem> GetManyByVersionId(IEnumerable<int> versionRecordIds) {
        //        var contentItemVersionRecords = GetManyImplementation((contentItemCriteria, contentItemVersionCriteria) =>
        //            contentItemVersionCriteria.Add(Restrictions.In("Id", versionRecordIds.ToArray())));

        //        var itemsById = contentItemVersionRecords
        //            .Select(r => Get(r.ContentItemRecord.Id, VersionOptions.VersionRecord(r.Id)))
        //            .GroupBy(ci => ci.VersionRecord.Id)
        //            .ToDictionary(g => g.Key);

        //        return versionRecordIds.SelectMany(id => {
        //            IGrouping<int, ContentItem> values;
        //            return itemsById.TryGetValue(id, out values) ? values : Enumerable.Empty<ContentItem>();
        //        }).ToArray();
        //    }

        //    public IEnumerable<T> GetManyByVersionId<T>(IEnumerable<int> versionRecordIds) where T : class, IContent {
        //        return GetManyByVersionId(versionRecordIds).AsPart<T>();
        //    }

        //    private IEnumerable<ContentItemVersionRecord> GetManyImplementation(Action<ICriteria, ICriteria> predicate) {
        //        var session = _sessionLocator.Value.For(typeof (ContentItemRecord));
        //        var contentItemVersionCriteria = session.CreateCriteria(typeof (ContentItemVersionRecord));
        //        var contentItemCriteria = contentItemVersionCriteria.CreateCriteria("ContentItemRecord");
        //        predicate(contentItemCriteria, contentItemVersionCriteria);

        //        var contentItemMetadata = session.SessionFactory.GetClassMetadata(typeof (ContentItemRecord));
        //        var contentItemVersionMetadata = session.SessionFactory.GetClassMetadata(typeof (ContentItemVersionRecord));

        //        if (hints != QueryHints.Empty) {
        //            // break apart and group hints by their first segment
        //            var hintDictionary = hints.Records
        //                .Select(hint => new { Hint = hint, Segments = hint.Split('.') })
        //                .GroupBy(item => item.Segments.FirstOrDefault())
        //                .ToDictionary(grouping => grouping.Key, StringComparer.InvariantCultureIgnoreCase);

        //            // locate hints that match properties in the ContentItemVersionRecord
        //            foreach (var hit in contentItemVersionMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key])) {
        //                contentItemVersionCriteria.SetFetchMode(hit.Hint, FetchMode.Eager);
        //                hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemVersionCriteria, ExtendCriteria);
        //            }

        //            // locate hints that match properties in the ContentItemRecord
        //            foreach (var hit in contentItemMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key])) {
        //                contentItemVersionCriteria.SetFetchMode("ContentItemRecord." + hit.Hint, FetchMode.Eager);
        //                hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemCriteria, ExtendCriteria);
        //            }

        //            if (hintDictionary.SelectMany(x => x.Value).Any(x => x.Segments.Count() > 1))
        //                contentItemVersionCriteria.SetResultTransformer(new DistinctRootEntityResultTransformer());
        //        }

        //        contentItemCriteria.SetCacheable(true);

        //        return contentItemVersionCriteria.List<ContentItemVersionRecord>();
        //    }

        //    private static ICriteria ExtendCriteria(ICriteria criteria, string segment) {
        //        return criteria.GetCriteriaByPath(segment) ?? criteria.CreateCriteria(segment, JoinType.LeftOuterJoin);
        //    }

        public virtual void Publish(ContentItem contentItem) {
            if (contentItem.VersionRecord.Published) {
                return;
            }
            // create a context for the item and it's previous published record
            var previous = contentItem.Record.Versions.SingleOrDefault(x => x.Published);
            var context = new PublishContentContext(contentItem, previous);

            // invoke handlers to acquire state, or at least establish lazy loading callbacks
            Handlers.Invoke(handler => handler.Publishing(context));

            if (context.Cancel) {
                return;
            }

            if (previous != null) {
                previous.Published = false;
            }
            contentItem.VersionRecord.Published = true;

            Handlers.Invoke(handler => handler.Published(context));
        }

        //public virtual void Unpublish(ContentItem contentItem) {
        //    ContentItem publishedItem;
        //    if (contentItem.VersionRecord.Published) {
        //        // the version passed in is the published one
        //        publishedItem = contentItem;
        //    }
        //    else {
        //        // try to locate the published version of this item
        //        publishedItem = Get(contentItem.Id, VersionOptions.Published);
        //    }

        //    if (publishedItem == null) {
        //        // no published version exists. no work to perform.
        //        return;
        //    }

        //    // create a context for the item. the publishing version is null in this case
        //    // and the previous version is the one active prior to unpublishing. handlers
        //    // should take this null check into account
        //    var context = new PublishContentContext(contentItem, publishedItem.VersionRecord) {
        //        PublishingItemVersionRecord = null
        //    };

        //    Handlers.Invoke(handler => handler.Unpublishing(context));

        //    publishedItem.VersionRecord.Published = false;

        //    Handlers.Invoke(handler => handler.Unpublished(context));
        //}

        //    public virtual void Remove(ContentItem contentItem) {
        //        var activeVersions = _contentItemVersionRepository.Fetch(x => x.ContentItemRecord == contentItem.Record && (x.Published || x.Latest));
        //        var context = new RemoveContentContext(contentItem);

        //        Handlers.Invoke(handler => handler.Removing(context), Logger);

        //        foreach (var version in activeVersions) {
        //            if (version.Published) {
        //                version.Published = false;
        //            }
        //            if (version.Latest) {
        //                version.Latest = false;
        //            }
        //        }

        //        Handlers.Invoke(handler => handler.Removed(context), Logger);
        //    }

        //    public virtual void Destroy(ContentItem contentItem) {
        //        var session = _sessionLocator.Value.For(typeof(ContentItemVersionRecord));
        //        var context = new DestroyContentContext(contentItem);

        //        // Give storage filters a chance to delete content part records.
        //        Handlers.Invoke(handler => handler.Destroying(context), Logger);

        //        // Delete content item version and content item records.
        //        session
        //            .CreateQuery("delete from Orchard.ContentManagement.Records.ContentItemVersionRecord civ where civ.ContentItemRecord.Id = (:id)")
        //            .SetParameter("id", contentItem.Id)
        //            .ExecuteUpdate();

        //        // Delete the content item record itself.
        //        session
        //            .CreateQuery("delete from Orchard.ContentManagement.Records.ContentItemRecord ci where ci.Id = (:id)")
        //            .SetParameter("id", contentItem.Id)
        //            .ExecuteUpdate();

        //        Handlers.Invoke(handler => handler.Destroyed(context), Logger);
        //    }

        //    protected virtual ContentItem BuildNewVersion(ContentItem existingContentItem) {
        //        var contentItemRecord = existingContentItem.Record;

        //        // locate the existing and the current latest versions, allocate building version
        //        var existingItemVersionRecord = existingContentItem.VersionRecord;
        //        var buildingItemVersionRecord = new ContentItemVersionRecord {
        //            ContentItemRecord = contentItemRecord,
        //            Latest = true,
        //            Published = false,
        //            Data = existingItemVersionRecord.Data,
        //        };


        //        var latestVersion = contentItemRecord.Versions.SingleOrDefault(x => x.Latest);

        //        if (latestVersion != null) {
        //            latestVersion.Latest = false;
        //            buildingItemVersionRecord.Number = latestVersion.Number + 1;
        //        }
        //        else {
        //            buildingItemVersionRecord.Number = contentItemRecord.Versions.Max(x => x.Number) + 1;
        //        }

        //        contentItemRecord.Versions.Add(buildingItemVersionRecord);
        //        _contentItemVersionRepository.Create(buildingItemVersionRecord);

        //        var buildingContentItem = New(existingContentItem.ContentType);
        //        buildingContentItem.VersionRecord = buildingItemVersionRecord;

        //        var context = new VersionContentContext {
        //            Id = existingContentItem.Id,
        //            ContentType = existingContentItem.ContentType,
        //            ContentItemRecord = contentItemRecord,
        //            ExistingContentItem = existingContentItem,
        //            BuildingContentItem = buildingContentItem,
        //            ExistingItemVersionRecord = existingItemVersionRecord,
        //            BuildingItemVersionRecord = buildingItemVersionRecord,
        //        };
        //        Handlers.Invoke(handler => handler.Versioning(context), Logger);
        //        Handlers.Invoke(handler => handler.Versioned(context), Logger);

        //        return context.BuildingContentItem;
        //    }

        public virtual void Create(ContentItem contentItem) {
            Create(contentItem, VersionOptions.Published);
        }

        public virtual void Create(ContentItem contentItem, VersionOptions options) {
            if (contentItem.VersionRecord == null) {
                // produce root record to determine the model id
                contentItem.VersionRecord = new ContentItemVersionRecord {
                    ContentItemRecord = new ContentItemRecord(),
                    Number = 1,
                    Latest = true,
                    Published = true
                };
            }

            // add to the collection manually for the created case
            contentItem.VersionRecord.ContentItemRecord.Versions.Add(contentItem.VersionRecord);

            // version may be specified
            if (options.VersionNumber != 0) {
                contentItem.VersionRecord.Number = options.VersionNumber;
            }

            // draft flag on create is required for explicitly-published content items
            if (options.IsDraft) {
                contentItem.VersionRecord.Published = false;
            }

            _contentStore.Store(contentItem);

            // build a context with the initialized instance to create
            var context = new CreateContentContext(contentItem);

            // invoke handlers to add information to persistent stores
            Handlers.Invoke(handler => handler.Creating(context));

            // deferring the assignment of ContentType as loading a Record might force NHibernate to AutoFlush 
            // the ContentPart, and needs the ContentItemRecord to be created before (created in previous statement)
            contentItem.VersionRecord.ContentItemRecord.ContentType = AcquireContentTypeRecord(contentItem.ContentType);

            Handlers.Invoke(handler => handler.Created(context));


            if (options.IsPublished) {
                var publishContext = new PublishContentContext(contentItem, null);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.Invoke(handler => handler.Publishing(publishContext));

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.Invoke(handler => handler.Published(publishContext));
            }
        }

        //    public virtual ContentItem Clone(ContentItem contentItem) {
        //        // Mostly taken from: http://orchard.codeplex.com/discussions/396664
        //        var importContentSession = new ImportContentSession(this);

        //        var element = Export(contentItem);

        //        // If a handler prevents this element from being exported, it can't be cloned
        //        if (element == null) {
        //            throw new InvalidOperationException("The content item couldn't be cloned because a handler prevented it from being exported.");
        //        }

        //        var elementId = element.Attribute("Id");
        //        var copyId = elementId.Value + "-copy";
        //        elementId.SetValue(copyId);
        //        var status = element.Attribute("Status");
        //        if (status != null) status.SetValue("Draft"); // So the copy is always a draft.

        //        importContentSession.Set(copyId, element.Name.LocalName);

        //        Import(element, importContentSession);

        //        return importContentSession.Get(copyId, element.Name.LocalName);
        //    }

        //    public virtual ContentItem Restore(ContentItem contentItem, VersionOptions options) {
        //        // Invoke handlers.
        //        Handlers.Invoke(handler => handler.Restoring(new RestoreContentContext(contentItem, options)), Logger);

        //        // Get the latest version.
        //        var versions = contentItem.Record.Versions.OrderBy(x => x.Number).ToArray();
        //        var latestVersionRecord = versions.SingleOrDefault(x => x.Latest) ?? versions.Last();

        //        // Get the specified version.
        //        var specifiedVersionContentItem =
        //            contentItem.VersionRecord.Number == options.VersionNumber || contentItem.VersionRecord.Id == options.VersionRecordId 
        //            ? contentItem 
        //            : Get(contentItem.Id, options);

        //        // Create a new version record based on the specified version record.
        //        var rolledBackContentItem = BuildNewVersion(specifiedVersionContentItem);
        //        rolledBackContentItem.VersionRecord.Published = options.IsPublished;

        //        // Invoke handlers.
        //        Handlers.Invoke(handler => handler.Restored(new RestoreContentContext(rolledBackContentItem, options)), Logger);

        //        if (options.IsPublished) {
        //            // Unpublish the latest version.
        //            latestVersionRecord.Published = false;

        //            var publishContext = new PublishContentContext(rolledBackContentItem, previousItemVersionRecord: latestVersionRecord);

        //            Handlers.Invoke(handler => handler.Publishing(publishContext), Logger);
        //            Handlers.Invoke(handler => handler.Published(publishContext), Logger);
        //        }

        //        return rolledBackContentItem;
        //    }

        //    /// <summary>
        //    /// Lookup for a content item based on a <see cref="ContentIdentity"/>. If multiple 
        //    /// resolvers can give a result, the one with the highest priority is used. As soon as 
        //    /// only one content item is returned from resolvers, it is returned as the result.
        //    /// </summary>
        //    /// <param name="contentIdentity">The <see cref="ContentIdentity"/> instance to lookup</param>
        //    /// <returns>The <see cref="ContentItem"/> instance represented by the identity object.</returns>
        //    public ContentItem ResolveIdentity(ContentIdentity contentIdentity) {
        //        var resolvers = _identityResolverSelectors.Value
        //            .Select(x => x.GetResolver(contentIdentity))
        //            .Where(x => x != null)
        //            .OrderByDescending(x => x.Priority);

        //        if (!resolvers.Any())
        //            return null;

        //        IEnumerable<ContentItem> contentItems = null;
        //        foreach (var resolver in resolvers) {
        //            var resolved = resolver.Resolve(contentIdentity).ToArray();

        //            // first pass
        //            if (contentItems == null) {
        //                contentItems = resolved;
        //            }
        //            else { // subsquent passes means we need to intersect 
        //                contentItems = contentItems.Intersect(resolved).ToArray();
        //            }

        //            if (contentItems.Count() == 1) {
        //                return contentItems.First();
        //            }
        //        }

        //        return contentItems.FirstOrDefault();
        //    }

        //    public ContentItemMetadata GetItemMetadata(IContent content) {
        //        var context = new GetContentItemMetadataContext {
        //            ContentItem = content.ContentItem,
        //            Metadata = new ContentItemMetadata()
        //        };

        //        Handlers.Invoke(handler => handler.GetContentItemMetadata(context), Logger);

        //        return context.Metadata;
        //    }

        //    public IEnumerable<GroupInfo> GetEditorGroupInfos(IContent content) {
        //        var metadata = GetItemMetadata(content);
        //        return metadata.EditorGroupInfo
        //            .GroupBy(groupInfo => groupInfo.Id)
        //            .Select(grouping => grouping.OrderBy(groupInfo => groupInfo.Position, new FlatPositionComparer()).FirstOrDefault());
        //    }

        //    public IEnumerable<GroupInfo> GetDisplayGroupInfos(IContent content) {
        //        var metadata = GetItemMetadata(content);
        //        return metadata.DisplayGroupInfo
        //            .GroupBy(groupInfo => groupInfo.Id)
        //            .Select(grouping => grouping.OrderBy(groupInfo => groupInfo.Position, new FlatPositionComparer()).FirstOrDefault());
        //    }

        //    public GroupInfo GetEditorGroupInfo(IContent content, string groupInfoId) {
        //        return GetEditorGroupInfos(content).FirstOrDefault(gi => string.Equals(gi.Id, groupInfoId, StringComparison.OrdinalIgnoreCase));
        //    }

        //    public GroupInfo GetDisplayGroupInfo(IContent content, string groupInfoId) {
        //        return GetDisplayGroupInfos(content).FirstOrDefault(gi => string.Equals(gi.Id, groupInfoId, StringComparison.OrdinalIgnoreCase));
        //    }

        //    public dynamic BuildDisplay(IContent content, string displayType = "", string groupId = "") {
        //        return _contentDisplay.Value.BuildDisplay(content, displayType, groupId);
        //    }

        //    public dynamic BuildEditor(IContent content, string groupId = "") {
        //        return _contentDisplay.Value.BuildEditor(content, groupId);
        //    }

        //    public dynamic UpdateEditor(IContent content, IUpdateModel updater, string groupId = "") {
        //        var context = new UpdateContentContext(content.ContentItem);

        //        Handlers.Invoke(handler => handler.Updating(context), Logger);

        //        var result = _contentDisplay.Value.UpdateEditor(content, updater, groupId);

        //        Handlers.Invoke(handler => handler.Updated(context), Logger);

        //        return result;
        //    }

        //    public void Clear() {
        //    }

        //    public IContentQuery<ContentItem> Query() {
        //        var query = _context.Resolve<IContentQuery>(TypedParameter.From<IContentManager>(this));
        //        return query.ForPart<ContentItem>();
        //    }

        //    public IHqlQuery HqlQuery() {
        //        return new DefaultHqlQuery(this, _sessionLocator.Value.For(typeof(ContentItemVersionRecord)), _sqlStatementProviders.Value, _shellSettings);
        //    }

        //    // Insert or Update imported data into the content manager.
        //    // Call content item handlers.
        //    public void Import(XElement element, ImportContentSession importContentSession) {
        //        var elementId = element.Attribute("Id");
        //        if (elementId == null) {
        //            return;
        //        }

        //        var identity = elementId.Value;

        //        if (String.IsNullOrWhiteSpace(identity)) {
        //            return;
        //        }

        //        var status = element.Attribute("Status");

        //        var item = importContentSession.Get(identity, VersionOptions.Latest, XmlConvert.DecodeName(element.Name.LocalName));
        //        if (item == null) {
        //            item = New(XmlConvert.DecodeName(element.Name.LocalName));
        //            if (status != null && status.Value == "Draft") {
        //                Create(item, VersionOptions.Draft);
        //            }
        //            else {
        //                Create(item);
        //            }
        //        }

        //        // create a version record if import handlers need it
        //        if(item.VersionRecord == null) {
        //            item.VersionRecord = new ContentItemVersionRecord {
        //                ContentItemRecord = new ContentItemRecord {
        //                    ContentType = AcquireContentTypeRecord(item.ContentType)
        //                },
        //                Number = 1,
        //                Latest = true,
        //                Published = true
        //            };                
        //        }

        //        var context = new ImportContentContext(item, element, importContentSession);
        //        foreach (var contentHandler in Handlers) {
        //            contentHandler.Importing(context);
        //        }

        //        foreach (var contentHandler in Handlers) {
        //            contentHandler.Imported(context);
        //        }

        //        var savedItem = Get(item.Id, VersionOptions.Latest);

        //        // the item has been pre-created in the first pass of the import, create it in db
        //        if(savedItem == null) {
        //            if (status != null && status.Value == "Draft") {
        //                Create(item, VersionOptions.Draft);
        //            }
        //            else {
        //                Create(item);
        //            }
        //        }

        //        if (status == null || status.Value == Published) {
        //            Publish(item);
        //        }
        //    }

        //    public XElement Export(ContentItem contentItem) {
        //        var context = new ExportContentContext(contentItem, new XElement(XmlConvert.EncodeLocalName(contentItem.ContentType)));

        //        foreach (var contentHandler in Handlers) {
        //            contentHandler.Exporting(context);
        //        }

        //        foreach (var contentHandler in Handlers) {
        //            contentHandler.Exported(context);
        //        }

        //        if (context.Exclude) {
        //            return null;
        //        }

        //        context.Data.SetAttributeValue("Id", GetItemMetadata(contentItem).Identity.ToString());
        //        if (contentItem.IsPublished()) {
        //            context.Data.SetAttributeValue("Status", Published);
        //        }
        //        else {
        //            context.Data.SetAttributeValue("Status", Draft);
        //        }

        //        return context.Data;
        //    }

        private ContentTypeRecord AcquireContentTypeRecord(string contentType) {
            var context = _dataContext.Set<ContentTypeRecord>();
            var contentTypeRecord = context.FirstOrDefault(x => x.Name == contentType);

            if (contentTypeRecord == null) {
                //TEMP: this is not safe... ContentItem types could be created concurrently?
                contentTypeRecord = new ContentTypeRecord { Name = contentType };
                _dataContext.Add(contentTypeRecord);
            }

            var contentTypeId = contentTypeRecord.Id;

            // There is a case when a content type record is created locally but the transaction is actually
            // cancelled. In this case we are caching an Id which is none existent, or might represent another
            // content type. Thus we need to ensure that the cache is valid, or invalidate it and retrieve it 
            // another time.

            var result = context.FirstOrDefault(x => x.Id == contentTypeId);

            if (result != null && result.Name.Equals(contentType, StringComparison.OrdinalIgnoreCase)) {
                return result;
            }

            // invalidate the cache entry and load it again
            return AcquireContentTypeRecord(contentType);
        }

        //    public void Index(ContentItem contentItem, IDocumentIndex documentIndex) {
        //        var indexContentContext = new IndexContentContext(contentItem, documentIndex);

        //        // dispatch to handlers to retrieve index information
        //        Handlers.Invoke(handler => handler.Indexing(indexContentContext), Logger);

        //        Handlers.Invoke(handler => handler.Indexed(indexContentContext), Logger);
        //    }
        //}

        //internal class CallSiteCollection : ConcurrentDictionary<string, CallSite<Func<CallSite, object, object>>> {
        //    private readonly Func<string, CallSite<Func<CallSite, object, object>>> _valueFactory;

        //    public CallSiteCollection(Func<string, CallSite<Func<CallSite, object, object>>> callSiteFactory) {
        //        _valueFactory = callSiteFactory;
        //    }

        //    public CallSiteCollection(Func<string, CallSiteBinder> callSiteBinderFactory) {
        //        _valueFactory = key => CallSite<Func<CallSite, object, object>>.Create(callSiteBinderFactory(key));
        //    }

        //    public object Invoke(object callee, string key) {
        //        var callSite = GetOrAdd(key, _valueFactory);
        //        return callSite.Target(callSite, callee);
        //    }
    }
}
