# Donations Feature - Complete Donation Management Interface

## Overview
The Donations feature provides a comprehensive interface for users to view, manage, and edit their donation history within the MSIH Platform for Good. This feature includes detailed donation tracking, recurring donation management, donation editing capabilities, and comprehensive analytics. It serves as the primary interface for donors to interact with their giving history and manage ongoing donations.

## Architecture

### Core Components
- **MyDonations Page**: Main donation management interface
- **DonationHistoryComponent**: Comprehensive donation listing and analytics component
- **EditDonationModal**: Modal interface for donation editing and management
- **Search and Filtering**: Advanced donation search and filtering capabilities

### UI Structure
- **Page Layout**: Main donation management page with integrated components
- **Component Architecture**: Modular components for reusable functionality
- **Modal Integration**: Modal-based editing interface for donation modifications
- **Responsive Design**: Mobile-optimized interface for donation management

## Key Features

### Comprehensive Donation History
- **DonationHistoryComponent**: Advanced donation history interface with:
  - Real-time donation loading and display
  - Comprehensive search across donation data
  - Advanced filtering by donation type (one-time, monthly, annual, recurring)
  - Pagination for efficient data management
  - Summary statistics and analytics display

### Donation Analytics and Statistics
- **Summary Cards**: Key donation metrics including:
  - Total donation count across all donations
  - Total donated amount with financial summaries
  - Monthly donation count and tracking
  - Annual donation count and management
- **Performance Tracking**: Donation trends and giving patterns
- **Campaign Attribution**: Donation tracking by campaign association

### Recurring Donation Management
- **EditDonationModal**: Comprehensive donation editing interface featuring:
  - Monthly donation amount modification
  - Annual donation amount adjustment
  - Donation status management (active/paused)
  - Recurring donation cancellation with confirmation
  - Read-only view for one-time donations
- **Status Control**: Active/inactive status management for recurring donations
- **Cancellation Workflow**: Secure cancellation process with confirmation dialogs

### Advanced Search and Filtering
- **Real-time Search**: Live search across donation messages, amounts, and campaign information
- **Type-based Filtering**: Filter donations by:
  - All donations
  - One-time donations only
  - Monthly recurring donations
  - Annual recurring donations
  - All recurring donations (monthly + annual)
- **Campaign Filtering**: Filter donations by associated campaigns
- **Date Range Filtering**: Time-based donation filtering capabilities

### Campaign Integration
- **Campaign Association**: Donations linked to specific campaigns with visual indicators
- **Campaign Name Display**: Clear campaign attribution with badge display
- **General Donations**: Proper handling of donations not associated with specific campaigns
- **Campaign Performance**: Campaign-specific donation analytics and tracking

## UI Components

### MyDonations.razor Features

#### Main Page Interface
- **Page Title**: Clear "My Donations" page title and branding
- **Integrated Components**: Seamless integration of DonationHistoryComponent
- **Modal Management**: EditDonationModal integration with state management
- **Action Handlers**: Comprehensive donation action handling (view, edit, cancel)

#### Donation Management Actions
- **View Donation**: Detailed donation information display
- **Edit Donation**: Modification interface for recurring donations
- **Cancel Donation**: Secure cancellation workflow with confirmation
- **Status Updates**: Real-time status updates and feedback

### DonationHistoryComponent.razor Features

#### Comprehensive Data Display
- **Donation Table**: Detailed donation information including:
  - Donation date with formatted display
  - Donation amount with currency formatting
  - Donation type with visual badges (One-time, Monthly, Annual)
  - Campaign association with badge display
  - Donation status indicators (Active, Paused, Completed)
  - Action buttons for donation management

#### Summary Statistics Dashboard
- **Summary Cards**: Visual statistics display featuring:
  - Total donation count with visual emphasis
  - Total donated amount with financial formatting
  - Monthly donation count tracking
  - Annual donation count monitoring
- **Performance Metrics**: Donation analytics and trend tracking
- **Visual Indicators**: Color-coded status and type indicators

#### Search and Filtering Interface
- **Search Input**: Real-time search with placeholder guidance
- **Filter Dropdown**: Comprehensive filtering options:
  - All Donations (default view)
  - One-time donations only
  - Monthly recurring donations
  - Annual recurring donations
  - All Recurring donations combined
- **Search Integration**: Combined search and filter functionality
- **Real-time Updates**: Live filtering and search results

#### Advanced Features
- **Pagination**: Efficient pagination with configurable page sizes
- **Loading States**: Professional loading indicators during data retrieval
- **Empty States**: User-friendly empty state messages for no donations
- **Error Handling**: Graceful error handling with user feedback
- **Responsive Design**: Mobile-optimized table and card layouts

