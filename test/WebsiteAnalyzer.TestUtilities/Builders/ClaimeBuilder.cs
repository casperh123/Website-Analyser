using System.Security.Claims;

namespace WebsiteAnalyzer.TestUtilities.Builders;

public class ClaimsBuilder
{
    private IEnumerable<Claim> _claims = [];
    private ClaimsIdentity _identity = new ClaimsIdentity();
    
    public (ClaimsPrincipal, Guid) Default()
    {
        Guid userId = Guid.NewGuid();

        _claims =
        [
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ];

        _identity = new ClaimsIdentity(_claims);

        return (new ClaimsPrincipal(_identity), userId);
    }

    public ClaimsPrincipal Build()
    {
        return new ClaimsPrincipal(_identity);
    }
}