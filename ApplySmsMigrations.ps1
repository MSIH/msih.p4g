# Apply SMS Migrations Script for .NET 9 with EF Core
# This script applies all pending migrations for the SMS DbContext

# Project variables
$projectDir = $PSScriptRoot

# Run the EF Core command to update the database
Write-Host "Applying migrations for SmsDbContext..."
dotnet ef database update `
    --context SmsDbContext `
    --project $projectDir

# Check if the command was successful
if ($LASTEXITCODE -eq 0) {
    Write-Host "Database updated successfully!" -ForegroundColor Green
} else {
    Write-Host "Failed to update database. Please check the error messages above." -ForegroundColor Red
}
