using OrchardCore.Entities;

namespace OrchardCore.Users.Models;

public class RegisterUserForm : Entity
{
    public string UserName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }
}
