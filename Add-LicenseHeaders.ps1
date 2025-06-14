# Add-LicenseHeaders.ps1
# Script to add license headers to source code files

param (
    [switch]$WhatIf,
    [switch]$Help
)

# Display help information
if ($Help) {
    Write-Host "Add-LicenseHeaders.ps1 - Adds license headers to source code files"
    Write-Host ""
    Write-Host "PARAMETERS:"
    Write-Host "  -WhatIf     Shows what would happen without making changes"
    Write-Host "  -Help       Displays this help message"
    Write-Host ""
    Write-Host "EXAMPLE:"
    Write-Host "  .\Add-LicenseHeaders.ps1"
    Write-Host "  .\Add-LicenseHeaders.ps1 -WhatIf"
    exit
}

# C# and other code files header
$codeHeader = @"
/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

"@

# Razor files header
$razorHeader = @"
@* 
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 *@

"@

# JSON, XML, YAML, and config files header
$configHeader = @"
/*
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

"@

# CSS and SCSS files header
$cssHeader = @"
/*
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

"@

# HTML files header (in comment format)
$htmlHeader = @"
<!--
 Copyright (c) 2025 MSIH LLC. All rights reserved.
 This file is developed for Make Sure It Happens Inc.
 Unauthorized copying, modification, distribution, or use is prohibited.
-->

"@

# File extensions and their corresponding headers
$fileTypes = @{
    ".cs"     = $codeHeader
    ".razor"  = $razorHeader
    ".js"     = $codeHeader
    ".ts"     = $codeHeader
    ".css"    = $cssHeader
    ".scss"   = $cssHeader
    ".html"   = $htmlHeader
    ".cshtml" = $htmlHeader
    ".json"   = $configHeader
    ".xml"    = $configHeader
    ".config" = $configHeader
    ".yaml"   = $configHeader
    ".yml"    = $configHeader
}

# Directories to exclude
$excludeDirs = @(
    "\\obj\\", 
    "\\bin\\", 
    "\\node_modules\\", 
    "\\.vs\\",
    "\\.git\\"
)

# Get all relevant files
$allFiles = @()
foreach ($ext in $fileTypes.Keys) {
    $files = Get-ChildItem -Path . -Filter "*$ext" -Recurse | 
        Where-Object { 
            $filePath = $_.FullName
            $exclude = $false
            foreach ($dir in $excludeDirs) {
                if ($filePath -match $dir) {
                    $exclude = $true
                    break
                }
            }
            -not $exclude
        }
    $allFiles += $files
}

$totalFiles = $allFiles.Count
$processedFiles = 0
$modifiedFiles = 0
$skippedFiles = 0

Write-Host "Found $totalFiles files to process" -ForegroundColor Cyan

# Process files
foreach ($file in $allFiles) {
    $processedFiles++
    $ext = [System.IO.Path]::GetExtension($file.Name).ToLower()
    $header = $fileTypes[$ext]
    
    Write-Progress -Activity "Adding license headers" -Status "Processing file $processedFiles of $totalFiles ($($file.Name))" -PercentComplete (($processedFiles / $totalFiles) * 100)
    
    try {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop
        
        # Skip files that already have a copyright header
        if ($content -match "Copyright \(c\) \d{4} MSIH LLC") {
            Write-Host "Skipped (already has header): $($file.FullName)" -ForegroundColor Yellow
            $skippedFiles++
            continue
        }
        
        # If in WhatIf mode, just report what would happen
        if ($WhatIf) {
            Write-Host "Would add license header to: $($file.FullName)" -ForegroundColor Green
            $modifiedFiles++
            continue
        }
        
        # Add license header to the file
        $content = $header + $content
        Set-Content -Path $file.FullName -Value $content -NoNewline -ErrorAction Stop
        $modifiedFiles++
        Write-Host "Added license header to: $($file.FullName)" -ForegroundColor Green
    }
    catch {
        Write-Host "Error processing file: $($file.FullName) - $_" -ForegroundColor Red
    }
}

Write-Host "`nSummary:" -ForegroundColor Cyan
if ($WhatIf) {
    Write-Host "  Would add license headers to $modifiedFiles out of $totalFiles files" -ForegroundColor Cyan
    Write-Host "  $skippedFiles files already have license headers" -ForegroundColor Yellow
} else {
    Write-Host "  Added license headers to $modifiedFiles out of $totalFiles files" -ForegroundColor Cyan
    Write-Host "  $skippedFiles files already have license headers" -ForegroundColor Yellow
}
