using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace WebsiteAnalyzer.Application.Services;

public class MailService
{
    private readonly IConfiguration _configuration;

    public MailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        string? smtpUsername = _configuration["MAILSENDER_USERNAME"];
        string? smtpPassword = _configuration["MAILSENDER_PASSWORD"];
        
        if (smtpUsername is null || smtpPassword is null)
        {
            throw new AuthenticationException();
        }
        
        MimeMessage message = new MimeMessage();
        
        message.From.Add(new MailboxAddress("Clypper's Website Analyzer", "noreply@clyppertechnology.com"));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = htmlMessage
        };

        using SmtpClient client = new SmtpClient();
        
        await client.ConnectAsync("smtp.mailersend.net", 587);
        await client.AuthenticateAsync(smtpUsername, smtpPassword);
            
        await client.SendAsync(message);
        await client.DisconnectAsync(true); 
    }
}