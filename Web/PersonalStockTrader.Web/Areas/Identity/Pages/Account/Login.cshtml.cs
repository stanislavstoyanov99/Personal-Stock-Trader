﻿namespace PersonalStockTrader.Web.Areas.Identity.Pages.Account
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    using PersonalStockTrader.Data.Models;

    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<LoginModel> logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ModelState.AddModelError(string.Empty, this.ErrorMessage);
            }

            returnUrl ??= this.Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await this.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            this.ExternalLogins = (await this.signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            this.ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= this.Url.Content("~/");

            if (this.ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await this.signInManager.PasswordSignInAsync(this.Input.Username, this.Input.Password, this.Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    this.logger.LogInformation("User logged in.");

                    var user = await this.signInManager.UserManager.FindByNameAsync(this.Input.Username);

                    var isInRoleAdmin = await this.signInManager.UserManager.IsInRoleAsync(user, GlobalConstants.AdministratorRoleName);
                    var isInRoleAccountManager = await this.signInManager.UserManager.IsInRoleAsync(user, GlobalConstants.AccountManagerRoleName);
                    var isInRoleConfirmedUser = await this.signInManager.UserManager.IsInRoleAsync(user, GlobalConstants.ConfirmedUserRoleName);
                    var isInRoleNotConfirmedUser = await this.signInManager.UserManager.IsInRoleAsync(user, GlobalConstants.NotConfirmedUserRoleName);

                    if (isInRoleAdmin)
                    {
                        return this.LocalRedirect("~/Administration/Dashboard");
                    }
                    else if (isInRoleAccountManager)
                    {
                        return this.LocalRedirect("~/AccountManagement/NewClients");
                    }
                    else if (isInRoleConfirmedUser)
                    {
                        return this.LocalRedirect("~/User/Trade");
                    }
                    else if (isInRoleNotConfirmedUser)
                    {
                        return this.LocalRedirect("~/User/Trade");
                    }
                }

                if (result.RequiresTwoFactor)
                {
                    return this.RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = this.Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    this.logger.LogWarning(GlobalConstants.LockOut);
                    return this.RedirectToPage("./Lockout");
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, GlobalConstants.InvalidLogin);
                    return this.Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return this.Page();
        }
    }
}
