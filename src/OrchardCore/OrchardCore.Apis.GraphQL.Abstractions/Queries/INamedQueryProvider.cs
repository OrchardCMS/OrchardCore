namespace OrchardCore.Apis.GraphQL.Queries;

public interface INamedQueryProvider
{
    IDictionary<string, string> Resolve();
}
