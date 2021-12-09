namespace OrchardCore.Users
{
    /// <summary>
    /// Contract for ASP.NET Core Identity services.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets the user name.
        /// </summary>
        string UserName { get; }
    }
}
