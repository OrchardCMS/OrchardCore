using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.FileStorage;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using OrchardCore.Media;
using OrchardCore.MetaWeblog;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Users.Services;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;
using YesSql;

namespace OrchardCore.Lists.RemotePublishing
{
    [RequireFeatures("OrchardCore.RemotePublishing")]
    public class MetaWeblogHandler : IXmlRpcHandler
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IMembershipService _membershipService;
        private readonly IEnumerable<IMetaWeblogDriver> _metaWeblogDrivers;
        private readonly ISession _session;
        protected readonly IStringLocalizer S;

        public MetaWeblogHandler(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IMembershipService membershipService,
            ISession session,
            IContentDefinitionManager contentDefinitionManager,
            IMediaFileStore mediaFileStore,
            IEnumerable<IMetaWeblogDriver> metaWeblogDrivers,
            IStringLocalizer<MetaWeblogHandler> localizer)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _authorizationService = authorizationService;
            _metaWeblogDrivers = metaWeblogDrivers;
            _session = session;
            _mediaFileStore = mediaFileStore;
            _membershipService = membershipService;
            S = localizer;
        }

        public void SetCapabilities(XElement options)
        {
            const string manifestUri = "http://schemas.microsoft.com/wlw/manifest/weblog";

            foreach (var driver in _metaWeblogDrivers)
            {
                driver.SetCapabilities((name, value) => { options.SetElementValue(XName.Get(name, manifestUri), value); });
            }
        }

        public async Task ProcessAsync(XmlRpcContext context)
        {
            if (context.RpcMethodCall.MethodName == "blogger.getUsersBlogs")
            {
                var result = await MetaWeblogGetUserBlogsAsync(context,
                    Convert.ToString(context.RpcMethodCall.Params[1].Value),
                    Convert.ToString(context.RpcMethodCall.Params[2].Value));

                context.RpcMethodResponse = new XRpcMethodResponse().Add(result);
            }

            if (context.RpcMethodCall.MethodName == "metaWeblog.getRecentPosts")
            {
                var result = await MetaWeblogGetRecentPosts(
                    context,
                    Convert.ToString(context.RpcMethodCall.Params[0].Value),
                    Convert.ToString(context.RpcMethodCall.Params[1].Value),
                    Convert.ToString(context.RpcMethodCall.Params[2].Value),
                    Convert.ToInt32(context.RpcMethodCall.Params[3].Value),
                    context.Drivers);

                context.RpcMethodResponse = new XRpcMethodResponse().Add(result);
            }

            if (context.RpcMethodCall.MethodName == "metaWeblog.newPost")
            {
                var result = await MetaWeblogNewPostAsync(
                    Convert.ToString(context.RpcMethodCall.Params[0].Value),
                    Convert.ToString(context.RpcMethodCall.Params[1].Value),
                    Convert.ToString(context.RpcMethodCall.Params[2].Value),
                    (XRpcStruct)context.RpcMethodCall.Params[3].Value,
                    Convert.ToBoolean(context.RpcMethodCall.Params[4].Value),
                    context.Drivers);

                context.RpcMethodResponse = new XRpcMethodResponse().Add(result);
            }

            if (context.RpcMethodCall.MethodName == "metaWeblog.getPost")
            {
                var result = await MetaWeblogGetPostAsync(
                    context,
                    Convert.ToString(context.RpcMethodCall.Params[0].Value),
                    Convert.ToString(context.RpcMethodCall.Params[1].Value),
                    Convert.ToString(context.RpcMethodCall.Params[2].Value),
                    context.Drivers);
                context.RpcMethodResponse = new XRpcMethodResponse().Add(result);
            }

            if (context.RpcMethodCall.MethodName == "metaWeblog.editPost")
            {
                var result = await MetaWeblogEditPostAsync(
                    Convert.ToString(context.RpcMethodCall.Params[0].Value),
                    Convert.ToString(context.RpcMethodCall.Params[1].Value),
                    Convert.ToString(context.RpcMethodCall.Params[2].Value),
                    (XRpcStruct)context.RpcMethodCall.Params[3].Value,
                    Convert.ToBoolean(context.RpcMethodCall.Params[4].Value),
                    context.Drivers);
                context.RpcMethodResponse = new XRpcMethodResponse().Add(result);
            }

            if (context.RpcMethodCall.MethodName == "blogger.deletePost")
            {
                var result = await MetaWeblogDeletePostAsync(
                    Convert.ToString(context.RpcMethodCall.Params[1].Value),
                    Convert.ToString(context.RpcMethodCall.Params[2].Value),
                    Convert.ToString(context.RpcMethodCall.Params[3].Value),
                    context.Drivers);
                context.RpcMethodResponse = new XRpcMethodResponse().Add(result);
            }

            if (context.RpcMethodCall.MethodName == "metaWeblog.newMediaObject")
            {
                var result = await MetaWeblogNewMediaObjectAsync(
                    Convert.ToString(context.RpcMethodCall.Params[1].Value),
                    Convert.ToString(context.RpcMethodCall.Params[2].Value),
                    (XRpcStruct)context.RpcMethodCall.Params[3].Value);
                context.RpcMethodResponse = new XRpcMethodResponse().Add(result);
            }
        }

        private async Task<XRpcStruct> MetaWeblogNewMediaObjectAsync(string userName, string password, XRpcStruct file)
        {
            _ = await ValidateUserAsync(userName, password);

            var name = file.Optional<string>("name");
            var bits = file.Optional<byte[]>("bits");

            var directoryName = Path.GetDirectoryName(name);
            var filePath = _mediaFileStore.Combine(directoryName, Path.GetFileName(name));
            Stream stream = null;
            try
            {
                stream = new MemoryStream(bits);
                filePath = await _mediaFileStore.CreateFileFromStreamAsync(filePath, stream);
            }
            finally
            {
                stream?.Dispose();
            }

            var publicUrl = _mediaFileStore.MapPathToPublicUrl(filePath);

            // Some clients require all optional attributes to be declared Wordpress responds in this way as well.
            return new XRpcStruct()
                .Set("file", publicUrl)
                .Set("url", publicUrl)
                .Set("type", file.Optional<string>("type"));
        }

        private async Task<XRpcArray> MetaWeblogGetUserBlogsAsync(XmlRpcContext context, string userName, string password)
        {
            var user = await ValidateUserAsync(userName, password);

            var array = new XRpcArray();

            // Look for all types using ListPart.
            foreach (var type in _contentDefinitionManager.ListTypeDefinitions())
            {
                if (!type.Parts.Any(x => x.Name == nameof(ListPart)))
                {
                    continue;
                }

                foreach (var list in await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == type.Name).ListAsync())
                {
                    // User needs to at least have permission to edit its own blog posts to access the service.
                    if (await _authorizationService.AuthorizeAsync(user, CommonPermissions.EditContent, list))
                    {
                        var metadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(list);
                        var displayRouteValues = metadata.DisplayRouteValues;

                        array.Add(new XRpcStruct()
                                      .Set("url", context.Url.Action(displayRouteValues["action"].ToString(), displayRouteValues["controller"].ToString(), displayRouteValues, context.HttpContext.Request.Scheme))
                                      .Set("blogid", list.ContentItemId)
                                      .Set("blogName", list.DisplayText));
                    }
                }
            }

            return array;
        }

        private async Task<XRpcArray> MetaWeblogGetRecentPosts(
            XmlRpcContext context,
            string contentItemId,
            string userName,
            string password,
            int numberOfPosts,
            IEnumerable<IXmlRpcDriver> drivers)
        {
            var user = await ValidateUserAsync(userName, password);

            // User needs to at least have permission to edit its own blog posts to access the service.
            await CheckAccessAsync(CommonPermissions.EditContent, user, null);

            var list = (await _contentManager.GetAsync(contentItemId))
                ?? throw new InvalidOperationException("Could not find content item " + contentItemId);

            var array = new XRpcArray();

            var contentItems = await _session.Query<ContentItem>()
                .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                .With<ContentItemIndex>(x => x.Latest)
                .OrderByDescending(x => x.CreatedUtc)
                .Take(numberOfPosts)
                .ListAsync();

            foreach (var contentItem in contentItems)
            {
                var postStruct = await CreateBlogStructAsync(context, contentItem);

                foreach (var driver in drivers)
                {
                    driver.Process(postStruct);
                }

                array.Add(postStruct);
            }

            return array;
        }

        private async Task<string> MetaWeblogNewPostAsync(
            string contentItemId,
            string userName,
            string password,
            XRpcStruct content,
            bool publish,
            IEnumerable<IXmlRpcDriver> drivers)
        {
            var user = await ValidateUserAsync(userName, password);

            // User needs permission to edit or publish its own blog posts.
            await CheckAccessAsync(publish ? CommonPermissions.PublishContent : CommonPermissions.EditContent, user, null);

            var list = (await _contentManager.GetAsync(contentItemId))
                ?? throw new InvalidOperationException("Could not find content item " + contentItemId);

            var postType = GetContainedContentTypes(list).FirstOrDefault();
            var contentItem = await _contentManager.NewAsync(postType.Name);

            contentItem.Owner = user.FindFirstValue(ClaimTypes.NameIdentifier);
            contentItem.Alter<ContainedPart>(x => x.ListContentItemId = list.ContentItemId);

            foreach (var driver in _metaWeblogDrivers)
            {
                driver.EditPost(content, contentItem);
            }

            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            // Try to get the UTC time zone by default.
            var publishedUtc = content.Optional<DateTime?>("date_created_gmt");
            if (publishedUtc == null)
            {
                // Take the local one
                publishedUtc = content.Optional<DateTime?>("dateCreated");
            }
            else
            {
                // Ensure it's read as a UTC time.
                publishedUtc = new DateTime(publishedUtc.Value.Ticks, DateTimeKind.Utc);
            }

            if (publish && (publishedUtc == null || publishedUtc <= DateTime.UtcNow))
            {
                await _contentManager.PublishAsync(contentItem);
            }
            else
            {
                await _contentManager.SaveDraftAsync(contentItem);
            }

            if (publishedUtc != null)
            {
                contentItem.CreatedUtc = publishedUtc;
            }

            foreach (var driver in drivers)
            {
                driver.Process(contentItem.ContentItemId);
            }

            return contentItem.ContentItemId;
        }

        private async Task<XRpcStruct> MetaWeblogGetPostAsync(
            XmlRpcContext context,
            string contentItemId,
            string userName,
            string password,
            IEnumerable<IXmlRpcDriver> drivers)
        {
            var user = await ValidateUserAsync(userName, password);

            var contentItem = (await _contentManager.GetAsync(contentItemId, VersionOptions.Latest))
                ?? throw new InvalidOperationException("Could not find content item " + contentItemId);

            await CheckAccessAsync(CommonPermissions.EditContent, user, contentItem);

            var postStruct = await CreateBlogStructAsync(context, contentItem);

            foreach (var driver in _metaWeblogDrivers)
            {
                driver.BuildPost(postStruct, context, contentItem);
            }

            foreach (var driver in drivers)
            {
                driver.Process(postStruct);
            }

            return postStruct;
        }

        private async Task<bool> MetaWeblogEditPostAsync(
            string contentItemId,
            string userName,
            string password,
            XRpcStruct content,
            bool publish,
            IEnumerable<IXmlRpcDriver> drivers)
        {
            var user = await ValidateUserAsync(userName, password);

            var contentItem = (await _contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired))
                ?? throw new Exception(S["The specified Blog Post doesn't exist anymore. Please create a new Blog Post."]);

            await CheckAccessAsync(publish ? CommonPermissions.PublishContent : CommonPermissions.EditContent, user, contentItem);

            foreach (var driver in _metaWeblogDrivers)
            {
                driver.EditPost(content, contentItem);
            }

            // Try to get the UTC time zone by default.
            var publishedUtc = content.Optional<DateTime?>("date_created_gmt");
            if (publishedUtc == null)
            {
                // take the local one
                publishedUtc = content.Optional<DateTime?>("dateCreated");
            }
            else
            {
                // Ensure it's read as a UTC time.
                publishedUtc = new DateTime(publishedUtc.Value.Ticks, DateTimeKind.Utc);
            }

            if (publish && (publishedUtc == null || publishedUtc <= DateTime.UtcNow))
            {
                await _contentManager.PublishAsync(contentItem);
            }
            else
            {
                await _contentManager.SaveDraftAsync(contentItem);
            }

            if (publishedUtc != null)
            {
                contentItem.CreatedUtc = publishedUtc;
            }

            foreach (var driver in drivers)
            {
                driver.Process(contentItem.Id);
            }

            return true;
        }

        private async Task<bool> MetaWeblogDeletePostAsync(
            string contentItemId,
            string userName,
            string password,
            IEnumerable<IXmlRpcDriver> drivers)
        {
            var user = await ValidateUserAsync(userName, password);

            var contentItem = (await _contentManager.GetAsync(contentItemId, VersionOptions.Latest))
                ?? throw new InvalidOperationException("Could not find content item " + contentItemId);

            if (!await _authorizationService.AuthorizeAsync(user, CommonPermissions.DeleteContent, contentItem))
            {
                throw new InvalidOperationException(S["Not authorized to delete this content"].Value);
            }

            foreach (var driver in drivers)
            {
                driver.Process(contentItem.ContentItemId);
            }

            await _contentManager.RemoveAsync(contentItem);
            return true;
        }

        private async Task<ClaimsPrincipal> ValidateUserAsync(string userName, string password)
        {
            if (!await _membershipService.CheckPasswordAsync(userName, password))
            {
                throw new InvalidOperationException(S["The username or e-mail or password provided is incorrect."].Value);
            }

            var storeUser = await _membershipService.GetUserAsync(userName);

            if (storeUser == null)
            {
                return null;
            }

            return await _membershipService.CreateClaimsPrincipal(storeUser);
        }

        private async Task<XRpcStruct> CreateBlogStructAsync(XmlRpcContext context, ContentItem contentItem)
        {
            var metadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);

            var url = context.Url.Action(
                metadata.DisplayRouteValues["action"].ToString(),
                metadata.DisplayRouteValues["controller"].ToString(),
                metadata.DisplayRouteValues,
                context.HttpContext.Request.Scheme);

            if (contentItem.HasDraft())
            {
                url = context.Url.Action("Preview", "Item", new { area = "OrchardCore.Contents", contentItemId = contentItem.ContentItemId });
            }

            var blogStruct = new XRpcStruct()
                .Set("postid", contentItem.ContentItemId)
                .Set("link", url)
                .Set("permaLink", url);

            if (contentItem.PublishedUtc != null)
            {
                blogStruct.Set("dateCreated", contentItem.PublishedUtc);
                blogStruct.Set("date_created_gmt", contentItem.PublishedUtc);
            }

            foreach (var driver in _metaWeblogDrivers)
            {
                driver.BuildPost(blogStruct, context, contentItem);
            }

            return blogStruct;
        }

        private async Task CheckAccessAsync(Permission permission, ClaimsPrincipal user, ContentItem contentItem)
        {
            if (!await _authorizationService.AuthorizeAsync(user, permission, contentItem))
            {
                throw new InvalidOperationException(S["Not authorized to delete this content"].Value);
            }
        }

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(ContentItem contentItem)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "ListPart"));
            var settings = contentTypePartDefinition.GetSettings<ListPartSettings>();
            var contentTypes = settings.ContainedContentTypes ?? Enumerable.Empty<string>();
            return contentTypes.Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType));
        }
    }
}
