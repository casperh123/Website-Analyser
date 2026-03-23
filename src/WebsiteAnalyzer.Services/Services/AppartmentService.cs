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

        var tenancies = items.Select(item => new TenancyDto
        {
            Id = item.GetProperty("id").GetString()!,
            Url = $"https://udlejning.cej.dk/boliger/{item.GetProperty("id").GetString()}",
            Name = item.GetProperty("name").GetString()!
        });

        var unseen = tenancies
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
                await mailService.SendEmailAsync("clypper.tech@protonmail.com", "CEJ Lejlighed Tilgængelig", body);
                await mailService.SendEmailAsync("ie@live.dk", "CEJ Lejlighed Tilgængelig", body);
            }
            catch
            {
                // ignored
            }
        }

        return unseen;
    }
}

public class TenancyDto
{
    public string Id { get; set; }
    public string Url { get; set; }
    public string Name { get; set; }
}