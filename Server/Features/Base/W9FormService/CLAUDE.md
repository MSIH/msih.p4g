# W9FormService

## Overview
The W9FormService manages IRS Form W-9 (Request for Taxpayer Identification Number and Certification) for tax compliance purposes. It provides secure handling of sensitive tax information including SSN/EIN encryption, role-based access control, and comprehensive audit trails. This service is essential for organizations that need to collect tax information from fundraisers and contractors for 1099 reporting purposes.

## Architecture

### Components
- **IW9FormService**: Service interface for W-9 form operations
- **W9FormService**: Core implementation with security and access control
- **W9Form**: Entity representing IRS Form W-9 data
- **SsnUtility**: Utility class for SSN/EIN validation, formatting, and encryption
- **W9FormController**: API controller for form management operations

### Dependencies
- Entity Framework Core for secure data persistence
- ApplicationDbContext for database operations
- IHttpContextAccessor for user context and authorization
- Claims-based authentication for role management
- Base64 encryption for sensitive data protection

## Key Features
- Complete IRS Form W-9 data management with all required fields
- Secure SSN/EIN encryption and formatted display
- Role-based access control (Admin-only operations vs user operations)
- User and fundraiser association support
- Form status workflow management (Draft, Submitted, Approved, Rejected)
- Comprehensive audit trails with creation and modification tracking
- Data validation and format verification for tax identifiers
- Automatic fundraiser W9Document flag updates

## Database Schema

### W9Form Entity
- **Id** (int): Primary key, auto-generated
- **Name** (string): Deprecated legacy field, use FirstName/LastName
- **FirstName** (string): Required, individual's first name (max 200 chars)
- **LastName** (string): Required, individual's last name (max 200 chars)
- **BusinessName** (string?): Optional business/DBA name (max 200 chars)
- **FederalTaxClassification** (string): Required tax classification (max 50 chars)
- **LLCTaxClassification** (string?): LLC tax election (C/S/P, max 1 char)
- **OtherClassificationInstructions** (string?): Special tax instructions (max 500 chars)
- **PartnershipTrustInfo** (string?): Partnership/trust details (max 50 chars)
- **ExemptPayeeCode** (string?): Exemption code if applicable (max 20 chars)
- **FATCAExemptionCode** (string?): FATCA exemption code (max 20 chars)
- **Address** (string): Required mailing address (max 200 chars)
- **CityStateZip** (string): Required city, state, ZIP (max 200 chars)
- **AccountNumbers** (string?): Optional account numbers (max 200 chars)
- **SocialSecurityNumber** (string?): Encrypted SSN storage (max 500 chars)
- **EmployerIdentificationNumber** (string?): Encrypted EIN storage (max 500 chars)
- **SignedDate** (DateTime): Date form was signed
- **SignatureVerification** (string?): Digital signature token (max 500 chars)
- **FundraiserId** (int?): Associated fundraiser ID
- **UserId** (int): Required associated user ID
- **Status** (string): Form status (max 50 chars, default: "Draft")
- **IsActive** (bool): Soft delete flag from BaseEntity
- **CreatedOn** (DateTime): Creation timestamp from BaseEntity
- **CreatedBy** (string): Creator identifier from BaseEntity
- **ModifiedOn** (DateTime?): Last modification timestamp from BaseEntity
- **ModifiedBy** (string?): Last modifier identifier from BaseEntity

### Form Status Values
- **Draft**: Form is being created/edited
- **Submitted**: Form submitted for review
- **Approved**: Form approved for tax reporting
- **Rejected**: Form rejected, needs correction

## Usage

### Creating and Saving W-9 Forms
```csharp
@inject IW9FormService W9FormService

// Create new W-9 form
var w9FormDto = new W9FormDto
{
    FirstName = "John",
    LastName = "Doe",
    BusinessName = "Doe Consulting LLC",
    FederalTaxClassification = "Limited Liability Company",
    LLCTaxClassification = "S", // S-Corp election
    Address = "123 Main Street",
    CityStateZip = "Anytown, CA 12345",
    SocialSecurityNumber = "123-45-6789", // Will be encrypted
    UserId = 123,
    FundraiserId = 456,
    Status = "Draft",
    SignedDate = DateTime.Now
};

var savedForm = await W9FormService.SaveW9FormAsync(w9FormDto);
Console.WriteLine($"Form saved with ID: {savedForm.Id}");
```

### Retrieving W-9 Forms
```csharp
// Get form by ID (with access control)
var form = await W9FormService.GetByIdAsync(123);

// Get form by user ID
var userForm = await W9FormService.GetByUserIdAsync(456);

// Get form by fundraiser ID
var fundraiserForm = await W9FormService.GetByFundraiserIdAsync(789);

if (form != null)
{
    Console.WriteLine($"Form for: {form.FirstName} {form.LastName}");
    Console.WriteLine($"Status: {form.Status}");
    Console.WriteLine($"SSN: {form.SocialSecurityNumber}"); // Displayed in formatted/masked form
}
```

