using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.RealTime;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.RealTime;

public sealed class WorkflowsHubTests
{
    private readonly Mock<IGroupManager> _groups = new();
    private readonly Mock<IHubCallerClients> _clients = new();
    private readonly Mock<IClientProxy> _others = new();
    private readonly Mock<IClientProxy> _group = new();
    private readonly Mock<ISingleClientProxy> _newcomer = new();
    private readonly Dictionary<object, object> _items = [];

    public WorkflowsHubTests()
    {
        _clients.Setup(x => x.OthersInGroup("workflow-type:type-1")).Returns(_others.Object);
        _clients.Setup(x => x.Group("workflow-type:type-1")).Returns(_group.Object);
        _clients.Setup(x => x.Client("connection-2")).Returns(_newcomer.Object);
    }

    [Fact]
    public async Task SubscribeWorkflowType_CanManageWorkflows_JoinsTheGroupAndTellsTheOthers()
    {
        var hub = CreateHub(canManage: true);

        Assert.True(await hub.SubscribeWorkflowType("type-1"));

        _groups.Verify(x => x.AddToGroupAsync("connection-1", "workflow-type:type-1", It.IsAny<CancellationToken>()), Times.Once);
        _others.Verify(x => x.SendCoreAsync("PresenceJoined", It.Is<object[]>(args => IsAlice(args)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Subscribe_CannotManageWorkflows_IsRejected()
    {
        var hub = CreateHub(canManage: false);

        Assert.False(await hub.SubscribeWorkflowType("type-1"));
        Assert.False(await hub.SubscribeInstance("workflow-1"));

        _groups.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AnnouncePresence_OnlyFromASubscriber_TellsTheNewcomer()
    {
        var hub = CreateHub(canManage: true);

        await hub.AnnouncePresence("type-1", "connection-2");

        _newcomer.Verify(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Never);

        await hub.SubscribeWorkflowType("type-1");
        await hub.AnnouncePresence("type-1", "connection-2");

        _newcomer.Verify(x => x.SendCoreAsync("PresenceHere", It.Is<object[]>(args => IsAlice(args)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnsubscribeAndDisconnect_TellTheGroupThatTheUserLeft()
    {
        var hub = CreateHub(canManage: true);
        await hub.SubscribeWorkflowType("type-1");

        await hub.UnsubscribeWorkflowType("type-1");

        _groups.Verify(x => x.RemoveFromGroupAsync("connection-1", "workflow-type:type-1", It.IsAny<CancellationToken>()), Times.Once);
        _group.Verify(x => x.SendCoreAsync("PresenceLeft", It.Is<object[]>(args => (string)args[0] == "connection-1"), It.IsAny<CancellationToken>()), Times.Once);

        // A disconnection only tells the groups the connection is still in.
        await hub.SubscribeWorkflowType("type-1");
        await hub.OnDisconnectedAsync(null);

        _group.Verify(x => x.SendCoreAsync("PresenceLeft", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SubscribeInstance_CanManageWorkflows_JoinsTheInstanceGroup()
    {
        var hub = CreateHub(canManage: true);

        Assert.True(await hub.SubscribeInstance("workflow-1"));

        _groups.Verify(x => x.AddToGroupAsync("connection-1", "workflow:workflow-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_Changes_GoToTheGroupsOfTheirTypeOrInstance()
    {
        var hub = new Mock<IHubContext<WorkflowsHub>>();
        var clients = new Mock<IHubClients>();
        var typeGroup = new Mock<IClientProxy>();
        var instanceGroup = new Mock<IClientProxy>();
        hub.SetupGet(x => x.Clients).Returns(clients.Object);
        clients.Setup(x => x.Group("workflow-type:type-1")).Returns(typeGroup.Object);
        clients.Setup(x => x.Group("workflow:workflow-1")).Returns(instanceGroup.Object);

        await SignalRWorkflowDesignerNotifier.SendAsync(hub.Object, new WorkflowTypeChange { Kind = WorkflowTypeChangeKind.Published, WorkflowTypeId = "type-1", UserName = "alice" });
        await SignalRWorkflowDesignerNotifier.SendAsync(hub.Object, new WorkflowInstanceChange { WorkflowId = "workflow-1", WorkflowTypeId = "type-1", Status = WorkflowStatus.Finished });

        typeGroup.Verify(x => x.SendCoreAsync("WorkflowTypeChanged", It.Is<object[]>(args => args.Length == 1), It.IsAny<CancellationToken>()), Times.Once);
        instanceGroup.Verify(x => x.SendCoreAsync("InstanceChanged", It.Is<object[]>(args => args.Length == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    private WorkflowsHub CreateHub(bool canManage)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(canManage ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        var context = new Mock<HubCallerContext>();
        context.SetupGet(x => x.ConnectionId).Returns("connection-1");
        context.SetupGet(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "alice-id"), new Claim(ClaimTypes.Name, "alice")], "Test")));
        context.SetupGet(x => x.Items).Returns(_items);

        return new WorkflowsHub(authorization.Object)
        {
            Context = context.Object,
            Groups = _groups.Object,
            Clients = _clients.Object,
        };
    }

    private static bool IsAlice(object[] args)
        => args.Length == 1 && args[0] is WorkflowPresence presence && presence.UserName == "alice" && presence.UserId == "alice-id" && presence.ConnectionId == "connection-1";
}
