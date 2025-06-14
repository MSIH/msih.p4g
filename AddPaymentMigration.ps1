$migrationName = "InitialPaymentDbContext"
$timestamp = Get-Date -Format "yyyyMMddHHmmss"

# Create the migration using EF Core Tools
dotnet ef migrations add ${timestamp}_${migrationName} --context PaymentDbContext --output-dir Server/Features/Base/PaymentService/Data/Migrations
