using OrchardCore.Entities;

namespace OrchardCore.Users.Models;

public class ResetPasswordForm : Entity
{
    public string Identifier { get; set; }

    public string NewPassword { get; set; }

    public string ResetToken { get; set; }
}
