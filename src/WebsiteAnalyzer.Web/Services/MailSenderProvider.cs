using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using WebsiteAnalyzer.Application.Services;

namespace WebsiteAnalyzer.Web.Services;

public class MailSenderProvider : IEmailSender
{
    private readonly MailService _mailService;

    public MailSenderProvider(MailService mailService)
    {
        _mailService = mailService;
    }
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _mailService.SendEmailAsync(email, subject, htmlMessage);
    }
}