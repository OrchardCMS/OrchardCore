using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    public interface IObjectGraphTypeProvider
    {
        void Register(Schema schema);
    }
}