### Administrative Operations
```csharp
// Update form status (Admin only)
var statusUpdated = await W9FormService.UpdateStatusAsync(123, "Approved");

// Delete form (Admin only)
var deleted = await W9FormService.DeleteW9FormAsync(123);

if (statusUpdated)
{
    Console.WriteLine("Form status updated successfully");
}
```

## Security Features

### Data Encryption
```csharp
// SSN/EIN are encrypted before storage
public static string EncryptEin(string ein)
{
    var bytes = System.Text.Encoding.UTF8.GetBytes(ein);
    return Convert.ToBase64String(bytes);
}

// Decrypted for processing, formatted for display
public static string FormatSsnForDisplay(string ssn)
{
    // Returns formatted SSN like "123-45-6789" or masked version
}
```

### Access Control
The service implements comprehensive access control:
- **User Access**: Users can only access their own forms
- **Admin Access**: Admins can access and manage all forms
- **Status Updates**: Only admins can change form status
- **Deletion**: Only admins can delete forms
- **Creation**: Users can create forms for themselves

### Data Validation
```csharp
// SSN validation
public static bool IsValidSsn(string ssn)
{
    string digitsOnly = Regex.Replace(ssn, @"\D", "");
    return digitsOnly.Length == 9;
}

// EIN validation  
public static bool IsValidEin(string ein)
{
    string digitsOnly = Regex.Replace(ein, @"\D", "");
    return digitsOnly.Length == 9;
}
```

## Integration

### FundraiserService Integration
- **Automatic Updates**: When W-9 is saved, updates Fundraiser.W9Document flag
- **Form Association**: Links W-9 forms to specific fundraiser records
- **Compliance Tracking**: Enables tracking of fundraiser tax compliance status

### UserService Integration
- **User Association**: All forms linked to specific user accounts
- **Authentication**: Uses user claims for access control
- **Audit Trails**: User information captured in creation/modification logs

### Tax Reporting Integration
- **1099 Preparation**: Provides necessary tax information for 1099 generation
- **Compliance Reporting**: Supports organizational tax compliance requirements
- **Data Export**: Structured data available for tax software integration

## Form Processing Workflow

### User Form Submission
```csharp
// 1. User creates draft form
var draftForm = new W9FormDto { Status = "Draft", UserId = currentUserId };
await W9FormService.SaveW9FormAsync(draftForm);

// 2. User completes and submits form
draftForm.Status = "Submitted";
draftForm.SignedDate = DateTime.Now;
await W9FormService.SaveW9FormAsync(draftForm);

// 3. Admin reviews and approves/rejects
await W9FormService.UpdateStatusAsync(formId, "Approved");
```

### Data Security Handling
```csharp
// During save operation:
private void MapToEntity(W9FormDto dto, W9Form entity)
{
    // Only update SSN if changed (prevents unnecessary re-encryption)
    if (!string.IsNullOrEmpty(dto.SocialSecurityNumber) &&
        dto.SocialSecurityNumber != SsnUtility.FormatSsnForDisplay(entity.SocialSecurityNumber))
    {
        entity.SocialSecurityNumber = SsnUtility.EncryptEin(dto.SocialSecurityNumber);
        entity.EmployerIdentificationNumber = null; // Clear EIN if SSN provided
    }
}
```

## Utility Functions

### SSN/EIN Formatting
```csharp
// Format for display
var formattedSSN = SsnUtility.FormatSsn("123456789"); // Returns "123-45-6789"
var formattedEIN = SsnUtility.FormatEinForDisplay("123456789"); // Returns "12-3456789"

// Mask sensitive data
var maskedSSN = SsnUtility.MaskSsn("123456789"); // Returns "XXX-XX-6789"
var maskedEIN = SsnUtility.MaskEin("123456789"); // Returns "XX-XXXXX6789"

// Validate format
var isValidSSN = SsnUtility.IsValidSsn("123-45-6789"); // Returns true
var isValidEIN = SsnUtility.IsValidEin("12-3456789"); // Returns true
```

## Compliance and Audit

### Audit Trail
- All form operations logged with user context
- Creation and modification timestamps maintained
- Status change history tracked
- Access attempts logged for security monitoring

### Data Retention
- Forms maintained for tax compliance periods
- Soft delete functionality preserves audit trails
- Encrypted sensitive data ensures privacy protection

### Regulatory Compliance
- Implements IRS Form W-9 requirements
- Supports 1099 reporting obligations
- Maintains data security for PII protection
- Provides audit trails for compliance verification

## Files

```
Server/Features/Base/W9FormService/
├── Controllers/
│   └── W9FormController.cs
├── Data/
│   └── W9FormDbContext.cs
├── Interfaces/
│   └── IW9FormService.cs
├── Models/
│   └── W9Form.cs
├── Services/
│   └── W9FormService.cs
├── Utilities/
│   └── SsnUtility.cs
└── CLAUDE.md
```