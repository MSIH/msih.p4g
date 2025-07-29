# Campaign Feature - Campaign Selection Interface

## Overview
The Campaign feature provides a streamlined interface component for campaign selection and management within the MSIH Platform for Good. This feature focuses on providing a user-friendly dropdown component that enables users to select campaigns across the platform, supporting both donation flows and administrative functions. While simple in structure, it serves as a critical integration point between users and the campaign system.

## Architecture

### Core Components
- **CampaignDropdown.razor**: Primary campaign selection component
- **Campaign Service Integration**: Direct integration with server-side CampaignService
- **Event-driven Architecture**: Callback-based selection handling for parent components
- **Data Loading**: Efficient campaign data loading and caching

### UI Structure
- **Dropdown Component**: Clean, accessible dropdown interface
- **Campaign Organization**: Alphabetically sorted campaign listing
- **Selection Management**: Two-way binding with parent components
- **Loading States**: Proper loading state management

## Key Features

### Campaign Selection Component
- **CampaignDropdown.razor**: Comprehensive campaign selection dropdown
- **Active Campaign Filtering**: Optional filtering to show only active campaigns
- **Alphabetical Sorting**: Campaigns automatically sorted by title for easy navigation
- **Two-way Data Binding**: Seamless integration with parent component state
- **Event Callbacks**: Event-driven architecture for selection change notifications

### Campaign Data Management
- **Server Integration**: Direct integration with ICampaignService for real-time data
- **Error Handling**: Graceful error handling with fallback to empty list
- **Data Caching**: Efficient data loading with component-level caching
- **Active/Inactive Filtering**: Configurable filtering based on campaign status

### Integration Capabilities
- **Parent Component Integration**: Easy integration with donation forms, admin pages, and other interfaces
- **Flexible Configuration**: Configurable behavior through component parameters
- **Selection Persistence**: Maintains selected campaign state across component lifecycle
- **Default Selection**: Support for pre-selected campaigns

## UI Components

### CampaignDropdown.razor Features

#### Core Dropdown Interface
- **Clean Design**: Bootstrap-styled select dropdown with consistent platform styling
- **Default Option**: Clear "-- Select a Campaign --" default option
- **Campaign Listing**: Comprehensive campaign list with title display
- **Selection Binding**: Two-way binding with `@bind` directive for seamless integration

#### Component Parameters
- **IsActive Parameter**: Controls whether to show only active campaigns or include inactive ones
- **SelectedCampaignId Parameter**: Current selected campaign ID with two-way binding
- **SelectedCampaignIdChanged Callback**: Event callback for selection change notifications

#### Data Loading and Display
- **Automatic Loading**: Campaigns loaded automatically on component initialization
- **Error Resilience**: Graceful handling of service errors with empty list fallback
- **Sorted Display**: Campaigns automatically sorted alphabetically by title
- **Loading States**: Proper component state management during data loading

### Component Implementation
```razor
<select class="form-select" @bind="SelectedCampaignId" @bind:after="OnSelectedCampaignIdChanged">
    <option value="">-- Select a Campaign --</option>
    @foreach (var campaign in campaigns.OrderBy(c => c.Title))
    {
        <option value="@campaign.Id">@campaign.Title</option>
    }
</select>

@code {
    [Parameter] public bool IsActive { get; set; } = true;
    [Parameter] public int? SelectedCampaignId { get; set; }
    [Parameter] public EventCallback<int?> SelectedCampaignIdChanged { get; set; }
    
    private List<Campaign> campaigns = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadCampaigns();
    }
    
    private async Task LoadCampaigns()
    {
        try
        {
            var allCampaigns = await CampaignService.GetAllAsync(includeInactive: !IsActive);
            campaigns = allCampaigns.ToList();
        }
        catch (Exception)
        {
            campaigns = new List<Campaign>();
        }
    }
    
    private async Task OnSelectedCampaignIdChanged()
    {
        await SelectedCampaignIdChanged.InvokeAsync(SelectedCampaignId);
    }
}
```

## Integration Points

### Server Service Integration
- **CampaignService**: Direct integration with ICampaignService for campaign data retrieval
- **Real-time Data**: Fresh campaign data loaded on component initialization
- **Service Methods**: Uses `GetAllAsync()` method with optional inactive inclusion
- **Error Handling**: Graceful degradation when service calls fail

### Parent Component Integration
- **Event-driven Communication**: Uses EventCallback pattern for selection notifications
- **Two-way Binding**: Seamless two-way data binding with parent component state
- **Parameter Passing**: Flexible parameter configuration for different use cases
- **State Management**: Maintains selection state independently

### Platform Integration
- **Donation Forms**: Integration with donation workflows for campaign attribution
- **Admin Interfaces**: Used in administrative forms for campaign assignment
- **Reporting Systems**: Campaign selection for filtering and reporting features
- **User Interfaces**: Campaign selection across various user-facing interfaces

## Usage Examples

