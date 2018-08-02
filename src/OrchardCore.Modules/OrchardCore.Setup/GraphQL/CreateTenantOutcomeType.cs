using GraphQL.Types;

namespace OrchardCore.Setup.GraphQL
{
    public class CreateTenantOutcomeType : AutoRegisteringObjectGraphType<CreateTenantOutcome>
    {
        public CreateTenantOutcomeType()
        {
            Name = "Tenant";
        }
    }
}
