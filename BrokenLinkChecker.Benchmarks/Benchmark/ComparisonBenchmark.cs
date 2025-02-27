using System.Diagnostics;
using System.Text;
using AngleSharp.Html.Parser;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction
{
    public class UltraFastLinkExtractor
    {
    }
}

public class ComparisonBenchmark
{
    private static readonly Dictionary<string, byte[]> _testData = new();
    private static readonly IHtmlParser _angleSharpParser = new HtmlParser();

    public static async Task RunBenchmarks()
    {
        Console.WriteLine("Preparing test data...");
        PrepareTestData();

        Console.WriteLine("\nRunning benchmarks...\n");
        Console.WriteLine("Custom Implementation:");
        await RunCustomBenchmarks();

        Console.WriteLine("\nAngleSharp Implementation:");
        await RunAngleSharpBenchmarks();
    }

    private static void PrepareTestData()
    {
        _testData["small"] = GenerateHtml(totalSize: 10_000, linkEveryNBytes: 500);
        _testData["medium"] = GenerateHtml(totalSize: 100_000, linkEveryNBytes: 1000);
        _testData["large"] = GenerateHtml(totalSize: 1_000_000, linkEveryNBytes: 2000);
        _testData["sparse"] = GenerateHtml(totalSize: 100_000, linkEveryNBytes: 5000);
        _testData["dense"] = GenerateHtml(totalSize: 100_000, linkEveryNBytes: 200);
    }

    private static async Task RunCustomBenchmarks()
    {
        await RunSingleBenchmark("Small HTML (10KB)", "small", iterations: 1000, useAngleSharp: false);
        await RunSingleBenchmark("Medium HTML (100KB)", "medium", iterations: 1000, useAngleSharp: false);
        await RunSingleBenchmark("Large HTML (1MB)", "large", iterations: 1000, useAngleSharp: false);
        await RunSingleBenchmark("Sparse Links", "sparse", iterations: 1000, useAngleSharp: false);
        await RunSingleBenchmark("Dense Links", "dense", iterations: 1000, useAngleSharp: false);

        await MeasureThroughput(useAngleSharp: false);
    }

    private static async Task RunAngleSharpBenchmarks()
    {
        await RunSingleBenchmark("Small HTML (10KB)", "small", iterations: 1000, useAngleSharp: true);
        await RunSingleBenchmark("Medium HTML (100KB)", "medium", iterations: 1000, useAngleSharp: true);
        await RunSingleBenchmark("Large HTML (1MB)", "large", iterations: 100, useAngleSharp: true);
        await RunSingleBenchmark("Sparse Links", "sparse", iterations: 1000, useAngleSharp: true);
        await RunSingleBenchmark("Dense Links", "dense", iterations: 1000, useAngleSharp: true);

        await MeasureThroughput(useAngleSharp: true);
    }

    private static async Task RunSingleBenchmark(string name, string dataKey, int iterations, bool useAngleSharp)
    {
        var sw = Stopwatch.StartNew();
        long totalLinks = 0;
        long peakMemoryStart = GC.GetTotalMemory(true);

        for (int i = 0; i < iterations; i++)
        {
            if (useAngleSharp)
            {
                var html = Encoding.UTF8.GetString(_testData[dataKey]);
                var document = await _angleSharpParser.ParseDocumentAsync(html);
                var links = document.QuerySelectorAll("a[href]")
                    .Select(element => element.GetAttribute("href"))
                    .Where(href => href != null)
                    .ToList();
                totalLinks += links.Count;
            }
            else
            {
                using var stream = new MemoryStream(_testData[dataKey]);
                var links = await UltraFastLinkExtractor.ExtractHrefsAsync(stream);
                totalLinks += links.Count;
            }
        }

        sw.Stop();
        long peakMemoryEnd = GC.GetTotalMemory(false);
        double memoryMB = (peakMemoryEnd - peakMemoryStart) / (1024.0 * 1024.0);

        double averageMs = sw.ElapsedMilliseconds / (double)iterations;
        double linksPerSecond = totalLinks * 1000.0 / sw.ElapsedMilliseconds;

        Console.WriteLine($"{name}:");
        Console.WriteLine($"  Average time: {averageMs:F2}ms per iteration");
        Console.WriteLine($"  Links found: {totalLinks / iterations:F0} per iteration");
        Console.WriteLine($"  Links per second: {linksPerSecond:F0}");
        Console.WriteLine($"  Memory delta: {memoryMB:F2}MB");
        Console.WriteLine();
    }

    private static async Task MeasureThroughput(bool useAngleSharp)
    {
        const int iterations = 100;
        var sw = Stopwatch.StartNew();
        long totalBytes = 0;

        for (int i = 0; i < iterations; i++)
        {
            foreach (var testFile in _testData.Values)
            {
                if (useAngleSharp)
                {
                    var html = Encoding.UTF8.GetString(testFile);
                    var document = await _angleSharpParser.ParseDocumentAsync(html);
                    _ = document.QuerySelectorAll("a[href]")
                        .Select(element => element.GetAttribute("href"))
                        .Where(href => href != null)
                        .ToList();
                }
                else
                {
                    using var stream = new MemoryStream(testFile);
                    _ = await UltraFastLinkExtractor.ExtractHrefsAsync(stream);
                }

                totalBytes += testFile.Length;
            }
        }

        sw.Stop();
        double seconds = sw.ElapsedMilliseconds / 1000.0;
        double megabytes = totalBytes / (1024.0 * 1024.0);
        double throughput = megabytes / seconds;

        Console.WriteLine("Throughput Test:");
        Console.WriteLine($"  Processed {megabytes:F2}MB in {seconds:F2}s");
        Console.WriteLine($"  Throughput: {throughput:F2}MB/s");
    }

    private static byte[] GenerateHtml(int totalSize, int linkEveryNBytes)
    {
        var sb = new StringBuilder(totalSize);
        sb.Append("<html><body>\n");
        int position = sb.Length;
        int linkCounter = 1;

        while (position < totalSize - 100) // Leave room for closing tags
        {
            // Add filler content
            int contentLength = Math.Min(linkEveryNBytes, totalSize - position - 100);
            if (contentLength > 0)
            {
                sb.Append("<p>").Append('x', contentLength).Append("</p>\n");
                position = sb.Length;
            }

            // Always add at least one link even for small/sparse cases
            if (position < totalSize - 100 || linkCounter == 1)
            {
                sb.Append($"<a href=\"https://example.com/page{linkCounter++}\">Link{linkCounter}</a>\n");
                position = sb.Length;
            }
        }

        sb.Append("</body></html>");
        return Encoding.ASCII.GetBytes(sb.ToString());
    }
}