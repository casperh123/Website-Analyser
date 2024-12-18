namespace WebsiteAnalyzer.Web.Services;

public class StateService
{
    public bool IsProcessing { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }

    public StateService()
    {
    }
}