### Basic Campaign Selection
```razor
@page "/donate"
@using msih.p4g.Client.Features.Campaign.Components

<div class="mb-3">
    <label class="form-label">Select Campaign</label>
    <CampaignDropdown @bind-SelectedCampaignId="selectedCampaignId" 
                     IsActive="true" />
</div>

@if (selectedCampaignId.HasValue)
{
    <div class="alert alert-info">
        Selected Campaign ID: @selectedCampaignId
    </div>
}

@code {
    private int? selectedCampaignId;
}
```

### Advanced Integration with Callbacks
```razor
@page "/admin/campaign-assignment"
@using msih.p4g.Client.Features.Campaign.Components

<div class="form-group">
    <label>Assign Campaign:</label>
    <CampaignDropdown SelectedCampaignId="@currentCampaignId"
                     SelectedCampaignIdChanged="@OnCampaignSelected"
                     IsActive="true" />
</div>

@code {
    private int? currentCampaignId;
    
    private async Task OnCampaignSelected(int? campaignId)
    {
        currentCampaignId = campaignId;
        
        // Perform additional actions when campaign is selected
        if (campaignId.HasValue)
        {
            // Load campaign details, update related data, etc.
            await LoadCampaignDetails(campaignId.Value);
        }
    }
    
    private async Task LoadCampaignDetails(int campaignId)
    {
        // Additional logic for campaign selection handling
    }
}
```

### Conditional Campaign Display
```razor
@using msih.p4g.Client.Features.Campaign.Components

<div class="row">
    <div class="col-md-6">
        <label>Active Campaigns:</label>
        <CampaignDropdown @bind-SelectedCampaignId="activeCampaignId"
                         IsActive="true" />
    </div>
    <div class="col-md-6">
        <label>All Campaigns (Including Inactive):</label>
        <CampaignDropdown @bind-SelectedCampaignId="allCampaignId"
                         IsActive="false" />
    </div>
</div>

@code {
    private int? activeCampaignId;
    private int? allCampaignId;
}
```

## User Workflows

### Campaign Selection Workflow
1. **Component Loading**: CampaignDropdown loads and displays campaign options
2. **Campaign Display**: User sees alphabetically sorted list of available campaigns
3. **Selection Process**: User selects desired campaign from dropdown
4. **Event Notification**: Parent component receives selection change notification
5. **State Update**: Selection state updated in parent component
6. **Dependent Actions**: Parent component can trigger additional actions based on selection

### Administrative Workflow
1. **Admin Interface**: Administrator accesses campaign management interface
2. **Campaign Assignment**: Uses CampaignDropdown to assign campaigns to various entities
3. **Selection Validation**: System validates campaign selection
4. **Association Creation**: Creates associations between campaigns and other entities
5. **Confirmation**: Administrator receives confirmation of successful assignment

### Donation Workflow Integration
1. **Donation Form**: User accesses donation form with campaign selection
2. **Campaign Choice**: User selects campaign using CampaignDropdown
3. **Campaign Attribution**: Donation system attributes donation to selected campaign
4. **Processing**: Donation processed with proper campaign tracking
5. **Reporting**: Campaign selection enables proper donation reporting and analytics

## Files

### Component Files
```
Client/Features/Campaign/
├── Components/
│   └── CampaignDropdown.razor
└── CLAUDE.md
```

### Related Server Components
```
Server/Features/CampaignService/ (campaign data and business logic)
Server/Features/CampaignService/Interfaces/ICampaignService.cs
Server/Features/CampaignService/Model/Campaign.cs
```

### Integration Points
```
Client/Features/Base/Payment/ (donation form integration)
Client/Features/Admin/ (administrative interface integration)
Client/Features/Donations/ (donation tracking integration)
```

## Performance Considerations

### Efficient Data Loading
- **Single Load**: Campaigns loaded once on component initialization
- **Minimal Network Requests**: Efficient service call pattern
- **Error Resilience**: Graceful handling of network issues
- **Caching**: Component-level caching of campaign data

### User Experience Optimization
- **Fast Rendering**: Lightweight component with minimal overhead
- **Responsive Design**: Bootstrap-based styling for consistent appearance
- **Accessibility**: Proper form labeling and accessibility attributes
- **Loading States**: Smooth user experience during data loading

### Integration Efficiency
- **Event-driven Architecture**: Efficient parent-child communication
- **Minimal Re-renders**: Optimized component lifecycle management
- **State Management**: Efficient state handling with two-way binding
- **Parameter Optimization**: Minimal parameter passing for maximum performance

## Security Considerations

### Data Access Security
- **Service Integration**: Secure integration with server-side services
- **Authorization**: Respects campaign visibility and access controls
- **Input Validation**: Proper validation of selected campaign IDs
- **Error Handling**: Secure error handling without information disclosure

### Component Security
- **XSS Protection**: Proper output encoding for campaign titles
- **Input Sanitization**: Secure handling of component parameters
- **State Protection**: Secure state management within component
- **Event Security**: Secure event callback handling

This feature provides a clean, efficient, and reusable campaign selection interface that integrates seamlessly across the MSIH Platform for Good, enabling consistent campaign selection experiences while maintaining performance and security standards.