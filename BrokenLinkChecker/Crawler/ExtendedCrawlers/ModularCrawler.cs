using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.models.Result;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T>(ILinkProcessor<T> linkProcessor)
    where T : Link
{
    private const int DefaultQueueSize = 1000000;
    private const int MaxConcurrentRequests = 1024;
    private const int SlowHostThresholdMs = 5000; // 5 seconds threshold for slow hosts
    
    public async IAsyncEnumerable<CrawlProgress<T>> CrawlWebsiteAsync(T startPage, [EnumeratorCancellation] CancellationToken token = default)
    {
        linkProcessor.FlushCache();
        
        // Create a channel to report progress back from task continuations
        var progressChannel = Channel.CreateUnbounded<CrawlProgress<T>>(new UnboundedChannelOptions { 
            SingleReader = true, 
            SingleWriter = false 
        });
        
        int linksChecked = 0;
        var linkQueue = new ConcurrentQueue<T>();
        
        // More efficient URL storage using BloomFilter + ConcurrentDictionary combination
        var visitedUrlFilter = new ConcurrentBloomFilter(expectedItems: 10_000_000, falsePositiveRate: 0.01);
        var processingUrls = new ConcurrentDictionary<string, byte>();
        
        // Track active hosts and their processing times
        var activeHosts = new ConcurrentDictionary<string, byte>();
        var hostTimers = new ConcurrentDictionary<string, Stopwatch>();
        
        // Track active task count for flow control
        var activeTaskCount = 0;
        
        // Add the start page
        linkQueue.Enqueue(startPage);
        
        // Start a background task to process the queue
        var processingTask = Task.Run(async () => {
            try 
            {
                while (!token.IsCancellationRequested)
                {
                    // Check for slow hosts and potentially release them
                    foreach (var hostEntry in hostTimers.ToArray())
                    {
                        string host = hostEntry.Key;
                        Stopwatch timer = hostEntry.Value;
                        
                        // If this host has been processing for more than the threshold, 
                        // consider it stuck/slow and release it
                        if (timer.ElapsedMilliseconds > SlowHostThresholdMs)
                        {
                            // Remove the timer and release the host
                            if (hostTimers.TryRemove(host, out _) && 
                                activeHosts.TryRemove(host, out _))
                            {
                                // We've released a slow host, so we can start a new task
                                StartNewTaskIfAvailable();
                            }
                        }
                    }
                    
                    // Start new tasks if we have capacity
                    while (Interlocked.Read(activeTaskCount) < MaxConcurrentRequests && 
                           !linkQueue.IsEmpty)
                    {
                        StartNewTaskIfAvailable();
                    }
                    
                    // Check if we're completely done
                    if (linkQueue.IsEmpty && Interlocked.Read(activeTaskCount) == 0 && linksChecked > 0)
                    {
                        break;
                    }
                    
                    await Task.Delay(100, token); // Short delay to prevent CPU spinning
                }
            }
            finally
            {
                // Make sure we complete the progress channel when done
                progressChannel.Writer.Complete();
            }
        }, token);
        
        // Function to start a new task if there are URLs in the queue
        void StartNewTaskIfAvailable()
        {
            if (linkQueue.TryDequeue(out var link))
            {
                string url = link.Target;
                
                // Skip already processed URLs - first check bloom filter for quick rejection
                if (visitedUrlFilter.MightContain(url))
                {
                    // If bloom filter says URL might be visited, double-check with the processingUrls
                    // to avoid rare false positives
                    if (processingUrls.ContainsKey(url))
                        return;
                }
                
                // Mark as processing
                if (!processingUrls.TryAdd(url, 0))
                    return;
                
                // Add to bloom filter
                visitedUrlFilter.Add(url);
                
                // Extract the host from the URL
                string host = GetHostFromUrl(url);
                
                // Skip if we're already processing this host
                if (!activeHosts.TryAdd(host, 0))
                {
                    // Put the link back in the queue for later processing
                    processingUrls.TryRemove(url, out _);
                    linkQueue.Enqueue(link);
                    return;
                }
                
                // Start timing this host
                var timer = new Stopwatch();
                timer.Start();
                hostTimers[host] = timer;
                
                // Increment the active task count
                Interlocked.Increment(ref activeTaskCount);
                
                // Create and start a new task for this link
                var processTask = ProcessLinkAsync(link, host, linkQueue, visitedUrlFilter, processingUrls);
                
                // Add continuation to handle completion and update state
                processTask.ContinueWith(async (completed) => {
                    try 
                    {
                        if (completed.IsCompletedSuccessfully)
                        {
                            var (processedLink, _) = await completed;
                            
                            // Increment the counter and report progress
                            var count = Interlocked.Increment(ref linksChecked);
                            
                            // Write to the progress channel for the main method to consume
                            await progressChannel.Writer.WriteAsync(
                                new CrawlProgress<T>(
                                    processedLink, 
                                    count, 
                                    linkQueue.Count + Interlocked.Read(activeTaskCount)
                                ), 
                                token
                            );
                        }
                    }
                    catch (Exception) 
                    {
                        // Handle any errors in the continuation
                    }
                    finally 
                    {
                        // Always clean up resources
                        hostTimers.TryRemove(host, out _);
                        activeHosts.TryRemove(host, out _);
                        Interlocked.Decrement(ref activeTaskCount);
                        
                        // Immediately try to start a new task to maintain throughput
                        StartNewTaskIfAvailable();
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        
        // Read from the progress channel and yield results
        while (await progressChannel.Reader.WaitToReadAsync(token))
        {
            while (progressChannel.Reader.TryRead(out var progress))
            {
                yield return progress;
            }
        }
        
        // Wait for the processing task to complete
        await processingTask;
    }
    
    private async Task<(T Link, IEnumerable<T> FoundLinks)> ProcessLinkAsync(
        T link,
        string host, 
        ConcurrentQueue<T> linkQueue,
        ConcurrentBloomFilter visitedUrlFilter,
        ConcurrentDictionary<string, byte> processingUrls)
    {
        try
        {
            // Process the link
            var foundLinks = await linkProcessor.ProcessLinkAsync(link).ConfigureAwait(false);
            
            // Collect new links in a local list first for batch processing
            var newLinks = new List<T>();
            
            // Filter and collect new links
            foreach (var foundLink in foundLinks)
            {
                string url = foundLink.Target;
                
                // Skip already visited URLs - using the two-step check for efficiency
                if (visitedUrlFilter.MightContain(url))
                {
                    if (processingUrls.ContainsKey(url))
                        continue;
                }
                
                newLinks.Add(foundLink);
            }
            
            // Add all new links to the bloom filter in batch
            foreach (var newLink in newLinks)
            {
                visitedUrlFilter.Add(newLink.Target);
            }
            
            // Add all new links to the queue
            foreach (var newLink in newLinks)
            {
                linkQueue.Enqueue(newLink);
            }
            
            return (link, foundLinks);
        }
        catch (Exception)
        {
            // Return empty collection on failure
            return (link, Enumerable.Empty<T>());
        }
    }
    
    private string GetHostFromUrl(string url)
    {
        // Extract host from URL
        try
        {
            return new Uri(url).Host;
        }
        catch
        {
            // If parsing fails, use the URL as the host to avoid errors
            return url;
        }
    }
}

/// <summary>
/// Lock-free Bloom filter implementation for efficient URL detection
/// </summary>
public class ConcurrentBloomFilter
{
    private readonly int[] _bitArray;
    private readonly int _hashFunctionCount;
    private readonly int _bitArraySize;
    
    public ConcurrentBloomFilter(long expectedItems, double falsePositiveRate)
    {
        // Calculate optimal size and hash function count based on expected items and false positive rate
        _bitArraySize = CalculateOptimalSize(expectedItems, falsePositiveRate);
        _hashFunctionCount = CalculateHashFunctionCount(_bitArraySize, expectedItems);
        
        // Initialize the bit array (using int array where each bit represents a position)
        _bitArray = new int[(_bitArraySize / 32) + 1];
    }
    
    public void Add(string item)
    {
        var hashValues = ComputeHash(item);
    
        foreach (int position in hashValues)
        {
            int arrayIndex = position / 32;
            int bitPosition = position % 32;
            int mask = 1 << bitPosition;
        
            // Interlocked operations for lock-free updates
            Interlocked.Or(ref _bitArray[arrayIndex], mask);
        }
    }
    
    public bool MightContain(string item)
    {
        var hashValues = ComputeHash(item);
        
        foreach (int position in hashValues)
        {
            int arrayIndex = position / 32;
            int bitPosition = position % 32;
            
            // We don't need atomic reads here - if we occasionally get a false negative
            // due to a race condition, it's not a problem since we'll just process 
            // the URL again
            if ((_bitArray[arrayIndex] & (1 << bitPosition)) == 0)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private int[] ComputeHash(string item)
    {
        // We'll use two hash functions and combine them to create k hash functions
        // This is based on the Kirsch-Mitzenmacher technique
        int hash1 = item.GetHashCode();
        int hash2 = CalculateSecondaryHash(item);
        
        var result = new int[_hashFunctionCount];
        
        for (int i = 0; i < _hashFunctionCount; i++)
        {
            // Combine hash1 and hash2 differently for each hash function
            result[i] = Math.Abs((hash1 + (i * hash2)) % _bitArraySize);
        }
        
        return result;
    }
    
    private int CalculateSecondaryHash(string item)
    {
        // Use a different algorithm for the second hash
        int hash = 0;
        for (int i = 0; i < item.Length; i++)
        {
            hash = (hash * 31) + item[i];
        }
        return hash;
    }
    
    private static int CalculateOptimalSize(long n, double p)
    {
        // Formula: m = -n*ln(p)/(ln(2)^2)
        return (int)Math.Ceiling(-n * Math.Log(p) / (Math.Log(2) * Math.Log(2)));
    }
    
    private static int CalculateHashFunctionCount(int m, long n)
    {
        // Formula: k = (m/n) * ln(2)
        return Math.Max(1, (int)Math.Round((double)m / n * Math.Log(2)));
    }
}