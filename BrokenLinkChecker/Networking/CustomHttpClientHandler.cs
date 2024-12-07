using System.IO.Compression;

namespace BrokenLinkChecker.Networking;

public class CustomHttpClientHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        
        // If no content encoding, return as-is
        if (!response.Content.Headers.ContentEncoding.Any())
            return response;

        // Create a stream that will handle the decompression on-demand
        var originalStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var decompressionStream = CreateDecompressionStream(originalStream, 
            response.Content.Headers.ContentEncoding.ToList());

        // Create new content without loading everything into memory
        var newContent = new StreamContent(decompressionStream);

        // Copy headers excluding content-encoding and content-length
        foreach (var header in response.Content.Headers)
        {
            if (header.Key.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                continue;
                
            newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        response.Content = newContent;
        return response;
    }

    private static Stream CreateDecompressionStream(Stream originalStream, IList<string> encodings)
    {
        // Handle encodings in reverse order as they were applied
        Stream currentStream = originalStream;

        foreach (var encoding in encodings.Reverse())
        {
            currentStream = encoding.ToLowerInvariant() switch
            {
                "gzip" => new GZipStream(currentStream, CompressionMode.Decompress, leaveOpen: false),
                "deflate" => new DeflateStream(currentStream, CompressionMode.Decompress, leaveOpen: false),
                "br" => new BrotliStream(currentStream, CompressionMode.Decompress, leaveOpen: false),
                _ => currentStream
            };
        }

        return currentStream;
    }
}