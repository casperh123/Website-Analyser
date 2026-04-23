using System.Net.Http.Json;
using System.Text.Json;
using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Services.Cache;

namespace WebsiteAnalyzer.Services.Services;

public class AppartmentService(HttpClient client, MailService mailService)
{
    private readonly AppartmentCache _cache = new AppartmentCache();

    public async Task<ICollection<TenancyDto>> GetAppartments()
    {
        var response = await client.GetAsync("https://udlejning.cej.dk/find-bolig/overblik?collection=residences&minRooms=2&monthlyPrice=0-16000&p=sj%C3%A6lland&_data=routes%2Fsearch%2Flayout");
        var raw = await response.Content.ReadAsStringAsync();

        var dataLine = raw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(line => line.StartsWith("data:"));

        if (dataLine is null)
            throw new InvalidOperationException("No data line found in SSE response.");

        var json = dataLine["data:".Length..].Trim();
        var root = JsonDocument.Parse(json).RootElement;

        var items = root
            .GetProperty("searchResponse")
            .GetProperty("items")
            .EnumerateArray();

        var cejTenancies = items.Select(item => new TenancyDto
        {
            Id = item.GetProperty("id").GetString()!,
            Url = $"https://udlejning.cej.dk/boliger/{item.GetProperty("id").GetString()}",
            Name = item.GetProperty("name").GetString()!
        });
        
        JoratoResponse? kerebyResponse = await client.GetFromJsonAsync<JoratoResponse>("https://api.jorato.com/tenancies?visibility=public&showAll=true&key=2gXoBtKvFMMgKJ1VBJ5G5pNr2GD");

        IEnumerable<TenancyDto> kerebyTenancies = kerebyResponse?.items
            .Where(IsAvailable)
            .Where(IsResidential)
            .Select(tenancy => new TenancyDto(tenancy))
            .ToList() ?? [];

        List<TenancyDto> unseen = cejTenancies
            .Concat(kerebyTenancies)
            .Where(t => !_cache.Exists(t.Id))
            .ToList();

        if (unseen.Any())
        {
            _cache.AddAppartments(unseen.Select(t => t.Id));
            
            var body = $"""
                Der er kommet {unseen.Count} nye boliger:<br/><br/>
                {string.Join("<br/>", unseen.Select(t => $"<a href=\"{t.Url}\">{t.Name}</a></br>"))}
                """;

            try
            {
                await mailService.SendEmailAsync("casperholten@protonmail.com", "Nye lejligheder Tilgængelige", body);
                await mailService.SendEmailAsync("ie@live.dk", "Nye lejligheder Tilgængelige", body);
            }
            catch
            {
                // ignored
            }
        }

        return unseen;
    }
    
    public bool IsAvailable(KerebyTenancy a) => a.state == "Available";
    public bool IsResidential(KerebyTenancy t) => t.classification == "Residential";
}

public class TenancyDto
{
    public string Id { get; set; }
    public string Url { get; set; }
    public string Name { get; set; }

    public TenancyDto()
    {
    }

    public TenancyDto(KerebyTenancy tenancy)
    {
        Id = tenancy.id;
        Url = tenancy.Url;
        Name = tenancy.title;
    }
}

public class JoratoResponse
{
    public List<KerebyTenancy> items { get; set; } = [];
}

public class KerebyTenancy
{
    public string state { get; set; } = string.Empty;
    public string classification { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    
    public Address address { get; set; } = new Address();
    public string Url => "https://kerebyudlejning.dk/bolig/" + address.flattened;

    public class Address
    {
        public string street { get; set; } = string.Empty;
        public string zipcode { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;

        public string flattened =>
            (
                street.Replace(" ", "-") +
                "-" +
                zipcode.Replace(" ", "-") +
                "-" +
                city.Replace(" ", "-")
            )
            .ToLower()
            .Replace(".", "")
            .Replace(",", "")
            .Replace("ø", "o")
            .Replace("å", "a")
            .Replace("æ", "a");
    }
}