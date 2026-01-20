using OrchardCore.Tenants.Workflows.Activities;

namespace OrchardCore.Tenants.Workflows.ViewModels
{
    public class CreateTenantTaskViewModel : TenantTaskViewModel<CreateTenantTask>
    {
        public string DescriptionExpression { get; set; }

        public string RequestUrlPrefixExpression { get; set; }

        public string RequestUrlHostExpression { get; set; }

        public string DatabaseProviderExpression { get; set; }

        public string ConnectionStringExpression { get; set; }

        public string TablePrefixExpression { get; set; }

        public string SchemaExpression { get; set; }

        public string RecipeNameExpression { get; set; }

        public string FeatureProfileExpression { get; set; }
    }
}
