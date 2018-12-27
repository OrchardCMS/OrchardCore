using OrchardCore.Entities;
namespace OrchardCore.Users
{
    /// <summary>
    /// Marker interface for ASP.NET Core Identity services.
    /// </summary>
    public interface IUser : IEntity
    {
        string UserName { get; }
    }
}
