using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Tests.DisplayManagement.Stubs;

internal sealed class GroupDisplayDriverStub : DisplayDriver<GroupModel>
{
    public const string ZoneName = "TestZone";

    private readonly string[] _groupIds;

    public GroupDisplayDriverStub(params string[] groupIds)
    {
        _groupIds = groupIds ?? [];
    }

    public override IDisplayResult Edit(GroupModel model, BuildEditorContext context)
    {
        var result = View("test", model)
            .Location(ZoneName)
            .OnGroup(_groupIds);

        return result;
    }
}

internal sealed class GroupModel
{
    public string Value { get; set; } = "some value";
}
