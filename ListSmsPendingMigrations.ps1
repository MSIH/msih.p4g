# Get Pending SMS Migrations Script for .NET 9 with EF Core
# This script lists all pending migrations for the SMS DbContext

# Project variables
$projectDir = $PSScriptRoot

# Run the EF Core command to list pending migrations
Write-Host "Listing pending migrations for SmsDbContext..."
dotnet ef migrations list `
    --context SmsDbContext `
    --project $projectDir
