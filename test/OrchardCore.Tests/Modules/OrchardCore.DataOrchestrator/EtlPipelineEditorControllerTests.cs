using System.Text.Json;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.DataOrchestrator.Services;
using OrchardCore.DataOrchestrator.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using DataOrchestratorAdminController = OrchardCore.DataOrchestrator.Controllers.AdminController;

namespace OrchardCore.Tests.Modules.OrchardCore.DataOrchestrator;

public class EtlPipelineEditorControllerTests
{
    [Fact]
    public async Task EditPost_AppliesValidEditorStateAndDropsRemovedActivityTransitions()
    {
        var first = new EtlActivityRecord
        {
            ActivityId = "first",
            Name = nameof(JsonSource),
            IsStart = true,
        };
        var removed = new EtlActivityRecord
        {
            ActivityId = "removed",
            Name = nameof(FilterTransform),
        };
        var load = new EtlActivityRecord
        {
            ActivityId = "load",
            Name = "Load",
        };
        var pipeline = new EtlPipelineDefinition
        {
            Id = 42,
            PipelineId = "pipeline",
            Name = "Pipeline",
            Activities = [first, removed, load],
        };
        var state = JsonSerializer.Serialize(new
        {
            activities = new object[]
            {
                new { id = first.ActivityId, x = 25.6, y = 40.2, isStart = false },
                new { id = load.ActivityId, x = 300.4, y = 90.8, isStart = true },
            },
            transitions = new object[]
            {
                new { sourceActivityId = first.ActivityId, destinationActivityId = load.ActivityId, sourceOutcomeName = "Done" },
                new { sourceActivityId = first.ActivityId, destinationActivityId = removed.ActivityId, sourceOutcomeName = "Done" },
                new { sourceActivityId = first.ActivityId, destinationActivityId = "missing", sourceOutcomeName = "Done" },
            },
            removedActivities = new[] { removed.ActivityId },
        });

        var pipelineService = CreatePipelineService(pipeline);
        var controller = CreateController(pipelineService.Object);

        var result = await controller.Edit(new EtlPipelineEditorUpdateModel
        {
            Id = pipeline.Id,
            State = state,
        }, "save");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirect.ActionName);

        Assert.DoesNotContain(pipeline.Activities, activity => activity.ActivityId == removed.ActivityId);
        Assert.Equal(26, first.X);
        Assert.Equal(40, first.Y);
        Assert.False(first.IsStart);
        Assert.Equal(300, load.X);
        Assert.Equal(91, load.Y);
        Assert.True(load.IsStart);

        var transition = Assert.Single(pipeline.Transitions);
        Assert.Equal(first.ActivityId, transition.SourceActivityId);
        Assert.Equal(load.ActivityId, transition.DestinationActivityId);
        Assert.Equal("Done", transition.SourceOutcomeName);

        pipelineService.Verify(x => x.SaveAsync(pipeline), Times.Once);
    }

    [Fact]
    public async Task EditPost_WithUnreadableEditorState_ReturnsEditorWithoutSaving()
    {
        var pipeline = new EtlPipelineDefinition
        {
            Id = 42,
            PipelineId = "pipeline",
            Name = "Pipeline",
        };
        var pipelineService = CreatePipelineService(pipeline);
        var controller = CreateController(pipelineService.Object);

        var result = await controller.Edit(new EtlPipelineEditorUpdateModel
        {
            Id = pipeline.Id,
            State = "not json",
        }, "save");

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        pipelineService.Verify(x => x.SaveAsync(It.IsAny<EtlPipelineDefinition>()), Times.Never);
    }

    [Fact]
    public async Task EditPost_WhenStartActivityIsRemoved_PromotesRemainingActivity()
    {
        var start = new EtlActivityRecord
        {
            ActivityId = "start",
            Name = nameof(JsonSource),
            IsStart = true,
        };
        var remaining = new EtlActivityRecord
        {
            ActivityId = "remaining",
            Name = "Load",
        };
        var pipeline = new EtlPipelineDefinition
        {
            Id = 42,
            PipelineId = "pipeline",
            Name = "Pipeline",
            Activities = [start, remaining],
        };
        var state = JsonSerializer.Serialize(new
        {
            activities = new object[]
            {
                new { id = remaining.ActivityId, x = 100, y = 120, isStart = false },
            },
            transitions = Array.Empty<object>(),
            removedActivities = new[] { start.ActivityId },
        });

        var pipelineService = CreatePipelineService(pipeline);
        var controller = CreateController(pipelineService.Object);

        var result = await controller.Edit(new EtlPipelineEditorUpdateModel
        {
            Id = pipeline.Id,
            State = state,
        }, "save");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.DoesNotContain(pipeline.Activities, activity => activity.ActivityId == start.ActivityId);
        Assert.True(Assert.Single(pipeline.Activities).IsStart);
    }

    private static Mock<IEtlPipelineService> CreatePipelineService(EtlPipelineDefinition pipeline)
    {
        var pipelineService = new Mock<IEtlPipelineService>(MockBehavior.Strict);
        pipelineService
            .Setup(x => x.GetByDocumentIdAsync(pipeline.Id))
            .ReturnsAsync(pipeline);
        pipelineService
            .Setup(x => x.SaveAsync(pipeline))
            .Returns(Task.CompletedTask);

        return pipelineService;
    }

    private static DataOrchestratorAdminController CreateController(IEtlPipelineService pipelineService)
    {
        var activityLibrary = new Mock<IEtlActivityLibrary>(MockBehavior.Strict);
        activityLibrary
            .Setup(x => x.ListActivities())
            .Returns([]);
        activityLibrary
            .Setup(x => x.ListCategories())
            .Returns([]);

        var stringLocalizer = new Mock<IStringLocalizer<DataOrchestratorAdminController>>();
        stringLocalizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        var controller = new DataOrchestratorAdminController(
            pipelineService,
            Mock.Of<IEtlPipelineExecutor>(),
            activityLibrary.Object,
            Mock.Of<IEtlActivityDisplayManager>(),
            CreateAuthorizationService().Object,
            Mock.Of<IUpdateModelAccessor>(),
            stringLocalizer.Object,
            Mock.Of<IHtmlLocalizer<DataOrchestratorAdminController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, "user-id"),
                        new Claim(ClaimTypes.Name, "admin"),
                    ], "TestAuth")),
                },
            },
        };

        return controller;
    }

    private static Mock<IAuthorizationService> CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService;
    }
}
