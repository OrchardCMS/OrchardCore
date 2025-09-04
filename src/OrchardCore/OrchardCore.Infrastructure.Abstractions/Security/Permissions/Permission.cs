using System.Security.Claims;

namespace OrchardCore.Security.Permissions;

/// <summary>
/// Represents a permission within the system.
/// </summary>
/// <remarks>
/// The permission name must be unique across the system.
/// </remarks>
public class Permission
{
    public const string ClaimType = "Permission";

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission"/> class.
    /// </summary>
    /// <param name="name">The name of the permission.</param>
    /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
    /// <remarks>
    /// The permission name must be unique across the system.
    /// </remarks>
    public Permission(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission"/> class with a description and security flag.
    /// </summary>
    /// <param name="name">The name of the permission.</param>
    /// <param name="description">The description of the permission.</param>
    /// <param name="isSecurityCritical">Indicates whether the permission is security critical.</param>
    /// <remarks>
    /// The permission name must be unique across the system.
    /// </remarks>
    public Permission(string name, string description, bool isSecurityCritical = false) : this(name)
    {
        Description = description;
        IsSecurityCritical = isSecurityCritical;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Permission"/> class with a description, implying permissions, and security flag.
    /// </summary>
    /// <param name="name">The name of the permission.</param>
    /// <param name="description">The description of the permission.</param>
    /// <param name="impliedBy">The permissions implying this permission.</param>
    /// <param name="isSecurityCritical">Indicates whether the permission is security critical.</param>
    /// <remarks>
    /// The permission name must be unique across the system.
    /// </remarks>
    public Permission(string name, string description, IEnumerable<Permission> impliedBy, bool isSecurityCritical = false) : this(name, description, isSecurityCritical)
    {
        ImpliedBy = impliedBy;
    }

    /// <summary>
    /// Gets the name of the permission.
    /// </summary>
    /// <remarks>
    /// The permission name must be unique across the system.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the description of the permission.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the category of the permission.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Gets the permissions implying this permission.
    /// </summary>
    public IEnumerable<Permission> ImpliedBy { get; }

    /// <summary>
    /// Gets a value indicating whether the permission is security critical.
    /// </summary>
    public bool IsSecurityCritical { get; }

    /// <summary>
    /// Converts a <see cref="Permission"/> to a <see cref="Claim"/>.
    /// </summary>
    /// <param name="p">The permission to convert.</param>
    /// <returns>A claim representing the permission.</returns>
    public static implicit operator Claim(Permission p)
    {
        return new Claim(ClaimType, p.Name);
    }
}
