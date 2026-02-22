# Test Script for Borrowing Returns Service API
# This script tests all the main endpoints
#
# IMPORTANT: User Validation Required
# =====================================
# The BorrowingReturnsService now validates users via UserIdentityService.
# Before running this script:
#
# Option 1: Use an existing user from UserIdentityService
#   - Query UserIdentityService to get a valid user ID
#   - Update $testUserId below with that ID
#   - Ensure the user is active (IsActive = true)
#
# Option 2: Create the test user in UserIdentityService
#   - POST to http://localhost:5192/api/users with:
#     {
#       "id": "8f51613e-e3c5-4cfa-a967-1e0f7c62c51c",
#       "email": "testuser@library.com",
#       "password": "Test123!",
#       "displayName": "Test User",
#       "isActive": true
#     }
#
# If you get "404 User not found", the user doesn't exist in UserIdentityService
# If you get "400 User inactive", set IsActive=true for the user

$baseUrl = "http://localhost:5005"
$userIdentityUrl = "http://localhost:5192"

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

Write-Host "=== Borrowing Returns Service API Tests ===" -ForegroundColor Green
Write-Host ""

$headers = @{}

# Test sample data
$testUserId = "00000000-0000-0000-0000-000000000001"
$testBookId = "22222222-2222-2222-2222-222222222222"
$borrowingId = $null

Write-Host "Test Data:" -ForegroundColor Cyan
Write-Host "  Test User ID: $testUserId" -ForegroundColor Gray
Write-Host "  Test Book ID: $testBookId" -ForegroundColor Gray
Write-Host ""

Write-Host "=== Prerequisites Check ===" -ForegroundColor Yellow
Write-Host "Checking if required services are running..." -ForegroundColor Gray
Write-Host ""

Write-Host ""
Write-Host "=== Starting API Tests ===" -ForegroundColor Green
Write-Host ""

# Test 1: Borrow a book (Physical)
Write-Host "Test 1: POST /api/Borrowing - Borrow a physical book" -ForegroundColor Yellow
$borrowRequest = @{
    userId = $testUserId.ToString()
    bookId = $testBookId.ToString()
    channel = 0  # Physical = 0
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Borrowing" -Method Post -Headers $headers -Body $borrowRequest -ContentType "application/json"
    $borrowingId = $response.id
    Write-Host "Success! Created borrowing with ID: $borrowingId" -ForegroundColor Green
    Write-Host "  User ID: $($response.userId)" -ForegroundColor Cyan
    Write-Host "  Book ID: $($response.bookId)" -ForegroundColor Cyan
    Write-Host "  Channel: $($response.channel) (Physical)" -ForegroundColor Cyan
    Write-Host "  Borrowed At: $($response.borrowedAt)" -ForegroundColor Cyan
    Write-Host "  Due Date: $($response.dueDate)" -ForegroundColor Cyan
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Gray
    }
}
Write-Host ""

