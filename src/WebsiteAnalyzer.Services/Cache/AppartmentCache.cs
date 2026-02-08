namespace WebsiteAnalyzer.Services.Cache;

public class AppartmentCache
{
    private readonly HashSet<string> _appartments = new HashSet<string>();
    
    public AppartmentCache()
    {
        
    }

    public void AddAppartments(IEnumerable<string> id)
    {
        _appartments.UnionWith(id);
    }

    public bool Exists(string id)
    {
        return _appartments.Contains(id);
    }
}