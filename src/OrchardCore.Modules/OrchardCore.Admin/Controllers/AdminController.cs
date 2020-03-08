/*
	defines the class AdminController <- Controller
		implementing method Index
			returning the IActionResult result of the View method
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Admin.Controllers
{
	[Authorize] // locks the controller
	public class AdminController: Controller
	{
		// returns an View() result
		public IActionResult Index ()
		{
			return View ();
		}
	}
}
