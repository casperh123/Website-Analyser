using Microsoft.Extensions.Configuration;
using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Services.Services;

namespace WebsiteAnalyzer.Application.Test.Services;

public class AppartmentServiceTests
{
    private readonly AppartmentService _sut;
    
    private static Dictionary<string, string> myConfiguration = new Dictionary<string, string>
    {
        {"Key1", "Value1"},
        {"Nested:Key1", "NestedValue1"},
        {"Nested:Key2", "NestedValue2"}
    };

    private IConfiguration configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(myConfiguration)
        .Build();

    public AppartmentServiceTests()
    {
        HttpClient client = new HttpClient();
        _sut = new AppartmentService(client, new MailService(configuration));
    }

    [Fact]
    public async Task test_cej_responds()
    {
        ICollection<TenancyDto> response = await _sut.GetAppartments();
        
        Assert.NotEmpty(response);
    }
}