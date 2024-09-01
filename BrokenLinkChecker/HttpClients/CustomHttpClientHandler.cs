using System.IO.Compression;
using System.Net.Http.Headers;

namespace BrokenLinkChecker.HttpClients;

public class CustomHttpClientHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        // Capture the original Content-Encoding header before any decompression
        List<string> originalContentEncoding = response.Content.Headers.ContentEncoding.ToList();

        if (!originalContentEncoding.Any())
        {
            return response;
        }
        
        Stream originalStream = await response.Content.ReadAsStreamAsync();
        Stream decompressedStream = originalStream;

        // Handle gzip, deflate, and br (Brotli) decompression manually
        if (originalContentEncoding.Contains("gzip"))
        {
            decompressedStream = new GZipStream(originalStream, CompressionMode.Decompress);
        }
        else if (originalContentEncoding.Contains("deflate"))
        {
            decompressedStream = new DeflateStream(originalStream, CompressionMode.Decompress);
        }
        else if (originalContentEncoding.Contains("br"))
        {
            decompressedStream = new BrotliStream(originalStream, CompressionMode.Decompress);
        }

        // Copy the decompressed stream into a MemoryStream to calculate its length
        MemoryStream memoryStream = new MemoryStream();
        await decompressedStream.CopyToAsync(memoryStream, cancellationToken);

        // Reset the memoryStream position to the beginning
        memoryStream.Position = 0;

        StreamContent newContent = new StreamContent(memoryStream);
        
        // Copy the headers from the original content
        foreach (var header in response.Content.Headers)
        {
            newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        newContent.Headers.ContentLength = memoryStream.Length;
        
        response.Content = newContent;

        return response;
    }
}
