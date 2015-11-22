using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Content management functionality to deal with Orchard content items and their parts
    /// </summary>
    public interface IContentManager : IDependency
    {
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

        /// <summary>
        /// Gets the content item with the specified id
        /// </summary>
        /// <param name="id">Numeric id of the content item</param>
        Task<ContentItem> Get(int id);

        /// <summary>
        /// Gets the content item with the specified id and version
        /// </summary>
        /// <param name="id">Numeric id of the content item</param>
        /// <param name="options">The version option</param>
        Task<ContentItem> Get(int id, VersionOptions options);

        Task Publish(ContentItem contentItem);
        Task Unpublish(ContentItem contentItem);
    }

    public class VersionOptions
    {
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
        public static VersionOptions Restore(int version, bool publish = false) { return new VersionOptions { VersionNumber = version, IsPublished = publish }; }

        public bool IsLatest { get; private set; }
        public bool IsPublished { get; private set; }
        public bool IsDraft { get; private set; }
        public bool IsDraftRequired { get; private set; }
        public bool IsAllVersions { get; private set; }
        public int VersionNumber { get; private set; }
        public int VersionRecordId { get; private set; }
    }
}