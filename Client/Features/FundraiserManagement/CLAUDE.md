# FundraiserStatisticsService

## Overview
The FundraiserStatisticsService is currently an empty service file that represents a planned client-side service for handling fundraiser performance statistics, analytics, and dashboard data in the MSIH Platform for Good application. This service is intended to provide client-side caching, data aggregation, and presentation logic for fundraiser-specific statistics while leveraging server-side data services.

## Architecture

### Current Status
- **File Status**: Empty placeholder (0 bytes)
- **Creation Date**: June 28, 2024
- **Implementation Status**: Not yet implemented
- **Purpose**: Client-side fundraiser statistics management

### Planned Components
- **IFundraiserStatisticsService**: Interface defining statistics operations
- **FundraiserStatisticsService**: Implementation with caching and aggregation
- **Statistics Models**: DTOs for various fundraiser metrics
- **Cache Management**: Client-side caching for performance optimization

### Planned Dependencies
- Server-side FundraiserService for base data
- Server-side DonationService for donation statistics
- Server-side CampaignService for campaign performance
- Client-side caching mechanisms (MemoryCache or browser storage)
- Chart.js or similar for data visualization integration

## Key Features (Planned)

### Statistics Aggregation
- Real-time fundraiser performance metrics
- Campaign success rate calculations
- Donation volume and frequency analysis
- Conversion rate tracking from referrals to donations
- Time-based performance trends

### Client-side Caching
- Performance optimization for dashboard loading
- Reduced server requests for frequently accessed data
- Automatic cache invalidation and refresh
- Progressive data loading for large datasets

### Data Presentation
- Formatted statistics for dashboard display
- Chart-ready data structures
- Comparative analysis between time periods
- Goal progress calculations and visualization

### Real-time Updates
- SignalR integration for live statistics updates
- Automatic refresh when new donations occur
- Dashboard notifications for milestone achievements
- Live performance indicators

## Blazor Integration (Planned)

### Dashboard Component Integration
```csharp
@page "/fundraiser/dashboard"
@inject IFundraiserStatisticsService StatisticsService
@implements IDisposable

<div class="dashboard-grid">
    <div class="stat-card">
        <h4>Total Raised</h4>
        <h2>@statistics.TotalRaised.ToString("C")</h2>
    </div>
    
    <div class="stat-card">
        <h4>Active Campaigns</h4>
        <h2>@statistics.ActiveCampaigns</h2>
    </div>
    
    <div class="stat-card">
        <h4>Conversion Rate</h4>
        <h2>@statistics.ConversionRate.ToString("P")</h2>
    </div>
</div>

<div class="chart-container">
    <canvas id="performanceChart"></canvas>
</div>

@code {
    private FundraiserStatistics statistics = new();
    
    protected override async Task OnInitializedAsync()
    {
        // Subscribe to real-time updates
        StatisticsService.StatisticsUpdated += OnStatisticsUpdated;
        
        // Load initial statistics
        statistics = await StatisticsService.GetFundraiserStatisticsAsync(
            fundraiserId: CurrentUser.FundraiserId,
            includeTrends: true
        );
        
        // Initialize charts
        await InitializeChartsAsync();
    }
    
    private async void OnStatisticsUpdated(FundraiserStatistics updatedStats)
    {
        statistics = updatedStats;
        await InvokeAsync(() =>
        {
            StateHasChanged();
            UpdateCharts();
        });
    }
    
    public void Dispose()
    {
        StatisticsService.StatisticsUpdated -= OnStatisticsUpdated;
    }
}
```

### Performance Analytics Page
```csharp
@page "/fundraiser/analytics"
@inject IFundraiserStatisticsService StatisticsService

<div class="analytics-filters">
    <select @bind="selectedTimeframe" @onchange="RefreshAnalytics">
        <option value="7">Last 7 Days</option>
        <option value="30">Last 30 Days</option>
        <option value="90">Last 90 Days</option>
        <option value="365">Last Year</option>
    </select>
</div>

<div class="analytics-grid">
    @if (analyticsData != null)
    {
        <div class="metric-card">
            <h5>Donation Trends</h5>
            <canvas id="donationTrendChart"></canvas>
        </div>
        
        <div class="metric-card">
            <h5>Top Performing Campaigns</h5>
            @foreach (var campaign in analyticsData.TopCampaigns)
            {
                <div class="campaign-item">
                    <span>@campaign.Name</span>
                    <span>@campaign.TotalRaised.ToString("C")</span>
                </div>
            }
        </div>
    }
</div>

@code {
    private int selectedTimeframe = 30;
    private FundraiserAnalytics? analyticsData;
    
    private async Task RefreshAnalytics()
    {
        analyticsData = await StatisticsService.GetAnalyticsAsync(
            fundraiserId: CurrentUser.FundraiserId,
            daysPeriod: selectedTimeframe,
            includeComparisons: true
        );
        
        await UpdateAnalyticsCharts();
        StateHasChanged();
    }
}
```

## Usage (Planned)

