# Website Analyzer
A website analysis toolkit designed for developers, SEO specialists, and website administrators. This solution combines broken link detection, cache warming, and detailed performance metrics to optimize website maintenance and user experience.

## Features

### Broken Link Detection
- Schedule scans scan your entire website for broken links and dead endpoints
- Detect both internal broken links
- Prevent SEO penalties from broken links
- Maintain user trust with perfectly functioning navigation

### Cache Warming
- Proactively warm your website's cache to ensure optimal performance
- Schedule automated cache warming sessions
- Reduce first-visit latency for your users
- Improve overall website response times
- Perfect for high-traffic websites and e-commerce platforms

### Website Development Insights
- Get technical metrics about your website
- Analyze page load times and performance bottlenecks
- Monitor resource usage and optimization opportunities
- Track changes in your website's technical health over time

## Getting Started

1. **Prerequisites**
    - Install .NET 9.0 SDK

2. **Setup**
   
## Add Env file

    POSTGRES_DB=
    POSTGRES_USER=
    POSTGRES_PASSWORD=
    CERT_PASSWORD=

3. **Running the Application**
   ```bash
   dotnet run
   ```
   The application will be available at `https://localhost:8080`

## Development

To generate database migrations, use the provided script:
```bash
./generate-migrations.sh
```

## License

GPL License - Copyright (c) 2025 Clypper Technology
