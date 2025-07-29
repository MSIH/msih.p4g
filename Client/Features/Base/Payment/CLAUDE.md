# IPayoutService (Client Interface)

## Overview
The IPayoutService interface represents a planned client-side interface for payout operations in the MSIH Platform for Good application. Currently empty (0 bytes), this interface is intended to define client-side payout functionality that would complement the existing server-side PayoutService while providing client-specific features like caching, offline support, and optimized user experiences for payout-related operations.

## Architecture

### Current Status
- **File Status**: Empty placeholder (0 bytes)
- **Creation Date**: June 28, 2024
- **Implementation Status**: Not yet implemented
- **Purpose**: Client-side payout interface definition

### Planned Architecture
- **Interface Definition**: Contract for client-side payout operations
- **Implementation Service**: Concrete service implementing the interface
- **DTO Models**: Client-optimized data transfer objects
- **Caching Layer**: Client-side caching for payout data
- **Offline Support**: Queue operations when server is unavailable

### Design Considerations
- **Server Integration**: Work with existing server-side PayoutService
- **Performance Optimization**: Client-side caching and data aggregation
- **User Experience**: Optimized flows for payout requests and tracking
- **Error Handling**: Client-side validation and error recovery
- **Real-time Updates**: Live payout status notifications

## Key Features (Planned)

### Payout Request Management
- Client-side payout request creation and validation
- Form data persistence and auto-save functionality
- Offline request queuing with sync when online
- Request status tracking and notifications

### Performance Optimization
- Cached payout history for faster loading
- Aggregated payout statistics
- Progressive loading for large payout datasets
- Optimistic updates for better user experience

### User Experience Features
- Real-time payout status updates
- Interactive payout calculators
- Visual payout timeline and history
- Export functionality for tax reporting

### Validation and Error Handling
- Client-side validation before server submission
- Retry logic for failed payout operations
- User-friendly error messages and recovery options
- Form validation with immediate feedback

## Blazor Integration (Planned)

### Interface Definition
```csharp
namespace msih.p4g.Client.Features.Base.Payment.Interfaces
{
    /// <summary>
    /// Client-side interface for payout operations with caching and offline support
    /// </summary>
    public interface IPayoutService
    {
        // Payout Request Operations
        Task<PayoutRequestResult> CreatePayoutRequestAsync(PayoutRequestDto request);
        Task<PayoutRequestResult> UpdatePayoutRequestAsync(int requestId, PayoutRequestDto request);
        Task<bool> CancelPayoutRequestAsync(int requestId, string reason);
        
        // Payout Status and History
        Task<PayoutStatus> GetPayoutStatusAsync(int payoutId, bool useCache = true);
        Task<List<PayoutHistoryItem>> GetPayoutHistoryAsync(int fundraiserId, int pageSize = 50, bool useCache = true);
        Task<PayoutSummary> GetPayoutSummaryAsync(int fundraiserId, DateTime? fromDate = null, DateTime? toDate = null);
        
        // Client-side Caching
        Task RefreshPayoutCacheAsync(int fundraiserId);
        Task ClearPayoutCacheAsync();
        PayoutSummary? GetCachedPayoutSummary(int fundraiserId);
        
        // Real-time Updates
        event Action<PayoutStatusUpdate> PayoutStatusChanged;
        Task SubscribeToPayoutUpdatesAsync(int fundraiserId);
        Task UnsubscribeFromPayoutUpdatesAsync(int fundraiserId);
        
        // Validation and Calculations
        Task<PayoutValidationResult> ValidatePayoutRequestAsync(PayoutRequestDto request);
        Task<PayoutCalculation> CalculatePayoutAmountAsync(decimal grossAmount, PayoutType type);
        Task<List<PayoutIssue>> CheckPayoutEligibilityAsync(int fundraiserId);
        
        // Export and Reporting
        Task<byte[]> ExportPayoutHistoryAsync(int fundraiserId, DateTime fromDate, DateTime toDate, ExportFormat format);
        Task<PayoutReport> GeneratePayoutReportAsync(int fundraiserId, PayoutReportType reportType);
        
        // Offline Support
        Task<List<PayoutRequest>> GetPendingOfflineRequestsAsync();
        Task SyncOfflineRequestsAsync();
        Task QueueOfflinePayoutRequestAsync(PayoutRequestDto request);
    }
}
```

