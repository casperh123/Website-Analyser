namespace WebsiteAnalyzer.Core.Exceptions;

public class AlreadyScheduledException : Exception
{
    public AlreadyScheduledException(string message) : base(message)
    {
        
    }
}