using System;

namespace BrokenLinkChecker
{
    public class Utilities
    {
        /// <summary>
        /// Resolves a target URL against a base URL. Returns the absolute URL.
        /// </summary>
        /// <param name="baseUrl">The base URL to resolve against.</param>
        /// <param name="target">The target URL to resolve.</param>
        /// <returns>The resolved absolute URL as a string.</returns>
        public static string GetUrl(string baseUrl, string target)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("Base URL must not be null or whitespace.", nameof(baseUrl));
            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentException("Target URL must not be null or whitespace.", nameof(target));

            // Attempt to create a URI from the target, assuming it might be absolute.
            if (Uri.TryCreate(target, UriKind.Absolute, out var fullUri))
                return fullUri.ToString();

            // If not absolute, resolve it against the base URL.
            var baseUri = new Uri(baseUrl);
            var resolvedUri = new Uri(baseUri, target);
            return resolvedUri.ToString();
        }
    }
}