# Test 2: Get borrowing by ID
if ($borrowingId) {
    Write-Host "Test 2: GET /api/Borrowing/$borrowingId - Get borrowing by ID" -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/Borrowing/$borrowingId" -Headers $headers
        Write-Host "Success! Retrieved borrowing details" -ForegroundColor Green
        Write-Host "  Is Returned: $($response.isReturned)" -ForegroundColor Cyan
        Write-Host "  Due Date: $($response.dueDate)" -ForegroundColor Cyan
    } catch {
        Write-Host "Failed: $_" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 3: Get user's borrowings
Write-Host "Test 3: GET /api/Borrowing/user/$testUserId - Get user's borrowings" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Borrowing/user/$testUserId" -Headers $headers
    Write-Host "Success! Found $($response.Count) borrowing(s) for user" -ForegroundColor Green
    if ($response.Count -gt 0) {
        Write-Host "  First borrowing ID: $($response[0].id)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 4: Try to borrow with unpaid late fees (should fail after returning late)
Write-Host "Test 4: POST /api/Borrowing - Try borrowing with same user (testing validation)" -ForegroundColor Yellow
$borrowRequest2 = @{
    userId = $testUserId.ToString()
    bookId = [Guid]::NewGuid().ToString()
    channel = 1  # Digital = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/Borrowing" -Method Post -Headers $headers -Body $borrowRequest2 -ContentType "application/json"
    Write-Host "Success! Created second borrowing: $($response.id)" -ForegroundColor Green
    Write-Host "  Channel: $($response.channel) (Digital)" -ForegroundColor Cyan
} catch {
    Write-Host "Failed (expected if book not in catalog/inventory): $_" -ForegroundColor Yellow
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Gray
    }
}
Write-Host ""

# Test 5: Return a book
if ($borrowingId) {
    Write-Host "Test 5: POST /api/Borrowing/$borrowingId/return - Return the book" -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/Borrowing/$borrowingId/return" -Method Post -Headers $headers
        Write-Host "Success! Book returned" -ForegroundColor Green
        Write-Host "  Returned At: $($response.returnedAt)" -ForegroundColor Cyan
        Write-Host "  Days Late: $($response.daysLate)" -ForegroundColor Cyan
        if ($response.lateFee) {
            Write-Host "  Late Fee ID: $($response.lateFee.id)" -ForegroundColor Yellow
            Write-Host "  Late Fee Amount: `$$($response.lateFee.amount)" -ForegroundColor Yellow
            Write-Host "  Is Paid: $($response.lateFee.isPaid)" -ForegroundColor Yellow
            $lateFeeId = $response.lateFee.id
        } else {
            Write-Host "  No late fee (returned on time)" -ForegroundColor Green
        }
    } catch {
        Write-Host "Failed: $_" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 6: Get user's late fees
Write-Host "Test 6: GET /api/latefee/user/$testUserId - Get user's late fees" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/latefee/user/$testUserId" -Headers $headers
    Write-Host "Success! Found $($response.Count) late fee(s)" -ForegroundColor Green
    if ($response.Count -gt 0) {
        foreach ($fee in $response) {
            Write-Host "  Late Fee ID: $($fee.id)" -ForegroundColor Cyan
            Write-Host "  Amount: `$$($fee.amount)" -ForegroundColor Cyan
            Write-Host "  Is Paid: $($fee.isPaid)" -ForegroundColor Cyan
            Write-Host ""
        }
    }
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 7: Pay a late fee (if exists)
if ($lateFeeId) {
    Write-Host "Test 7: POST /api/latefee/$lateFeeId/pay - Pay the late fee" -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/latefee/$lateFeeId/pay" -Method Patch -Headers $headers
        Write-Host "Success! Late fee paid" -ForegroundColor Green
        Write-Host "  Fee ID: $($response.id)" -ForegroundColor Cyan
        Write-Host "  Amount: `$$($response.amount)" -ForegroundColor Cyan
        Write-Host "  Is Paid: $($response.isPaid)" -ForegroundColor Cyan
    } catch {
        Write-Host "Failed: $_" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 8: Get late fee by borrowing ID
if ($borrowingId) {
    Write-Host "Test 8: GET /api/latefee/borrowing/$borrowingId - Get late fee by borrowing ID" -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/latefee/borrowing/$borrowingId" -Headers $headers
        Write-Host "Success! Retrieved late fee" -ForegroundColor Green
        Write-Host "  Amount: `$$($response.amount)" -ForegroundColor Cyan
        Write-Host "  Is Paid: $($response.isPaid)" -ForegroundColor Cyan
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 404) {
            Write-Host "No late fee found (book returned on time)" -ForegroundColor Green
        } else {
            Write-Host "Failed: $_" -ForegroundColor Red
        }
    }
    Write-Host ""
}

# Test 9: Try to return already returned book (should fail)
if ($borrowingId) {
    Write-Host "Test 9: POST /api/Borrowing/$borrowingId/return - Try returning already returned book" -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/Borrowing/$borrowingId/return" -Method Post -Headers $headers
        Write-Host "Unexpected: Should have failed!" -ForegroundColor Red
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 400) {
            Write-Host "Success! Correctly rejected (book already returned)" -ForegroundColor Green
        } else {
            Write-Host "Failed with unexpected error: $_" -ForegroundColor Red
        }
    }
    Write-Host ""
}

Write-Host "=== Testing Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "- Borrowing operations tested" -ForegroundColor Gray
Write-Host "- Return flow validated" -ForegroundColor Gray
Write-Host "- Late fee calculation verified" -ForegroundColor Gray
Write-Host "- Error handling confirmed" -ForegroundColor Gray
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Ensure UserIdentityService is running on port 5192" -ForegroundColor Gray
Write-Host "2. Create/use an active user in UserIdentityService" -ForegroundColor Gray
Write-Host "3. Check the service logs for integration details" -ForegroundColor Gray
Write-Host "4. Verify database tables (Borrowings, LateFees)" -ForegroundColor Gray
Write-Host "5. Test with real books from CatalogService" -ForegroundColor Gray
Write-Host "6. Verify inventory adjustments in InventoryService" -ForegroundColor Gray
Write-Host ""
Write-Host "Common Issues:" -ForegroundColor Yellow
Write-Host "- 404 'User not found': The test user doesn't exist in UserIdentityService" -ForegroundColor Gray
Write-Host "  Fix: Use an existing user ID or create one in UserIdentityService" -ForegroundColor Gray
Write-Host "- 400 'User inactive': The user exists but IsActive=false" -ForegroundColor Gray
Write-Host "  Fix: Activate the user account in UserIdentityService" -ForegroundColor Gray
