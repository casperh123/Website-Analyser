using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.DocumentParsing.LinkFilter;

public interface ILinkFilter<T> where T : Link
{
    public T? Filter(T link);
}