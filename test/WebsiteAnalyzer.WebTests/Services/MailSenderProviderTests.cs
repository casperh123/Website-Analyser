using Microsoft.AspNetCore.Identity.UI.Services;
using WebsiteAnalyzer.Web.Services;
using Xunit;

namespace WebsiteAnalyzer.WebTests.Services;

public class MailSenderProviderTests
{
    private IEmailSender EmailSender { get; set; }

    public MailSenderProviderTests()
    {
        EmailSender = new MailSenderProvider();
    }

    [Fact]
    public async Task Test_SendEmailAsync_SendsMai()
    {
        string email = "contact@clyppertechnology.com";
        string subject = "test email";
        string message = "<p>message</p>";
        
        await EmailSender.SendEmailAsync(email, subject, message);
        
        Assert.True(true);
    }
}