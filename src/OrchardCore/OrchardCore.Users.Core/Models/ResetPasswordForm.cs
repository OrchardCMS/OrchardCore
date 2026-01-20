using OrchardCore.Entities;

namespace OrchardCore.Users.Models;

public class ResetPasswordForm : Entity
{
    public string UsernameOrEmail { get; set; }

    public string NewPassword { get; set; }

    public string ResetToken { get; set; }
}