### Payout Request Component
```csharp
@page "/payout/request"
@inject IPayoutService PayoutService
@inject AuthService AuthService

<div class="payout-request-form">
    <h3>Request Payout</h3>
    
    @if (payoutSummary != null)
    {
        <div class="available-balance">
            <h5>Available Balance: @payoutSummary.AvailableBalance.ToString("C")</h5>
            <small>Minimum payout: @minimumPayout.ToString("C")</small>
        </div>
    }
    
    <EditForm Model="payoutRequest" OnValidSubmit="SubmitPayoutRequest">
        <DataAnnotationsValidator />
        
        <div class="mb-3">
            <label class="form-label">Payout Amount</label>
            <InputNumber @bind-Value="payoutRequest.Amount" class="form-control" 
                        @onchange="CalculatePayoutAmount" />
            <ValidationMessage For="@(() => payoutRequest.Amount)" />
            
            @if (payoutCalculation != null)
            {
                <div class="payout-breakdown mt-2">
                    <small>Gross Amount: @payoutCalculation.GrossAmount.ToString("C")</small><br>
                    <small>Processing Fee: @payoutCalculation.ProcessingFee.ToString("C")</small><br>
                    <strong>Net Amount: @payoutCalculation.NetAmount.ToString("C")</strong>
                </div>
            }
        </div>
        
        <div class="mb-3">
            <label class="form-label">Payout Method</label>
            <select @bind="payoutRequest.PayoutMethod" class="form-select">
                <option value="BankTransfer">Bank Transfer</option>
                <option value="PayPal">PayPal</option>
                <option value="Check">Check</option>
            </select>
        </div>
        
        <button type="submit" class="btn btn-primary" disabled="@isSubmitting">
            @if (isSubmitting)
            {
                <span class="spinner-border spinner-border-sm me-2"></span>
            }
            Request Payout
        </button>
    </EditForm>
</div>

@code {
    private PayoutRequestDto payoutRequest = new();
    private PayoutSummary? payoutSummary;
    private PayoutCalculation? payoutCalculation;
    private decimal minimumPayout = 25.00m;
    private bool isSubmitting = false;
    
    protected override async Task OnInitializedAsync()
    {
        var user = await AuthService.GetCurrentUserAsync();
        if (user?.Fundraiser != null)
        {
            // Load cached summary first for immediate display
            payoutSummary = PayoutService.GetCachedPayoutSummary(user.Fundraiser.Id);
            
            // Then refresh with server data
            payoutSummary = await PayoutService.GetPayoutSummaryAsync(user.Fundraiser.Id);
            StateHasChanged();
        }
        
        // Subscribe to real-time updates
        PayoutService.PayoutStatusChanged += OnPayoutStatusChanged;
    }
    
    private async Task CalculatePayoutAmount()
    {
        if (payoutRequest.Amount > 0)
        {
            payoutCalculation = await PayoutService.CalculatePayoutAmountAsync(
                payoutRequest.Amount, 
                PayoutType.Standard
            );
            StateHasChanged();
        }
    }
    
    private async Task SubmitPayoutRequest()
    {
        isSubmitting = true;
        
        try
        {
            // Client-side validation first
            var validation = await PayoutService.ValidatePayoutRequestAsync(payoutRequest);
            if (!validation.IsValid)
            {
                ShowValidationErrors(validation.Errors);
                return;
            }
            
            // Submit request
            var result = await PayoutService.CreatePayoutRequestAsync(payoutRequest);
            if (result.Success)
            {
                ShowSuccessMessage("Payout request submitted successfully!");
                NavigationManager.NavigateTo("/payout/history");
            }
            else
            {
                ShowErrorMessage(result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            // Queue for offline processing if server unavailable
            await PayoutService.QueueOfflinePayoutRequestAsync(payoutRequest);
            ShowInfoMessage("Request queued for processing when connection is restored.");
        }
        finally
        {
            isSubmitting = false;
        }
    }
    
    private void OnPayoutStatusChanged(PayoutStatusUpdate update)
    {
        InvokeAsync(() =>
        {
            // Update UI with real-time status changes
            ShowNotification($"Payout status updated: {update.NewStatus}");
            StateHasChanged();
        });
    }
    
    public void Dispose()
    {
        PayoutService.PayoutStatusChanged -= OnPayoutStatusChanged;
    }
}
```

