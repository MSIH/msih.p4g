# ClearMigrationsLock.ps1
# Script to clear the EF Migrations lock from SQLite database

# Locate the SQLite database file
$dbFile = "msih_p4g.db"

if (-not (Test-Path $dbFile)) {
    Write-Host "Database file not found at: $dbFile" -ForegroundColor Red
    Write-Host "Please run this script from the project root directory where the SQLite database is located."
    exit 1
}

Write-Host "Found database file: $dbFile" -ForegroundColor Green

# Make sure SQLite is available
try {
    # Using SQLite command line from .NET tool if installed
    $sqliteInstalled = $null -ne (Get-Command "sqlite3" -ErrorAction SilentlyContinue)
    
    if (-not $sqliteInstalled) {
        Write-Host "SQLite command line tool not found. Installing dotnet-sqlite3 tool..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-sqlite3
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to install dotnet-sqlite3 tool. Please install it manually or install sqlite3 command line tool." -ForegroundColor Red
            exit 1
        }
        
        # Use dotnet-sqlite3 instead
        $sqliteCmd = "dotnet sqlite3"
    } else {
        $sqliteCmd = "sqlite3"
    }
}
catch {
    Write-Host "Error checking for SQLite tools: $_" -ForegroundColor Red
    exit 1
}

# Check if migrations lock table exists
$checkTableCmd = "$sqliteCmd '$dbFile' 'SELECT name FROM sqlite_master WHERE type=''table'' AND name=''__EFMigrationsLock'''"
$tableExists = Invoke-Expression $checkTableCmd

if ($tableExists) {
    Write-Host "Found __EFMigrationsLock table. Attempting to clear lock..." -ForegroundColor Yellow
    
    # Delete the migrations lock table
    $dropTableCmd = "$sqliteCmd '$dbFile' 'DROP TABLE __EFMigrationsLock'"
    Invoke-Expression $dropTableCmd
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Successfully dropped __EFMigrationsLock table!" -ForegroundColor Green
        Write-Host "You can now run your application again. The migration should proceed normally." -ForegroundColor Green
    } else {
        Write-Host "Failed to drop __EFMigrationsLock table." -ForegroundColor Red
    }
} else {
    Write-Host "No __EFMigrationsLock table found in the database." -ForegroundColor Yellow
    
    # Create a backup of the database
    Copy-Item $dbFile "$dbFile.backup"
    Write-Host "Created backup of the database at $dbFile.backup" -ForegroundColor Green
    
    # Try an alternate approach - open and close a connection
    Write-Host "Trying to open a connection to the database to fix potential locking issues..." -ForegroundColor Yellow
    $vacuumCmd = "$sqliteCmd '$dbFile' 'VACUUM'"
    Invoke-Expression $vacuumCmd
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Successfully performed VACUUM on the database." -ForegroundColor Green
        Write-Host "This may have fixed any locking issues. Try running your application again." -ForegroundColor Green
    } else {
        Write-Host "Failed to perform VACUUM on the database." -ForegroundColor Red
    }
}

Write-Host "`nAdditional troubleshooting steps if the issue persists:" -ForegroundColor Cyan
Write-Host "1. Make sure no other process is using the database file"
Write-Host "2. Try deleting the database file completely (if you can afford to lose data) and let EF Core recreate it"
Write-Host "3. Check for file system permissions issues"
Write-Host "4. Run the following SQL commands directly if the script didn't work:"
Write-Host "   $sqliteCmd '$dbFile' 'PRAGMA busy_timeout = 5000; PRAGMA journal_mode=DELETE; DROP TABLE IF EXISTS __EFMigrationsLock;'"
