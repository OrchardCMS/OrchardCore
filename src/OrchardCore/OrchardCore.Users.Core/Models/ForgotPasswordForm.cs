using OrchardCore.Entities;

namespace OrchardCore.Users.Models;

public class ForgotPasswordForm : Entity
{
    public string UsernameOrEmail { get; set; }
}
