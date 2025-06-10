# Generate SMS Migration Script for .NET 9 with EF Core
# This script generates a new migration for the SMS DbContext

param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

# Project variables
$projectDir = $PSScriptRoot
$outputDir = Join-Path $projectDir "Server\Features\Base\SmsService\Data\Migrations"

# Ensure the migrations directory exists
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force
}

# Run the EF Core command to add a migration
Write-Host "Adding migration '$MigrationName' for SmsDbContext..."
dotnet ef migrations add $MigrationName `
    --context SmsDbContext `
    --output-dir "Server\Features\Base\SmsService\Data\Migrations" `
    --namespace "msih.p4g.Server.Features.Base.SmsService.Data.Migrations" `
    --project $projectDir

# Check if the command was successful
if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration '$MigrationName' created successfully!" -ForegroundColor Green
    Write-Host "Migration files created in: $outputDir"
} else {
    Write-Host "Failed to create migration. Please check the error messages above." -ForegroundColor Red
}
