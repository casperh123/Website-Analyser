using Microsoft.AspNetCore.Components;

namespace WebsiteAnalyzer.Web.Components.Templates;

public abstract class ProcessingComponentBase : ComponentBase, IDisposable
{
    private CancellationTokenSource? _currentOperation;

    private CancellationToken CurrentToken => _currentOperation?.Token ?? CancellationToken.None;
    
    protected bool IsProcessing { get; private set; }

    protected async Task StartProcessingAsync(Func<CancellationToken, Task> processingTask)
    {
        try
        {
            _currentOperation?.Cancel();
            _currentOperation?.Dispose();
            _currentOperation = new CancellationTokenSource();

            IsProcessing = true;
            await processingTask(CurrentToken);
        }
        finally
        {
            IsProcessing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected void CancelProcessing()
    {
        _currentOperation?.Cancel();
    }

    public virtual void Dispose()
    {
        _currentOperation?.Cancel();
        _currentOperation?.Dispose();
    }
}