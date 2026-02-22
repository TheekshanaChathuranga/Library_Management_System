# Test Script for Catalog Service API
# This script tests all the main endpoints

$baseUrl = "https://localhost:5001"
$credential = "admin:password123"
$bytes = [System.Text.Encoding]::UTF8.GetBytes($credential)
$base64 = [Convert]::ToBase64String($bytes)
$headers = @{ Authorization = "Basic $base64" }

# Handle certificate validation for older PowerShell versions
if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type) {
    $certCallback = @"
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
public class ServerCertificateValidationCallback
{
    public static void Ignore()
    {
        if(ServicePointManager.ServerCertificateValidationCallback ==null)
        {
            ServicePointManager.ServerCertificateValidationCallback += 
                delegate (
                    Object obj,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors errors
                )
                {
                    return true;
                };
        }
    }
}
"@
    Add-Type $certCallback
}
[ServerCertificateValidationCallback]::Ignore()

Write-Host "=== Catalog Service API Tests ===" -ForegroundColor Green
Write-Host ""

# Test 1: Search all books
Write-Host "Test 1: GET /api/books - Get all books" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/books" -Headers $headers
    Write-Host "Success! Found $($response.total) books" -ForegroundColor Green
    Write-Host "Sample: $($response.items[0].title) by $($response.items[0].author)" -ForegroundColor Cyan
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 2: Search with query
Write-Host "Test 2: GET /api/books?q=tolkien - Search for Tolkien books" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/books?q=tolkien" -Headers $headers
    Write-Host "Success! Found $($response.total) result(s)" -ForegroundColor Green
    if ($response.total -gt 0) {
        Write-Host "Result: $($response.items[0].title)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 3: Filter by availability
Write-Host "Test 3: GET /api/books?available=true - Get available books" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/books?available=true" -Headers $headers
    Write-Host "Success! Found $($response.total) available books" -ForegroundColor Green
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 4: Create a new book
Write-Host "Test 4: POST /api/books - Create a new book" -ForegroundColor Yellow
$newBook = @{
    title = "Clean Code"
    author = "Robert C. Martin"
    isbn = "978-0-13-235088-4"
    genre = "Programming"
    isAvailable = $true
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/books" -Method Post -Headers $headers -Body $newBook -ContentType "application/json"
    $bookId = $response.id
    Write-Host "Success! Created book with ID: $bookId" -ForegroundColor Green
    Write-Host "Book: $($response.title)" -ForegroundColor Cyan
    
    # Test 5: Get book by ID
    Write-Host ""
    Write-Host "Test 5: GET /api/books/$bookId - Get book by ID" -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "$baseUrl/api/books/$bookId" -Headers $headers
    Write-Host "Success! Retrieved: $($response.title)" -ForegroundColor Green
    
    # Test 6: Update book availability
    Write-Host ""
    Write-Host "Test 6: PATCH /api/books/$bookId/availability - Update availability" -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "$baseUrl/api/books/$bookId/availability" -Method Patch -Headers $headers -Body "false" -ContentType "application/json"
    Write-Host "Success! Availability updated to: $($response.isAvailable)" -ForegroundColor Green
    
    # Test 7: Update book details
    Write-Host ""
    Write-Host "Test 7: PUT /api/books/$bookId - Update book details" -ForegroundColor Yellow
    $updateBook = @{
        title = "Clean Code: A Handbook of Agile Software Craftsmanship"
        author = "Robert C. Martin"
        isbn = "978-0-13-235088-4"
        genre = "Software Engineering"
        isAvailable = $true
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/books/$bookId" -Method Put -Headers $headers -Body $updateBook -ContentType "application/json"
    Write-Host "Success! Updated: $($response.title)" -ForegroundColor Green
    
    # Test 8: Delete book
    Write-Host ""
    Write-Host "Test 8: DELETE /api/books/$bookId - Delete book" -ForegroundColor Yellow
    Invoke-RestMethod -Uri "$baseUrl/api/books/$bookId" -Method Delete -Headers $headers
    Write-Host "Success! Book deleted" -ForegroundColor Green
    
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Testing Complete ===" -ForegroundColor Green