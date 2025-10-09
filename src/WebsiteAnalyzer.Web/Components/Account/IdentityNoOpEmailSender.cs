using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebsiteAnalyzer.Infrastructure;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Web.Services;

namespace WebsiteAnalyzer.Web.Components.Account;

// Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
{
    [Inject]
    private IEmailSender _emailSender { get; set; }

    public IdentityNoOpEmailSender(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        _emailSender.SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        _emailSender.SendEmailAsync(email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        _emailSender.SendEmailAsync(email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");
}