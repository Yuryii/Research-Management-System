# Start DAB (Data API Builder) SQL Server MCP Server
# Run this before using the SQL MCP in Cursor
#
# Usage: .\start-dab.ps1
# Then restart Cursor to pick up the MCP connection

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$ConfigPath = Join-Path $ProjectRoot "dab-config.json"

Write-Host "Starting DAB SQL Server MCP for CA-RMS..." -ForegroundColor Cyan
Write-Host "Config: $ConfigPath" -ForegroundColor Gray
Write-Host "MCP endpoint: http://localhost:5000/mcp" -ForegroundColor Gray
Write-Host ""

$env:ASPNETCORE_URLS = "http://localhost:5000"
$env:ASPNETCORE_ENVIRONMENT = "Development"

Set-Location $ProjectRoot
dab start --config $ConfigPath
