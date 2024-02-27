using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Services;

public interface IUserControllerService
{
    Task<bool> SendEmailAsync(string email, string subject, IShape model);

    Task<IUser> RegisterUser(Controller controller, RegisterViewModel model, string confirmationEmailSubject);

    Task<string> SendEmailConfirmationTokenAsync(Controller controller, User user, string subject);
}
