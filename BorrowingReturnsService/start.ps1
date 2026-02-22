# Quick Start Script for Borrowing Returns Service
# Run this script to start PostgreSQL, Redis (shared with Catalog Service), and the API

Write-Host "=== Borrowing Returns Microservice Quick Start ===" -ForegroundColor Green
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker status..." -ForegroundColor Yellow
docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Start PostgreSQL container (shared with Catalog Service)
Write-Host "Starting PostgreSQL container..." -ForegroundColor Yellow
docker run --name postgres-catalog -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=catalogdb -p 5432:5432 -d postgres:16 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "PostgreSQL started successfully!" -ForegroundColor Green
} else {
    Write-Host "PostgreSQL container already exists, starting it..." -ForegroundColor Yellow
    docker start postgres-catalog 2>&1 | Out-Null
}

# Start Redis container (shared with Catalog Service)
Write-Host "Starting Redis container..." -ForegroundColor Yellow
docker run --name redis-catalog -p 6379:6379 -d redis:7-alpine 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "Redis started successfully!" -ForegroundColor Green
} else {
    Write-Host "Redis container already exists, starting it..." -ForegroundColor Yellow
    docker start redis-catalog 2>&1 | Out-Null
}

# Wait for services to be ready
Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Navigate to project directory
Set-Location -Path "$PSScriptRoot\BorrowingReturnsService\BorrowingReturnsService"

# Apply migrations
Write-Host "Applying database migrations..." -ForegroundColor Yellow
dotnet ef database update

# Run the application
Write-Host ""
Write-Host "=== Starting Borrowing Returns Service ===" -ForegroundColor Green
Write-Host "API will be available at:" -ForegroundColor Cyan
Write-Host "  - HTTPS: https://localhost:5006" -ForegroundColor Cyan
Write-Host "  - HTTP:  http://localhost:5005" -ForegroundColor Cyan
Write-Host "  - Swagger: https://localhost:5006/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "External Service Dependencies:" -ForegroundColor Cyan
Write-Host "  - CatalogService: http://localhost:5000" -ForegroundColor Cyan
Write-Host "  - InventoryService: http://localhost:5002" -ForegroundColor Cyan
Write-Host ""
Write-Host "Shared Infrastructure:" -ForegroundColor Cyan
Write-Host "  - PostgreSQL: localhost:5432 (database: borrowing_returns_db)" -ForegroundColor Cyan
Write-Host "  - Redis: localhost:6379" -ForegroundColor Cyan
Write-Host ""
Write-Host "JWT Configuration:" -ForegroundColor Cyan
Write-Host "  - Issuer: BorrowingReturnsService" -ForegroundColor Cyan
Write-Host "  - Audience: BorrowingReturnsClient" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Yellow
Write-Host ""

dotnet run