### EditDonationModal.razor Features

#### Modal Interface Design
- **Dynamic Titles**: Context-aware modal titles based on donation type
- **Form Validation**: Comprehensive form validation with error display
- **Status Indicators**: Clear donation type and status display
- **User Guidance**: Helpful text and instructions for donation editing

#### Donation Editing Capabilities
- **Amount Display**: Read-only donation amount display (currently not editable)
- **Status Management**: 
  - Monthly donation activation/deactivation toggles
  - Annual donation activation/deactivation controls
  - Clear labeling and user guidance for status changes
- **Readonly Fields**: Appropriate readonly fields for donation date, type, and message
- **Campaign Display**: Campaign information display when applicable

#### Transaction Fee Information
- **Fee Display**: Comprehensive transaction fee information including:
  - Transaction fee amount with clear formatting
  - Total amount charged to donor
  - Net donation amount received by organization
- **Fee Handling**: Different display based on fee payment method
- **Transparency**: Clear financial breakdown for donor understanding

#### Action Management
- **Save Changes**: Update recurring donation settings with validation
- **Close Modal**: Cancel editing without changes
- **Loading States**: Processing indicators during save operations
- **Error Handling**: User-friendly error messages and feedback

## Integration Points

### Server Service Integration
- **DonationService**: Comprehensive integration with donation management services
- **Campaign Service**: Campaign data integration for donation attribution
- **Authentication**: User authentication and authorization for donation access
- **Real-time Data**: Live donation data loading and updates

### Authentication and Authorization
- **AuthService**: User authentication for donation access control
- **Email-based Access**: Donation access based on user email authentication
- **Secure Operations**: Protected donation modification operations
- **Session Management**: Secure session handling for donation management

### Campaign System Integration
- **Campaign Attribution**: Donations linked to specific campaigns
- **Campaign Name Resolution**: Campaign ID to name resolution for display
- **Campaign Analytics**: Campaign-specific donation tracking and analytics
- **Cross-service Communication**: Seamless integration between donation and campaign systems

## User Workflows

### Donation History Viewing Workflow
1. **Page Access**: User navigates to My Donations page
2. **Data Loading**: System loads user's complete donation history
3. **Summary Display**: Dashboard shows donation statistics and summaries
4. **History Browse**: User browses detailed donation history with pagination
5. **Search/Filter**: User applies search terms or filters to find specific donations
6. **Detail Review**: User reviews individual donation details and information

### Recurring Donation Management Workflow
1. **Donation Selection**: User selects recurring donation for modification
2. **Edit Modal**: EditDonationModal opens with current donation information
3. **Status Review**: User reviews current donation status and settings
4. **Modification**: User modifies donation status (active/paused)
5. **Confirmation**: User confirms changes with save action
6. **Update Processing**: System processes donation updates
7. **Feedback**: User receives confirmation of successful changes

### Donation Cancellation Workflow
1. **Cancellation Initiation**: User selects cancel action for recurring donation
2. **Confirmation Dialog**: System displays cancellation confirmation dialog
3. **Cancellation Confirmation**: User confirms cancellation intent
4. **Processing**: System processes donation cancellation
5. **Status Update**: Donation status updated to cancelled
6. **Confirmation**: User receives cancellation confirmation
7. **History Update**: Donation history reflects cancellation status

### Search and Filtering Workflow
1. **Search Entry**: User enters search terms in search input
2. **Real-time Search**: System performs live search across donation data
3. **Filter Selection**: User selects donation type filter
4. **Combined Filtering**: System applies both search and filter criteria
5. **Results Display**: Filtered donation results displayed in table
6. **Refinement**: User can further refine search and filter criteria

## Usage Examples

### Basic Donation History Display
```razor
@page "/donations"
@using msih.p4g.Client.Features.Donations.Components

<PageTitle>My Donations</PageTitle>

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <h2 class="mb-0">My Donations</h2>
            
            <!-- Donation History Component -->
            <DonationHistoryComponent ShowSummary="true" 
                                    ShowActions="true" 
                                    OnDonationClick="ViewDonation" />
        </div>
    </div>
</div>

<!-- Edit Donation Modal -->
<EditDonationModal Donation="selectedDonation"
                   IsVisible="showEditModal"
                   OnClose="CloseEditModal"
                   OnSave="SaveDonation"
                   OnCancel="CancelDonation" />
```

