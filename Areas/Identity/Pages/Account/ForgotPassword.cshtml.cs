// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TimeMate.Areas.Identity.Data;
using TimeMate.Models;

namespace TimeMate.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<TimeMateUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly smtpSettings _smtpSettings;

        public ForgotPasswordModel(UserManager<TimeMateUser> userManager, IEmailSender emailSender, smtpSettings smtpSettings)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _smtpSettings = smtpSettings;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
            ErrorMessage = "Invalid email address.")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                SendEmail(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            bool SendEmail(string email, string Subject, string confirmLink)
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


            return Page();
        }
    }
}
