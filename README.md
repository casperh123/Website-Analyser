# Website Analyzer

A comprehensive website analysis tool that helps developers and website owners maintain and optimize their web presence. This all-in-one solution combines broken link checking, cache warming, and detailed website metrics to ensure your website performs at its best.

## 🚀 Key Features

### Broken Link Detection
- Automatically scan your entire website for broken links and dead endpoints
- Detect both internal and external broken links
- Generate detailed reports of link health
- Prevent SEO penalties from broken links
- Maintain user trust with perfectly functioning navigation

### Cache Warming
- Proactively warm your website's cache to ensure optimal performance
- Schedule automated cache warming sessions
- Reduce first-visit latency for your users
- Improve overall website response times
- Perfect for high-traffic websites and e-commerce platforms

### Website Development Insights
- Get comprehensive technical metrics about your website
- Analyze page load times and performance bottlenecks
- Review your website's structure and navigation paths
- Monitor resource usage and optimization opportunities
- Track changes in your website's technical health over time

Built with .NET 8.0, this tool features a modern Blazor web interface and robust link checking capabilities, making it perfect for developers, SEO specialists, and website administrators who need reliable website maintenance tools.

## Project Structure

The project is organized into several components:

- **BrokenLinkChecker**: Core library responsible for detecting and reporting broken links within websites. Features include:
    - Advanced crawling capabilities with customizable crawlers
    - Modular link extraction system
    - Sophisticated document parsing
    - Custom HTTP handling with SSL/TLS support
    - Support for various resource types and header processing

- **WebsiteAnalyzer.Core**: Core domain logic and business rules
- **WebsiteAnalyzer.Infrastructure**: Infrastructure layer containing external service implementations
- **WebsiteAnalyzer.Web**: Modern Blazor Server web interface featuring:
    - User authentication and authorization
    - Interactive website analysis dashboard
    - Cache warming capabilities
    - Real-time crawl monitoring
    - Website metrics visualization

## Technical Requirements

- .NET 8.0 SDK
- SQLite database
- Modern web browser supporting WebAssembly

## Dependencies

The project uses several key packages:
- **BrokenLinkChecker**:
    - AngleSharp 1.1.2 (HTML parsing)
    - HtmlAgilityPack 1.11.61 (HTML processing)
    - Serilog 4.0.0 (Logging)

- **WebsiteAnalyzer.Web**:
    - Blazor Server
    - Entity Framework Core with SQLite
    - ASP.NET Core Identity
    - Radzen Components

## Getting Started

1. **Prerequisites**
    - Install .NET 8.0 SDK
    - Ensure you have a compatible IDE (Visual Studio 2022 or JetBrains Rider recommended)

2. **Setup**
   ```bash
   # Clone the repository
   git clone [repository-url]
   
   # Navigate to the Web project
   cd Website-Analyser/WebsiteAnalyzer.Web
   
   # Restore dependencies
   dotnet restore
   
   # Update database
   dotnet ef database update
   ```

3. **Running the Application**
   ```bash
   dotnet run
   ```
   The application will be available at `https://localhost:5001` or `http://localhost:5000`

## Development

To generate database migrations, use the provided script:
```bash
./generate-migrations.sh
```

### Architecture Notes

- The solution follows Clean Architecture principles
- BrokenLinkChecker is the core library handling all link checking logic
- WebsiteAnalyzer.Web is a modern Blazor Server application with real-time updates
- Authentication is handled via ASP.NET Core Identity
- SQLite is used for data persistence with Entity Framework Core

## License

[License information to be added]