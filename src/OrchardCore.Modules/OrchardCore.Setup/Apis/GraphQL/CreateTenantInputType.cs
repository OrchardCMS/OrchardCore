using GraphQL.Types;

namespace OrchardCore.Setup.Apis.GraphQL
{
    public class CreateTenantInputType : InputObjectGraphType
    {
        public CreateTenantInputType()
        {
            Name = "SiteSetupInput";

            Field<StringGraphType>("siteName");
            Field<StringGraphType>("databaseProvider");
            //Field<BooleanGraphType>("databaseProviderPreset");
            //Field<StringGraphType>("connectionString");
            //Field<BooleanGraphType>("connectionStringPreset");
            //Field<StringGraphType>("tablePrefix");
            //Field<StringGraphType>("tablePrefixPreset");
            Field<StringGraphType>("userName");
            Field<StringGraphType>("email");
            Field<StringGraphType>("password");
            Field<StringGraphType>("passwordConfirmation");
            Field<StringGraphType>("recipeName");
        }
    }
}
