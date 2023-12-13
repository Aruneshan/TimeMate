#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using TimeMate.Areas.Identity.Data;
using TimeMate.Models;

namespace TimeMate.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<TimeMateUser> _signInManager;
        private readonly UserManager<TimeMateUser> _userManager;
        private readonly IUserStore<TimeMateUser> _userStore;
        private readonly IUserEmailStore<TimeMateUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager <IdentityRole> _roleManager;
        private readonly smtpSettings _smtpSettings;

        public RegisterModel(
            UserManager<TimeMateUser> userManager,
            IUserStore<TimeMateUser> userStore,
            SignInManager<TimeMateUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IOptions<smtpSettings> smtpSettings,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _smtpSettings = smtpSettings.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [System.ComponentModel.DataAnnotations.Required]
            [Display(Name = "FirstName")]
            public string FirstName { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            [Display(Name = "LastName")]
            public string LastName { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
                ErrorMessage = "The password does not meet the required criteria.")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [System.ComponentModel.DataAnnotations.Required]
            public string Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Input = new InputModel()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i

                })
            };
        }
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;


                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, Input.Role);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    SendEmail(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private bool SendEmail(string email, string Subject, string confirmLink)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(_smtpSettings.fromEmail);
                message.To.Add(email);
                message.Subject = Subject;
                message.IsBodyHtml = true;
                message.Body = confirmLink;

                smtp.Port = _smtpSettings.smtpPort;
                smtp.Host = _smtpSettings.smtpHost;

                smtp.EnableSsl = _smtpSettings.enableSsl;
                smtp.UseDefaultCredentials = _smtpSettings.useDefaultCredentials;
                smtp.Credentials = new NetworkCredential(_smtpSettings.userName, _smtpSettings.password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private TimeMateUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<TimeMateUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(TimeMateUser)}'. " +
                    $"Ensure that '{nameof(TimeMateUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<TimeMateUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<TimeMateUser>)_userStore;
        }
    }
}
