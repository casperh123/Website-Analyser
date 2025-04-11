using Microsoft.AspNetCore.Identity.UI.Services;
using WebsiteAnalyzer.Web.Services;
using Xunit;

namespace WebsiteAnalyzer.WebTests.Services;

public class MailSenderProviderTests
{
    private IEmailSender _emailSender { get; set; }

    public MailSenderProviderTests()
    {
        _emailSender = new MailSenderProvider();
    }

    [Fact]
    public async Task Test_SendEmailAsync_SendsMai()
    {
        string email = "contact@clyppertechnology.com";
        string subject = "test email";
        string message = "<p>message</p>";
        
        await _emailSender.SendEmailAsync(email, subject, message);
        
        Assert.True(true);
    }
}