### Payout History Component
```csharp
@page "/payout/history"
@inject IPayoutService PayoutService
@inject AuthService AuthService

<div class="payout-history">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h3>Payout History</h3>
        <button class="btn btn-outline-primary" @onclick="ExportHistory">
            Export History
        </button>
    </div>
    
    @if (payoutHistory.Any())
    {
        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Amount</th>
                        <th>Method</th>
                        <th>Status</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var payout in payoutHistory)
                    {
                        <tr class="@GetRowClass(payout.Status)">
                            <td>@payout.RequestDate.ToString("MMM dd, yyyy")</td>
                            <td>@payout.Amount.ToString("C")</td>
                            <td>@payout.PayoutMethod</td>
                            <td>
                                <span class="badge @GetStatusBadgeClass(payout.Status)">
                                    @payout.Status
                                </span>
                            </td>
                            <td>
                                <button class="btn btn-sm btn-outline-info" 
                                       @onclick="() => ViewPayoutDetails(payout.Id)">
                                    Details
                                </button>
                                @if (payout.Status == "Pending")
                                {
                                    <button class="btn btn-sm btn-outline-danger" 
                                           @onclick="() => CancelPayout(payout.Id)">
                                        Cancel
                                    </button>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
    else
    {
        <div class="text-center py-5">
            <p>No payout history found.</p>
            <a href="/payout/request" class="btn btn-primary">Request Your First Payout</a>
        </div>
    }
</div>

@code {
    private List<PayoutHistoryItem> payoutHistory = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadPayoutHistory();
        
        // Subscribe to real-time updates
        await PayoutService.SubscribeToPayoutUpdatesAsync(CurrentUser.FundraiserId);
        PayoutService.PayoutStatusChanged += OnPayoutStatusChanged;
    }
    
    private async Task LoadPayoutHistory()
    {
        var user = await AuthService.GetCurrentUserAsync();
        if (user?.Fundraiser != null)
        {
            // Use cached data for immediate display
            payoutHistory = await PayoutService.GetPayoutHistoryAsync(
                user.Fundraiser.Id, 
                pageSize: 100, 
                useCache: true
            );
        }
    }
    
    private async Task ExportHistory()
    {
        var user = await AuthService.GetCurrentUserAsync();
        if (user?.Fundraiser != null)
        {
            var exportData = await PayoutService.ExportPayoutHistoryAsync(
                user.Fundraiser.Id,
                DateTime.Now.AddYears(-1),
                DateTime.Now,
                ExportFormat.CSV
            );
            
            await DownloadFile(exportData, "payout-history.csv", "text/csv");
        }
    }
    
    private void OnPayoutStatusChanged(PayoutStatusUpdate update)
    {
        InvokeAsync(async () =>
        {
            // Refresh the specific payout item
            var item = payoutHistory.FirstOrDefault(p => p.Id == update.PayoutId);
            if (item != null)
            {
                item.Status = update.NewStatus;
                StateHasChanged();
            }
        });
    }
}
```

## Usage (Planned)

### Basic Payout Operations
```csharp
@inject IPayoutService PayoutService

// Create a payout request
var request = new PayoutRequestDto
{
    Amount = 100.00m,
    PayoutMethod = "BankTransfer",
    FundraiserId = fundraiserId
};

var result = await PayoutService.CreatePayoutRequestAsync(request);
if (result.Success)
{
    Console.WriteLine($"Payout request created: {result.PayoutId}");
}

// Get payout status with caching
var status = await PayoutService.GetPayoutStatusAsync(payoutId, useCache: true);

// Get payout history
var history = await PayoutService.GetPayoutHistoryAsync(fundraiserId, pageSize: 50);
```

### Client-side Validation
```csharp
// Validate before submission
var validation = await PayoutService.ValidatePayoutRequestAsync(request);
if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        ShowError(error.Message);
    }
    return;
}

// Calculate payout amounts
var calculation = await PayoutService.CalculatePayoutAmountAsync(grossAmount, PayoutType.Express);
Console.WriteLine($"Net amount: {calculation.NetAmount:C}");
Console.WriteLine($"Processing fee: {calculation.ProcessingFee:C}");
```

