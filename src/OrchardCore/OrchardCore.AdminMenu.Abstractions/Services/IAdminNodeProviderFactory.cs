using OrchardCore.AdminMenu.Models;

namespace OrchardCore.AdminMenu.Services
{
    public interface IAdminNodeProviderFactory
    {
        string Name { get; }
        AdminNode Create();
    }

    public class AdminNodeProviderFactory<TAdminNode> : IAdminNodeProviderFactory where TAdminNode : AdminNode, new()
    {
        private static readonly string TypeName = typeof(TAdminNode).Name;

        public string Name => TypeName;

        public AdminNode Create()
        {
            return new TAdminNode();
        }
    }
}
