using System.Collections.Generic;
using OrchardVNext.ContentManagement.MetaData.Models;

namespace OrchardVNext.ContentManagement {
    /// <summary>
    /// Content management functionality to deal with Orchard content items and their parts
    /// </summary>
    public interface IContentManager : IDependency {
        IEnumerable<ContentTypeDefinition> GetContentTypeDefinitions();

        /// <summary>
        /// Instantiates a new content item with the specified type
        /// </summary>
        /// <remarks>
        /// The content item is not yet persisted!
        /// </remarks>
        /// <param name="contentType">The name of the content type</param>
        ContentItem New(string contentType);


        /// <summary>
        /// Creates (persists) a new content item
        /// </summary>
        /// <param name="contentItem">The content instance filled with all necessary data</param>
        void Create(ContentItem contentItem);

        /// <summary>
        /// Creates (persists) a new content item with the specified version
        /// </summary>
        /// <param name="contentItem">The content instance filled with all necessary data</param>
        /// <param name="options">The version to create the item with</param>
        void Create(ContentItem contentItem, VersionOptions options);


        ///// <summary>
        ///// Makes a clone of the content item
        ///// </summary>
        ///// <param name="contentItem">The content item to clone</param>
        ///// <returns>Clone of the item</returns>
        //ContentItem Clone(ContentItem contentItem);

        ///// <summary>
        ///// Rolls back the specified content item by creating a new version based on the specified version.
        ///// </summary>
        ///// <param name="contentItem">The content item to roll back.</param>
        ///// <param name="options">The version to roll back to. Either specify the version record id, version number, and IsPublished to publish the new version.</param>
        ///// <returns>Returns the latest version of the content item, which is based on the specified version.</returns>
        //ContentItem Restore(ContentItem contentItem, VersionOptions options);

        /// <summary>
        /// Gets the content item with the specified id
        /// </summary>
        /// <param name="id">Numeric id of the content item</param>
        ContentItem Get(int id);

        /// <summary>
        /// Gets the content item with the specified id and version
        /// </summary>
        /// <param name="id">Numeric id of the content item</param>
        /// <param name="options">The version option</param>
        ContentItem Get(int id, VersionOptions options);

        ///// <summary>
        ///// Gets all versions of the content item specified with its id
        ///// </summary>
        ///// <param name="id">Numeric id of the content item</param>
        //IEnumerable<ContentItem> GetAllVersions(int id);

        //IEnumerable<T> GetMany<T>(IEnumerable<int> ids, VersionOptions options) where T : class, IContent;
        //IEnumerable<T> GetManyByVersionId<T>(IEnumerable<int> versionRecordIds) where T : class, IContent;
        //IEnumerable<ContentItem> GetManyByVersionId(IEnumerable<int> versionRecordIds);

        void Publish(ContentItem contentItem);
        void Unpublish(ContentItem contentItem);
        //void Remove(ContentItem contentItem);

        ///// <summary>
        ///// Permanently deletes the specified content item, including all of its content part records.
        ///// </summary>
        //void Destroy(ContentItem contentItem);
    }
    
    public class VersionOptions {
        /// <summary>
        /// Gets the latest version.
        /// </summary>
        public static VersionOptions Latest { get { return new VersionOptions { IsLatest = true }; } }

        /// <summary>
        /// Gets the latest published version.
        /// </summary>
        public static VersionOptions Published { get { return new VersionOptions { IsPublished = true }; } }

        /// <summary>
        /// Gets the latest draft version.
        /// </summary>
        public static VersionOptions Draft { get { return new VersionOptions { IsDraft = true }; } }

        /// <summary>
        /// Gets the latest version and creates a new version draft based on it.
        /// </summary>
        public static VersionOptions DraftRequired { get { return new VersionOptions { IsDraft = true, IsDraftRequired = true }; } }

        /// <summary>
        /// Gets all versions.
        /// </summary>
        public static VersionOptions AllVersions { get { return new VersionOptions { IsAllVersions = true }; } }

        /// <summary>
        /// Gets a specific version based on its number.
        /// </summary>
        public static VersionOptions Number(int version) { return new VersionOptions { VersionNumber = version }; }

        /// <summary>
        /// Gets a specific version based on the version record identifier.
        /// </summary>
        public static VersionOptions VersionRecord(int id) { return new VersionOptions { VersionRecordId = id }; }

        /// <summary>
        /// Creates a new version based on the specified version number.
        /// </summary>
        public static VersionOptions Restore(int version, bool publish = false) { return new VersionOptions { VersionNumber = version, IsPublished = publish}; }

        public bool IsLatest { get; private set; }
        public bool IsPublished { get; private set; }
        public bool IsDraft { get; private set; }
        public bool IsDraftRequired { get; private set; }
        public bool IsAllVersions { get; private set; }
        public int VersionNumber { get; private set; }
        public int VersionRecordId { get; private set; }
    }
}
