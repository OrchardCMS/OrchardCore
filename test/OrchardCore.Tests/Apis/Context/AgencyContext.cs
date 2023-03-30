namespace OrchardCore.Tests.Apis.Context
{
    public class AgencyContext : SiteContext
    {
        static AgencyContext()
        {
        }

        public AgencyContext()
        {
            this.WithRecipe("Agency");
        }
    }
}
