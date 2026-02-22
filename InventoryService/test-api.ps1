# Test Script for Inventory Service API
# This script tests all the main endpoints

$baseUrl = "http://localhost:5002"

# No need for certificate validation in development

Write-Host "=== Inventory Service API Tests ===" -ForegroundColor Green
Write-Host ""

# Test 1: Get all inventory
Write-Host "Test 1: GET /api/inventory - Get all inventory records" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory"
    Write-Host "Success! Found $($response.Count) inventory records" -ForegroundColor Green
    if ($response.Count -gt 0) {
        Write-Host "Sample: $($response[0].title) - Physical: $($response[0].physicalAvailable)/$($response[0].physicalTotal), Digital: $($response[0].digitalAvailable)/$($response[0].digitalTotal)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 2: Create a new inventory record (BookId must exist in CatalogService)
Write-Host "Test 2: POST /api/inventory - Create a new inventory record" -ForegroundColor Yellow
$newInventory = @{
    bookId = "44444444-4444-4444-4444-444444444444"  # Make sure this ID exists in CatalogService
    physicalTotal = 4
    digitalTotal = 20
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory" -Method Post -Body $newInventory -ContentType "application/json"
    $bookId = $response.bookId
    Write-Host "Success! Created inventory for book: $bookId" -ForegroundColor Green
    if ($response.bookMetadata) {
        Write-Host "Title: $($response.bookMetadata.title)" -ForegroundColor Cyan
        Write-Host "Author: $($response.bookMetadata.author)" -ForegroundColor Cyan
    }
    Write-Host "Physical copies: $($response.physicalAvailable)/$($response.physicalTotal)" -ForegroundColor Cyan
    Write-Host "Digital copies: $($response.digitalAvailable)/$($response.digitalTotal)" -ForegroundColor Cyan
    
    # Test 3: Get inventory by book ID
    Write-Host ""
    Write-Host "Test 3: GET /api/inventory/$bookId - Get inventory by book ID" -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory/$bookId"
    Write-Host "Success! Retrieved inventory" -ForegroundColor Green
    if ($response.bookMetadata) {
        Write-Host "Book: $($response.bookMetadata.title)" -ForegroundColor Cyan
    }
    Write-Host "Physical available: $($response.physicalAvailable)" -ForegroundColor Cyan
    Write-Host "Digital available: $($response.digitalAvailable)" -ForegroundColor Cyan
    
    # Test 4: Borrow a physical copy
    Write-Host ""
    Write-Host "Test 4: POST /api/inventory/$bookId/borrow - Borrow a physical copy" -ForegroundColor Yellow
    $borrowRequest = @{
        quantity = 1
        channel = 0  # Physical
        reference = "loan-test-001"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory/$bookId/borrow" -Method Post -Body $borrowRequest -ContentType "application/json"
    Write-Host "Success! Borrowed 1 physical copy" -ForegroundColor Green
    Write-Host "Physical available now: $($response.physicalAvailable)/$($response.physicalTotal)" -ForegroundColor Cyan
    
    # Test 5: Borrow a digital copy
    Write-Host ""
    Write-Host "Test 5: POST /api/inventory/$bookId/borrow - Borrow a digital copy" -ForegroundColor Yellow
    $borrowDigital = @{
        quantity = 2
        channel = 1  # Digital
        reference = "digital-loan-002"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory/$bookId/borrow" -Method Post -Body $borrowDigital -ContentType "application/json"
    Write-Host "Success! Borrowed 2 digital copies" -ForegroundColor Green
    Write-Host "Digital available now: $($response.digitalAvailable)/$($response.digitalTotal)" -ForegroundColor Cyan
    
    # Test 6: Return a physical copy
    Write-Host ""
    Write-Host "Test 6: POST /api/inventory/$bookId/return - Return a physical copy" -ForegroundColor Yellow
    $returnRequest = @{
        quantity = 1
        channel = 0  # Physical
        reference = "return-test-001"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory/$bookId/return" -Method Post -Body $returnRequest -ContentType "application/json"
    Write-Host "Success! Returned 1 physical copy" -ForegroundColor Green
    Write-Host "Physical available now: $($response.physicalAvailable)/$($response.physicalTotal)" -ForegroundColor Cyan
    
    # Test 7: Return digital copies
    Write-Host ""
    Write-Host "Test 7: POST /api/inventory/$bookId/return - Return digital copies" -ForegroundColor Yellow
    $returnDigital = @{
        quantity = 2
        channel = 1  # Digital
        reference = "digital-return-002"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory/$bookId/return" -Method Post -Body $returnDigital -ContentType "application/json"
    Write-Host "Success! Returned 2 digital copies" -ForegroundColor Green
    Write-Host "Digital available now: $($response.digitalAvailable)/$($response.digitalTotal)" -ForegroundColor Cyan
    
    # Test 8: Update inventory totals
    Write-Host ""
    Write-Host "Test 8: PUT /api/inventory/$bookId - Update inventory totals" -ForegroundColor Yellow
    $updateTotals = @{
        physicalTotal = 10
        digitalTotal = 50
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory/$bookId" -Method Put -Body $updateTotals -ContentType "application/json"
    Write-Host "Success! Updated totals" -ForegroundColor Green
    Write-Host "New physical total: $($response.physicalTotal)" -ForegroundColor Cyan
    Write-Host "New digital total: $($response.digitalTotal)" -ForegroundColor Cyan
    
    # Test 9: Try to borrow more than available (should fail)
    Write-Host ""
    Write-Host "Test 9: POST /api/inventory/$bookId/borrow - Try to borrow more than available" -ForegroundColor Yellow
    $overBorrow = @{
        quantity = 100
        channel = 0
        reference = "over-borrow-test"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory/$bookId/borrow" -Method Post -Body $overBorrow -ContentType "application/json"
        Write-Host "Unexpected: Request should have failed!" -ForegroundColor Red
    } catch {
        Write-Host "Success! Validation working - cannot borrow more than available" -ForegroundColor Green
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    # Test 10: Check inventory again to verify final state
    Write-Host ""
    Write-Host "Test 10: GET /api/inventory/$bookId - Verify final state" -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "$baseUrl/api/inventory/$bookId"
    Write-Host "Success! Final inventory state:" -ForegroundColor Green
    if ($response.bookMetadata) {
        Write-Host "Book: $($response.bookMetadata.title) by $($response.bookMetadata.author)" -ForegroundColor Cyan
    }
    Write-Host "Physical: $($response.physicalAvailable)/$($response.physicalTotal)" -ForegroundColor Cyan
    Write-Host "Digital: $($response.digitalAvailable)/$($response.digitalTotal)" -ForegroundColor Cyan
    Write-Host "Last Updated: $($response.lastUpdatedUtc)" -ForegroundColor Cyan
    
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
    Write-Host "Error Details: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Testing Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Summary of Inventory Operations:" -ForegroundColor Cyan
Write-Host "  - Channel 0 = Physical copies" -ForegroundColor Cyan
Write-Host "  - Channel 1 = Digital copies" -ForegroundColor Cyan
Write-Host "  - Borrow operation decreases availability" -ForegroundColor Cyan
Write-Host "  - Return operation increases availability" -ForegroundColor Cyan
Write-Host "  - Availability is clamped between 0 and total" -ForegroundColor Cyan
