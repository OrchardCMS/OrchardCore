namespace OrchardCore.Routing.Extensions;

public static class EndpointExtensions
{
    private const string EndpointGroupTypeNameSuffix = "Endpoints";

    public static string GetGroupName(this IEndpoint endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        var groupName = endpoint.GetType().Name;
        if (groupName.EndsWith(EndpointGroupTypeNameSuffix))
        {
            groupName = groupName.Remove(groupName.IndexOf(EndpointGroupTypeNameSuffix), EndpointGroupTypeNameSuffix.Length);
        }

        return groupName;
    }
}
