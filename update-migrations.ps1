# Update migrations for all DbContexts in the msih.p4g solution
Write-Host "Updating migrations for all DbContexts..." -ForegroundColor Green

# Ensure we're in the project directory
$ProjectDirectory = Get-Location

# List of all DbContexts to update
$contextList = @(
    @{
        Context = "UserDbContext"
        MigrationName = "UpdateUserDbContext"
        OutputDir = "Server/Features/Base/UserService/Data/Migrations"
    },
    @{
        Context = "SmsDbContext"
        MigrationName = "UpdateSmsDbContext"
        OutputDir = "Server/Features/Base/SmsService/Data/Migrations"
    },
    @{
        Context = "CampaignDbContext"
        MigrationName = "UpdateCampaignDbContext"
        OutputDir = "Server/Features/CampaignService/Data/Migrations"
    },
    @{
        Context = "PaymentDbContext"
        MigrationName = "UpdatePaymentDbContext"
        OutputDir = "Server/Features/Base/PaymentService/Data/Migrations"
    },
    @{
        Context = "SettingsDbContext"
        MigrationName = "UpdateSettingsDbContext"
        OutputDir = "Server/Features/Base/SettingsService/Data/Migrations"
    },
    @{
        Context = "ProfileDbContext"
        MigrationName = "UpdateProfileDbContext"
        OutputDir = "Server/Features/Base/ProfileService/Data/Migrations"
    },
    @{
        Context = "DonorDbContext"
        MigrationName = "UpdateDonorDbContext"
        OutputDir = "Server/Features/DonorService/Data/Migrations"
    },
    @{
        Context = "DonationDbContext"
        MigrationName = "UpdateDonationDbContext"
        OutputDir = "Server/Features/DonationService/Data/Migrations"
    }
)

# Create migrations for each DbContext
foreach ($context in $contextList) {
    Write-Host "Creating migration for $($context.Context)..." -ForegroundColor Cyan
    
    $command = "dotnet ef migrations add $($context.MigrationName) --context $($context.Context) --output-dir $($context.OutputDir)"
    Write-Host "Running: $command"
    
    try {
        Invoke-Expression $command
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to create migration for $($context.Context). Exit code: $LASTEXITCODE" -ForegroundColor Red
        } else {
            Write-Host "Successfully created migration for $($context.Context)" -ForegroundColor Green
        }
    } catch {
        Write-Host "Error creating migration for $($context.Context): $_" -ForegroundColor Red
    }
}

Write-Host "All migrations created. Now updating database..." -ForegroundColor Green

# Update database for each DbContext
foreach ($context in $contextList) {
    Write-Host "Updating database for $($context.Context)..." -ForegroundColor Cyan
    
    $command = "dotnet ef database update --context $($context.Context)"
    Write-Host "Running: $command"
    
    try {
        Invoke-Expression $command
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to update database for $($context.Context). Exit code: $LASTEXITCODE" -ForegroundColor Red
        } else {
            Write-Host "Successfully updated database for $($context.Context)" -ForegroundColor Green
        }
    } catch {
        Write-Host "Error updating database for $($context.Context): $_" -ForegroundColor Red
    }
}

Write-Host "All database migrations completed!" -ForegroundColor Green
