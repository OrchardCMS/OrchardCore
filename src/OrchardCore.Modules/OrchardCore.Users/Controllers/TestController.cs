using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Users.Models;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Users.Controllers
{
    [ApiController]
    [IgnoreAntiforgeryToken]
    [AllowAnonymous]
    [Route("Test")]
    public class TestController : ControllerBase
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly UserManager<IUser> _userManager;

        public TestController(IWorkflowManager workflowManager, UserManager<IUser> userManager)
        {
            _workflowManager = workflowManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Test()
        {
            var user = await _userManager.FindByNameAsync("Admin");
            await TriggerWorkflowEventAsync(nameof(UserCreatedEvent), (User)user);
            return Ok();
        }

        private Task TriggerWorkflowEventAsync(string name, User user)
        {
            return _workflowManager.TriggerEventAsync(name,
                input: new { User = user },
                correlationId: user.Id.ToString()
            );
        }
    }
}
