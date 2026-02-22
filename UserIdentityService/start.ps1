# Quick Start Script for User Identity Service
# Run this script to start PostgreSQL, Redis, and the API

Write-Host "=== User Identity Microservice Quick Start ===" -ForegroundColor Green
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker status..." -ForegroundColor Yellow
docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Start PostgreSQL container (shared with CatalogService)
Write-Host "Starting PostgreSQL container..." -ForegroundColor Yellow
docker run --name postgres-catalog -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=catalogdb -p 5432:5432 -d postgres:16 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "PostgreSQL started successfully!" -ForegroundColor Green
} else {
    Write-Host "PostgreSQL container already exists, ensuring it's running..." -ForegroundColor Yellow
    docker start postgres-catalog 2>&1 | Out-Null
}

# Start Redis container (shared with CatalogService)
Write-Host "Starting Redis container..." -ForegroundColor Yellow
docker run --name redis-catalog -p 6379:6379 -d redis:7-alpine 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "Redis started successfully!" -ForegroundColor Green
} else {
    Write-Host "Redis container already exists, ensuring it's running..." -ForegroundColor Yellow
    docker start redis-catalog 2>&1 | Out-Null
}

# Wait for services to be ready
Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Create useridentity database if it doesn't exist
Write-Host "Creating useridentity database..." -ForegroundColor Yellow
docker exec postgres-catalog psql -U postgres -c "CREATE DATABASE useridentity;" 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "Database created successfully!" -ForegroundColor Green
} else {
    Write-Host "Database already exists or connection issue (continuing...)." -ForegroundColor Yellow
}

# Navigate to project directory
Set-Location -Path "$PSScriptRoot\src\UserIdentityService.Api"

# Apply migrations
Write-Host "Applying database migrations..." -ForegroundColor Yellow
dotnet ef database update

# Run the application
Write-Host ""
Write-Host "=== Starting User Identity Service ===" -ForegroundColor Green
Write-Host "API will be available at:" -ForegroundColor Cyan
Write-Host "  - HTTPS: https://localhost:7032" -ForegroundColor Cyan
Write-Host "  - HTTP:  http://localhost:5192" -ForegroundColor Cyan
Write-Host "  - Swagger: https://localhost:7032/swagger" -ForegroundColor Cyan
Write-Host "  - Health Check: https://localhost:7032/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "Default Admin Credentials:" -ForegroundColor Cyan
Write-Host "  - Email: admin@library.local" -ForegroundColor Cyan
Write-Host "  - Password: ChangeMe!123" -ForegroundColor Cyan
Write-Host ""
Write-Host "System Roles:" -ForegroundColor Cyan
Write-Host "  - Admin (full access)" -ForegroundColor Cyan
Write-Host "  - Librarian (manage catalog, view catalog, issue loans)" -ForegroundColor Cyan
Write-Host "  - Member (view catalog)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Yellow
Write-Host ""

dotnet run