### Donation Management Actions
```csharp
private async Task SaveDonation(DonationDto donation)
{
    try
    {
        if (AuthService.IsAuthenticated && !string.IsNullOrEmpty(AuthService.CurrentUser?.Email))
        {
            var success = await DonationService.UpdateRecurringDonationAsync(
                AuthService.CurrentUser.Email,
                donation.Id,
                donation.DonationAmount,
                donation.IsActive);

            if (success)
            {
                CloseEditModal();
                await JSRuntime.InvokeVoidAsync("alert", "Donation updated successfully!");
                StateHasChanged();
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", "Failed to update donation. Please try again.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving donation: {ex.Message}");
        await JSRuntime.InvokeVoidAsync("alert", "An error occurred while updating the donation.");
    }
}

private async Task CancelDonation(int donationId)
{
    try
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
            "Are you sure you want to cancel this recurring donation? This action cannot be undone.");

        if (confirmed)
        {
            if (AuthService.IsAuthenticated && !string.IsNullOrEmpty(AuthService.CurrentUser?.Email))
            {
                var success = await DonationService.CancelRecurringDonationAsync(AuthService.CurrentUser.Email, donationId);

                if (success)
                {
                    CloseEditModal();
                    await JSRuntime.InvokeVoidAsync("alert", "Recurring donation cancelled successfully!");
                    StateHasChanged();
                }
                else
                {
                    await JSRuntime.InvokeVoidAsync("alert", "Failed to cancel donation. Please try again.");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error canceling donation: {ex.Message}");
        await JSRuntime.InvokeVoidAsync("alert", "An error occurred while canceling the donation.");
    }
}
```

### Advanced Search and Filtering
```razor
<!-- Search and Filter Section in DonationHistoryComponent -->
<div class="row mb-4">
    <div class="col-md-6">
        <div class="input-group">
            <input type="text" class="form-control" placeholder="Search donations..."
                   @bind="searchTerm" @bind:event="oninput" @onkeypress="OnSearchKeyPress" />
            <button class="btn btn-outline-secondary" type="button" @onclick="SearchDonations">
                <span class="oi oi-magnifying-glass"></span>
            </button>
        </div>
    </div>
    <div class="col-md-6">
        <select class="form-select" @bind="filterType" @bind:after="FilterChanged">
            <option value="all">All Donations</option>
            <option value="onetime">One-time</option>
            <option value="monthly">Monthly</option>
            <option value="annual">Annual</option>
            <option value="recurring">All Recurring</option>
        </select>
    </div>
</div>
```

## Files

### Client Components
```
Client/Features/Donations/
├── Pages/
│   └── MyDonations.razor
├── Components/
│   ├── DonationHistoryComponent.razor
│   └── EditDonationModal.razor
└── CLAUDE.md
```

### Related Server Components
```
Server/Features/DonationService/ (donation management and processing)
Server/Features/CampaignService/ (campaign integration)
Shared/Dtos/DonationDto.cs (data transfer objects)
```

### Integration Points
```
Client/Features/Authentication/Services/AuthService.cs
Client/Common/Components/PaginationComponent.razor
Server/Features/DonationService/Interfaces/IDonationService.cs
```

## Security and Data Protection

### Access Control Security
- **User Authentication**: Donations accessible only to authenticated users
- **Email-based Authorization**: Donation access based on authenticated user email
- **Secure Operations**: Protected donation modification and cancellation operations
- **Session Validation**: Continuous session validation for secure access

### Data Security
- **Encrypted Transmission**: Secure data transmission for donation information
- **Input Validation**: Comprehensive validation for donation modifications
- **Error Handling**: Secure error handling without sensitive information disclosure
- **Audit Trail**: Comprehensive logging of donation modifications and cancellations

### Privacy Protection
- **User Data Isolation**: Users can only access their own donation data
- **Secure Display**: Sensitive information properly protected in display
- **Privacy Compliance**: GDPR and privacy regulation compliance
- **Data Minimization**: Only necessary donation data displayed and processed

## Performance Optimization

### Efficient Data Management
- **Pagination**: Efficient pagination for large donation datasets
- **Lazy Loading**: On-demand loading of donation data
- **Search Optimization**: Optimized search and filtering operations
- **Caching Strategies**: Strategic caching of frequently accessed donation data

### User Experience Optimization
- **Real-time Updates**: Live updates for donation status changes
- **Responsive Design**: Mobile-optimized interfaces for donation management
- **Fast Loading**: Optimized component loading and rendering
- **Progressive Enhancement**: Enhanced experience on capable devices

### Component Efficiency
- **Modular Design**: Efficient component architecture with clear separation of concerns
- **State Management**: Optimized state management across components
- **Event Handling**: Efficient event handling and callback management
- **Memory Management**: Proper component lifecycle management to prevent memory leaks

This feature provides a comprehensive donation management interface that enables users to effectively track, manage, and modify their donations while maintaining security, performance, and user experience standards.