### Real-time Updates
```csharp
// Subscribe to payout status updates
await PayoutService.SubscribeToPayoutUpdatesAsync(fundraiserId);
PayoutService.PayoutStatusChanged += (update) =>
{
    Console.WriteLine($"Payout {update.PayoutId} status changed to {update.NewStatus}");
    UpdateUI(update);
};

// Unsubscribe when component is disposed
await PayoutService.UnsubscribeFromPayoutUpdatesAsync(fundraiserId);
```

### Offline Support
```csharp
// Queue payout request for offline processing
try
{
    await PayoutService.CreatePayoutRequestAsync(request);
}
catch (HttpRequestException)
{
    // Queue for later processing
    await PayoutService.QueueOfflinePayoutRequestAsync(request);
    ShowMessage("Request queued for processing when connection is restored");
}

// Sync offline requests when connection is restored
await PayoutService.SyncOfflineRequestsAsync();
```

## Integration (Planned)

### Server-side PayoutService Integration
- **Data Synchronization**: Keep client cache in sync with server data
- **Request Processing**: Forward client requests to server-side processing
- **Status Updates**: Receive server-side status changes via SignalR
- **Validation**: Leverage server-side business rules and validations

### Client-side Component Integration
- **Dashboard Integration**: Display payout summaries and quick actions
- **Form Components**: Reusable payout request and configuration forms
- **Notification System**: Real-time alerts for payout status changes
- **Export Components**: Data export functionality for reporting

### Authentication Integration
- **User Context**: Automatically determine fundraiser ID from authenticated user
- **Authorization**: Ensure users can only access their own payout data
- **Session Management**: Handle authentication state changes gracefully

### Caching Strategy
- **Browser Storage**: Persist payout data across browser sessions  
- **Memory Cache**: Fast access to frequently requested data
- **Cache Invalidation**: Smart refresh when server data changes
- **Offline Queue**: Store requests when server is unavailable

## Implementation Considerations

### Why Client-side Interface is Beneficial
1. **Performance**: Client-side caching reduces server load and improves response times
2. **User Experience**: Offline support and optimistic updates for better UX  
3. **Validation**: Immediate client-side validation prevents unnecessary server requests
4. **Real-time Features**: Centralized management of live payout status updates
5. **Data Aggregation**: Complex calculations and summaries performed client-side

### Architecture Decisions
- **Hybrid Approach**: Combine client-side optimization with server-side authority
- **Cache-First**: Use cached data by default with background refresh
- **Offline Queue**: Queue operations when server is unavailable
- **Event-Driven**: Use events for loose coupling between components

### Security Considerations
- **Server Authority**: All business logic and final decisions happen server-side
- **Client Validation**: UI validation only, never trust client-side data
- **Authentication**: Ensure proper user context for all operations
- **Data Exposure**: Only cache non-sensitive aggregated data client-side

## Files

```
Client/Features/Base/Payment/Interfaces/
├── IPayoutService.cs           # Currently empty - planned interface definition
└── CLAUDE.md                   # This documentation file
```

## Related Files (Planned)

```
Client/Features/Base/Payment/Services/
└── PayoutService.cs            # Planned implementation of IPayoutService

Client/Features/Base/Payment/Models/
├── PayoutRequestDto.cs         # Client-optimized payout request model
├── PayoutSummary.cs           # Aggregated payout summary data
├── PayoutCalculation.cs       # Payout amount calculation results
└── PayoutStatusUpdate.cs      # Real-time status update model

Server/Features/Base/PayoutService/
├── Services/PayoutService.cs   # Server-side payout processing
├── Models/PayoutRequest.cs     # Server-side payout models
└── CLAUDE.md                   # Server-side payout documentation
```

## Development Roadmap

### Phase 1: Basic Interface
- Define IPayoutService interface with core methods
- Implement basic payout request and status operations
- Add simple client-side validation

### Phase 2: Caching and Performance
- Implement client-side caching mechanisms
- Add payout history caching and pagination
- Optimize for large datasets

### Phase 3: Real-time Features
- SignalR integration for live status updates
- Real-time dashboard updates
- Push notifications for status changes

### Phase 4: Advanced Features
- Offline support with request queuing
- Advanced reporting and export functionality
- Performance analytics and optimization

### Phase 5: Integration Testing
- End-to-end testing with server services
- Performance testing with large datasets
- Real-time update testing
- Offline/online synchronization testing