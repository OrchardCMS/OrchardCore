using GraphQL.Types;

namespace OrchardCore.Setup.Apis.GraphQL
{
    public class CreateTenantOutcomeType : AutoRegisteringObjectGraphType<CreateTenantOutcome>
    {
        public CreateTenantOutcomeType()
        {
            Name = "Tenant";
        }
    }
}
