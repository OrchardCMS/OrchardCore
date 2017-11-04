using GraphQL.Types;

namespace OrchardCore.Setup.Apis.GraphQL
{
    public class CreateTenantOutcomeType : AutoRegisteringObjectGraphType<CreateTenantOutcomeType>
    {
        public CreateTenantOutcomeType()
        {
            Name = "Tenant";
        }
    }
}
