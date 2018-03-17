using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly ILogger _logger;
        private readonly ISiteService _siteService;

        public AccountController(
            IUserService userService,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            ILogger<AccountController> logger,
            ISiteService siteService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
            _siteService = siteService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning(2, "User account locked out.");
                //    return View("Lockout");
                //}
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersCanRegister)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            if (!(await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>().UsersCanRegister)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userService.CreateUserAsync(model.UserName, model.Email, new string[0], model.Password, (key, message) => ModelState.AddModelError(key, message));

                if (user != null)
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                    //    $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User created a new account with password.");
                    return RedirectToLocal(returnUrl);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");

            return Redirect("~/");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetAuthenticatedUserAsync(User);
                if (await _userService.ChangePasswordAsync(user, model.CurrentPassword, model.Password, (key, message) => ModelState.AddModelError(key, message)))
                {
                    return RedirectToLocal(Url.Action("ChangePasswordConfirmation"));
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Redirect("~/");
            }
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpPost]
        [SecurityHeaders]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            // start challenge and roundtrip the return URL and 
            var props = new AuthenticationProperties()
            {
                RedirectUri = Url.Action("ExternalLoginCallback"),
                Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
            };
            return Challenge(props, provider);
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        [SecurityHeaders]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(IdentityModel.JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _signInManager.UserManager.FindByLoginAsync(provider, providerUserId);

            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = await AutoProvisionUser(provider, providerUserId, claims);
            }

            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            //await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username));
            // await HttpContext.SignInAsync( user, provider, localSignInProps, additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            await _signInManager.RefreshSignInAsync(user);

            // validate return URL and redirect back to authorization endpoint or a local page
            var returnUrl = result.Properties.Items["returnUrl"];
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }

        private async Task<IUser> AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            try
            {
                var emailClaim = claims.FirstOrDefault(c => c.Type == IdentityModel.JwtClaimTypes.Email) ??
                                  claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

                if (emailClaim == null)
                {
                    throw new ArgumentNullException(nameof(emailClaim), "Could not find claim for Email");
                }

                var user = await _signInManager.UserManager.FindByEmailAsync(emailClaim.Value);

                var userCanRegister = false;

                if (user == null)
                {
                    userCanRegister = (await _siteService.GetSiteSettingsAsync()).As<RegistrationSettings>()?.UsersCanRegister ?? false;
                    if (userCanRegister)
                    {
                        var nameClaim = claims.FirstOrDefault(c => c.Type == IdentityModel.JwtClaimTypes.PreferredUserName) ??
                                        claims.FirstOrDefault(c => c.Type == IdentityModel.JwtClaimTypes.Name) ??
                                        claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                        string userName = nameClaim == null ? emailClaim.Value.Substring(0, emailClaim.Value.IndexOf("@")) : nameClaim.Value;

                        userName = userName.Replace(" ", "");
                        int tries = 1;
                        Random r = new Random();
                        if (await _signInManager.UserManager.FindByNameAsync(userName) != null)
                            userName = userName + r.Next(10);

                        while (user == null && tries < 10)
                        {
                            user = await _userService.CreateUserAsync(userName, emailClaim.Value, new string[] { }, GenerateTempPassword(), (key, message) =>
                            {
                                _logger.LogWarning(string.IsNullOrWhiteSpace(key) ? message : $"{key}: {message}");
                            });
                            userName = userName + r.Next(10);
                            tries++;
                        }
                    }
                }

                if (user == null)
                    throw new InvalidOperationException($"Could not create user. Allow register users: {userCanRegister}");

                await _signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not auto provision user {providerUserId} from {provider} provider", ex);
            }
        }

        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == IdentityModel.JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(IdentityModel.JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }
        }


        // based on the sample from http://www.obviex.com/Samples/Password.aspx
        // We could create a service to provide strong temp passwords and settings 
        // for temp password generation

        // Define supported password characters divided into groups.
        // You can add (or remove) characters to (from) these groups.
        private static string PASSWORD_CHARS_LCASE = "abcdefgijkmnopqrstwxyz";
        private static string PASSWORD_CHARS_UCASE = "ABCDEFGHJKLMNPQRSTWXYZ";
        private static string PASSWORD_CHARS_NUMERIC = "23456789";
        private static string PASSWORD_CHARS_SPECIAL = "*$-+?_&=!%{}/";

        /// <summary>
        /// Generates a random password.
        /// </summary>
        /// <param name="minLength">
        /// Minimum password length.
        /// </param>
        /// <param name="maxLength">
        /// Maximum password length.
        /// </param>
        /// <returns>
        /// Randomly generated password.
        /// </returns>
        /// <remarks>
        /// The length of the generated password will be determined at
        /// random and it will fall with the range determined by the
        /// function parameters.
        /// </remarks>
        private string GenerateTempPassword()
        {
            var minLength = _signInManager.Options.Password.RequiredLength;
            var maxLength = _signInManager.Options.Password.RequiredLength;
            // Make sure that input parameters are valid.
            if (minLength <= 0 || maxLength <= 0 || minLength > maxLength)
                return null;

            // Create a local array containing supported password characters
            // grouped by types. You can remove character groups from this
            // array, but doing so will weaken the password strength.
            char[][] charGroups = new char[][]
            {
            PASSWORD_CHARS_LCASE.ToCharArray(),
            PASSWORD_CHARS_UCASE.ToCharArray(),
            PASSWORD_CHARS_NUMERIC.ToCharArray(),
            PASSWORD_CHARS_SPECIAL.ToCharArray()
            };

            // Use this array to track the number of unused characters in each
            // character group.
            int[] charsLeftInGroup = new int[charGroups.Length];

            // Initially, all characters in each group are not used.
            for (int i = 0; i < charsLeftInGroup.Length; i++)
                charsLeftInGroup[i] = charGroups[i].Length;

            // Use this array to track (iterate through) unused character groups.
            int[] leftGroupsOrder = new int[charGroups.Length];

            // Initially, all character groups are not used.
            for (int i = 0; i < leftGroupsOrder.Length; i++)
                leftGroupsOrder[i] = i;

            // Because we cannot use the default randomizer, which is based on the
            // current time (it will produce the same "random" number within a
            // second), we will use a random number generator to seed the
            // randomizer.

            // Use a 4-byte array to fill it with random bytes and convert it then
            // to an integer value.
            byte[] randomBytes = new byte[4];

            // Generate 4 random bytes.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);

            // Convert 4 bytes into a 32-bit integer value.
            int seed = BitConverter.ToInt32(randomBytes, 0);

            // Now, this is real randomization.
            Random random = new Random(seed);

            // This array will hold password characters.
            char[] password = null;

            // Allocate appropriate memory for the password.
            if (minLength < maxLength)
                password = new char[random.Next(minLength, maxLength + 1)];
            else
                password = new char[minLength];

            // Index of the next character to be added to password.
            int nextCharIdx;

            // Index of the next character group to be processed.
            int nextGroupIdx;

            // Index which will be used to track not processed character groups.
            int nextLeftGroupsOrderIdx;

            // Index of the last non-processed character in a group.
            int lastCharIdx;

            // Index of the last non-processed group.
            int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

            // Generate password characters one at a time.
            for (int i = 0; i < password.Length; i++)
            {
                // If only one character group remained unprocessed, process it;
                // otherwise, pick a random character group from the unprocessed
                // group list. To allow a special character to appear in the
                // first position, increment the second parameter of the Next
                // function call by one, i.e. lastLeftGroupsOrderIdx + 1.
                if (lastLeftGroupsOrderIdx == 0)
                    nextLeftGroupsOrderIdx = 0;
                else
                    nextLeftGroupsOrderIdx = random.Next(0, lastLeftGroupsOrderIdx);

                // Get the actual index of the character group, from which we will
                // pick the next character.
                nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];

                // Get the index of the last unprocessed characters in this group.
                lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

                // If only one unprocessed character is left, pick it; otherwise,
                // get a random character from the unused character list.
                if (lastCharIdx == 0)
                    nextCharIdx = 0;
                else
                    nextCharIdx = random.Next(0, lastCharIdx + 1);

                // Add this character to the password.
                password[i] = charGroups[nextGroupIdx][nextCharIdx];

                // If we processed the last character in this group, start over.
                if (lastCharIdx == 0)
                    charsLeftInGroup[nextGroupIdx] =
                                              charGroups[nextGroupIdx].Length;
                // There are more unprocessed characters left.
                else
                {
                    // Swap processed character with the last unprocessed character
                    // so that we don't pick it until we process all characters in
                    // this group.
                    if (lastCharIdx != nextCharIdx)
                    {
                        char temp = charGroups[nextGroupIdx][lastCharIdx];
                        charGroups[nextGroupIdx][lastCharIdx] =
                                    charGroups[nextGroupIdx][nextCharIdx];
                        charGroups[nextGroupIdx][nextCharIdx] = temp;
                    }
                    // Decrement the number of unprocessed characters in
                    // this group.
                    charsLeftInGroup[nextGroupIdx]--;
                }

                // If we processed the last group, start all over.
                if (lastLeftGroupsOrderIdx == 0)
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                // There are more unprocessed groups left.
                else
                {
                    // Swap processed group with the last unprocessed group
                    // so that we don't pick it until we process all groups.
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                    {
                        int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                        leftGroupsOrder[lastLeftGroupsOrderIdx] =
                                    leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }
                    // Decrement the number of unprocessed groups.
                    lastLeftGroupsOrderIdx--;
                }
            }

            // Convert password characters into a string and return the result.
            return new string(password);
        }

    }
}