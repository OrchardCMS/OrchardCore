using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Taxonomies.Controllers;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Taxonomies;

public class AdminControllerTests
{
    private const string TaxonomyId = "taxonomy-id";
    private const string TermId = "term-id";
    private const string TextFromHandler = "Text from handler";

    [Fact]
    public async Task CreatePost_ValidModel_RaisesCreateEventsAroundEditorUpdateAndSavesTheirChanges()
    {
        var taxonomy = CreateTaxonomy();
        var events = new List<string>();
        var controller = CreateController(taxonomy, events);

        await controller.CreatePost("Tag", TaxonomyId, null);

        Assert.Equal(["Creating", "Editor", "Created"], events);
        Assert.Equal(TextFromHandler, Assert.Single(Reload(taxonomy).GetOrCreate<TaxonomyPart>().Terms).DisplayText);
    }

    [Fact]
    public async Task CreatePost_InvalidModel_DoesNotRaiseCreatedOrAddTerm()
    {
        var taxonomy = CreateTaxonomy();
        var events = new List<string>();
        var controller = CreateController(taxonomy, events, isModelValid: false);

        await controller.CreatePost("Tag", TaxonomyId, null);

        Assert.Equal(["Creating", "Editor"], events);
        Assert.Empty(Reload(taxonomy).GetOrCreate<TaxonomyPart>().Terms);
    }

    [Fact]
    public async Task EditPost_ValidModel_RaisesUpdateEventsAroundEditorUpdateAndSavesTheirChanges()
    {
        var taxonomy = CreateTaxonomy(new ContentItem { ContentItemId = TermId, ContentType = "Tag", DisplayText = "Old" });
        var events = new List<string>();
        var controller = CreateController(taxonomy, events);

        await controller.EditPost(TaxonomyId, TermId);

        Assert.Equal([$"Updating {TermId}", "Editor", $"Updated {TermId}"], events);
        Assert.Equal(TextFromHandler, Assert.Single(Reload(taxonomy).GetOrCreate<TaxonomyPart>().Terms).DisplayText);
    }

    [Fact]
    public async Task EditPost_InvalidModel_DoesNotRaiseUpdatedOrChangeTerm()
    {
        var taxonomy = CreateTaxonomy(new ContentItem { ContentItemId = TermId, ContentType = "Tag", DisplayText = "Old" });
        var events = new List<string>();
        var controller = CreateController(taxonomy, events, isModelValid: false);

        await controller.EditPost(TaxonomyId, TermId);

        Assert.Equal([$"Updating {TermId}", "Editor"], events);
        Assert.Equal("Old", Assert.Single(Reload(taxonomy).GetOrCreate<TaxonomyPart>().Terms).DisplayText);
    }

    private static ContentItem Reload(ContentItem contentItem)
        => JObject.FromObject(contentItem).ToObject<ContentItem>();

    private static ContentItem CreateTaxonomy(params ContentItem[] terms)
    {
        var taxonomy = new ContentItem { ContentItemId = TaxonomyId, ContentType = "Taxonomy" };
        taxonomy.Alter<TaxonomyPart>(part => part.Terms.AddRange(terms));

        return taxonomy;
    }

    private static AdminController CreateController(ContentItem taxonomy, List<string> events, bool isModelValid = true)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var definitionManager = new Mock<IContentDefinitionManager>();
        definitionManager
            .Setup(manager => manager.GetTypeDefinitionAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => new ContentTypeDefinitionBuilder().WithName(name).Build());

        var contentManager = new Mock<IContentManager>();
        contentManager
            .Setup(manager => manager.GetAsync(TaxonomyId, It.IsAny<VersionOptions>()))
            .ReturnsAsync(taxonomy);
        contentManager
            .Setup(manager => manager.NewAsync(It.IsAny<string>()))
            .ReturnsAsync((string contentType) => new ContentItem { ContentItemId = "new-id", ContentType = contentType });

        var displayManager = new Mock<IContentItemDisplayManager>();
        AdminController controller = null;
        displayManager
            .Setup(manager => manager.UpdateEditorAsync(
                It.IsAny<ContentItem>(),
                It.IsAny<IUpdateModel>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback(() =>
            {
                events.Add("Editor");

                if (!isModelValid)
                {
                    controller.ModelState.AddModelError("Field", "Invalid");
                }
            })
            .ReturnsAsync(new Shape());

        controller = new AdminController(
            Mock.Of<YesSql.ISession>(),
            contentManager.Object,
            authorizationService.Object,
            displayManager.Object,
            definitionManager.Object,
            Mock.Of<INotifier>(),
            Mock.Of<IHtmlLocalizer<AdminController>>(),
            Mock.Of<IUpdateModelAccessor>(),
            [new RecordingContentHandler(events)],
            NullLogger<AdminController>.Instance)
        {
            ControllerContext = new()
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity("Test")) },
            },
        };

        return controller;
    }

    private sealed class RecordingContentHandler : ContentHandlerBase
    {
        private readonly List<string> _events;

        public RecordingContentHandler(List<string> events)
        {
            _events = events;
        }

        public override Task CreatingAsync(CreateContentContext context)
        {
            _events.Add("Creating");

            return Task.CompletedTask;
        }

        public override Task CreatedAsync(CreateContentContext context)
        {
            _events.Add("Created");
            context.ContentItem.DisplayText = TextFromHandler;

            return Task.CompletedTask;
        }

        public override Task UpdatingAsync(UpdateContentContext context)
        {
            _events.Add($"Updating {context.ContentItem.ContentItemId}");

            return Task.CompletedTask;
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            _events.Add($"Updated {context.ContentItem.ContentItemId}");
            context.ContentItem.DisplayText = TextFromHandler;

            return Task.CompletedTask;
        }
    }
}