### Basic Statistics Retrieval
```csharp
@inject IFundraiserStatisticsService StatisticsService

// Get current fundraiser statistics
var stats = await StatisticsService.GetFundraiserStatisticsAsync(
    fundraiserId: fundraiserId,
    includeTrends: true
);

Console.WriteLine($"Total Raised: {stats.TotalRaised:C}");
Console.WriteLine($"Active Campaigns: {stats.ActiveCampaigns}");
Console.WriteLine($"This Month: {stats.ThisMonth.TotalRaised:C}");

// Get campaign-specific statistics
var campaignStats = await StatisticsService.GetCampaignStatisticsAsync(
    campaignId: campaignId,
    includeHourlyBreakdown: true
);

// Get comparative statistics
var comparison = await StatisticsService.GetComparisonStatisticsAsync(
    fundraiserId: fundraiserId,
    currentPeriod: DateTime.Now.AddDays(-30),
    comparisonPeriod: DateTime.Now.AddDays(-60)
);
```

### Real-time Updates
```csharp
// Subscribe to statistics updates
StatisticsService.StatisticsUpdated += (updatedStats) =>
{
    // Update dashboard in real-time
    UpdateDashboardDisplays(updatedStats);
    NotifyUser($"New donation received! Total: {updatedStats.TotalRaised:C}");
};

// Manual refresh
await StatisticsService.RefreshStatisticsAsync(fundraiserId);

// Get cached statistics (fast)
var cachedStats = StatisticsService.GetCachedStatistics(fundraiserId);
```

### Data Export and Reporting
```csharp
// Export statistics for external analysis
var exportData = await StatisticsService.ExportStatisticsAsync(
    fundraiserId: fundraiserId,
    format: ExportFormat.CSV,
    dateRange: new DateRange(startDate, endDate)
);

// Generate performance report
var report = await StatisticsService.GeneratePerformanceReportAsync(
    fundraiserId: fundraiserId,
    includeCharts: true,
    includeComparisons: true
);
```

## Integration (Planned)

### Server-side Service Integration
- **FundraiserService**: Base fundraiser data and campaign information
- **DonationService**: Transaction data for revenue calculations
- **CampaignService**: Campaign performance and goal tracking
- **UserService**: Fundraiser profile and activity data

### Client-side Component Integration
- **Dashboard Components**: Real-time statistics display
- **Chart Components**: Data visualization for trends and analytics
- **Notification System**: Alerts for milestones and achievements
- **Export Components**: Data export and reporting functionality

### SignalR Integration
- **Real-time Notifications**: Live updates when donations occur
- **Dashboard Refresh**: Automatic statistics refresh
- **Milestone Alerts**: Notifications when goals are reached
- **Performance Updates**: Live tracking of campaign performance

### Caching Strategy
- **Memory Caching**: Fast access to frequently requested statistics
- **Browser Storage**: Persistence across browser sessions
- **Cache Invalidation**: Smart refresh when data changes
- **Performance Optimization**: Reduced server load and faster response times

## Implementation Roadmap

### Phase 1: Basic Statistics
- Implement IFundraiserStatisticsService interface
- Basic statistics retrieval (total raised, campaign count)
- Simple caching mechanism
- Integration with existing dashboard

### Phase 2: Advanced Analytics
- Time-based trend analysis
- Comparative statistics (period over period)
- Campaign performance breakdowns
- Goal progress tracking

### Phase 3: Real-time Features
- SignalR integration for live updates
- Real-time dashboard refresh
- Milestone notifications
- Live performance indicators

### Phase 4: Advanced Features
- Data export functionality
- Custom reporting
- Advanced visualization
- Performance optimization

## Files

```
Client/Features/FundraiserManagement/Services/
├── FundraiserStatisticsService.cs  # Currently empty - planned implementation
└── CLAUDE.md                       # This documentation file

Client/Features/FundraiserManagement/Interfaces/
└── IFundraiserStatisticsService.cs # Currently empty - planned interface
```

## Related Files (Current)

```
Client/Features/FundraiserManagement/
├── Component/
│   ├── AffiliateComissions.razor   # Uses commission data
│   └── AffiliateComissions.razor.cs
├── Pages/
│   ├── Affiliate.razor             # Potential statistics integration
│   └── MyFundraiser.razor          # Dashboard page for statistics

Server/Features/FundraiserService/
├── Services/FundraiserService.cs   # Server-side data source
└── CLAUDE.md                       # Server-side fundraiser documentation

Server/Features/DonationService/
├── Services/DonationService.cs     # Transaction data source
└── CLAUDE.md                       # Donation service documentation
```

## Development Notes

### Why This Service is Needed
While the current architecture allows direct server-side service injection, a client-side statistics service provides:

1. **Performance Optimization**: Client-side caching reduces server load
2. **User Experience**: Faster dashboard loading with cached data
3. **Real-time Features**: Centralized handling of live updates
4. **Data Aggregation**: Complex calculations performed once and cached
5. **Offline Capability**: Basic statistics available when server is unreachable

### Implementation Considerations
- Balance between client-side caching and data freshness
- Handle large datasets efficiently
- Provide fallback to server-side services when needed
- Implement proper error handling and retry logic
- Consider memory usage for extensive statistics caching

### Testing Strategy
- Unit tests for statistics calculations
- Integration tests with server services
- Performance tests for large datasets  
- Real-time update testing with SignalR
- Cache invalidation and refresh testing