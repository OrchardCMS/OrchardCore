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
        private static readonly string _typeName = typeof(TAdminNode).Name;

        public string Name => _typeName;

        public AdminNode Create()
        {
            return new TAdminNode();
        }
    }
}
