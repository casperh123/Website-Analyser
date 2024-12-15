using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public class StreamingLinkParser
{
    // Use a HashSet for automatic uniqueness checks, avoiding Distinct().
    private static readonly Regex LinkRegex = new Regex(
        @"(?:http[s]?:\/\/.)?(?:www\.)?[-a-zA-Z0-9@%._\+~#=]{2,256}\.[a-z]{2,6}\b(?:[-a-zA-Z0-9@:%_\+.~#?&\/\/=]*)", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<IEnumerable<string>> ParseLinksAsync(Stream htmlStream)
    {
        var links = new HashSet<string>();
        var buffer = new char[8192]; // Buffer size for streaming
        int leftoverLength = 0;

        using var reader = new StreamReader(htmlStream, Encoding.UTF8);

        int bytesRead;
        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            // Process the new chunk and leftover from the previous chunk
            ReadOnlySpan<char> chunk = new ReadOnlySpan<char>(buffer, 0, buffer.Length);
            
            Regex.ValueMatchEnumerator match = LinkRegex.EnumerateMatches(chunk);

            while (match.MoveNext())
            {
                ValueMatch currentMatch = match.Current;
                ReadOnlySpan<char> linkSpan = chunk.Slice(currentMatch.Index, currentMatch.Length);
                
                links.Add(new string(linkSpan));
            }
        }

        return links;
    }
}