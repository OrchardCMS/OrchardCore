namespace OrchardCore.Tests.Apis.Context
{
    public class AgencyContext : SiteContext
    {
        public AgencyContext()
        {
            this.WithRecipe("Agency");
        }
    }
}
