param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

Write-Host "Creating migration: $MigrationName for SmsDbContext"

# Navigate to the project root
Set-Location -Path (Get-Item -Path ".").FullName

# Run EF Core migration command
dotnet ef migrations add $MigrationName `
    --context SmsDbContext `
    --output-dir Server/Features/Base/SmsService/Data/Migrations `
    --startup-project .

Write-Host "Migration created successfully."
