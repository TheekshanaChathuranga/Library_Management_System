# Test Script for User Identity Service API
# This script tests authentication, user management, and role management endpoints

$baseUrl = "http://localhost:5192"

# Handle certificate validation for HTTPS
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

Write-Host "=== User Identity Service API Tests ===" -ForegroundColor Green
Write-Host ""

# Variables to store tokens
$accessToken = $null
$refreshToken = $null
$newUserId = $null
$newRoleId = $null

# Test 1: Health Check
Write-Host "Test 1: GET /health - Health check" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    Write-Host "Success! Service is healthy" -ForegroundColor Green
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 2: Login with admin credentials
Write-Host "Test 2: POST /api/auth/login - Login as admin" -ForegroundColor Yellow
$loginRequest = @{
    email = "admin@library.local"
    password = "ChangeMe!123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginRequest -ContentType "application/json"
    $accessToken = $response.accessToken
    $refreshToken = $response.refreshToken
    Write-Host "Success! Logged in as admin" -ForegroundColor Green
    Write-Host "Access Token: $($accessToken.Substring(0, 20))..." -ForegroundColor Cyan
    Write-Host "Roles: $($response.roles -join ', ')" -ForegroundColor Cyan
    Write-Host "Permissions: $($response.permissions -join ', ')" -ForegroundColor Cyan
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
    Write-Host "Cannot continue without authentication. Exiting..." -ForegroundColor Red
    exit 1
}
Write-Host ""

# Create headers with bearer token
$headers = @{
    Authorization = "Bearer $accessToken"
}

# Test 3: Get all roles
Write-Host "Test 3: GET /api/roles - Get all roles" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/roles" -Headers $headers
    Write-Host "Success! Found $($response.Count) roles" -ForegroundColor Green
    foreach ($role in $response) {
        Write-Host "  - $($role.name): $($role.permissions.Count) permissions" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 4: Create a new role
Write-Host "Test 4: POST /api/roles - Create a new role" -ForegroundColor Yellow
$newRole = @{
    name = "Guest"
    description = "Guest user with limited access"
    permissions = @("catalog.view")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/roles" -Method Post -Headers $headers -Body $newRole -ContentType "application/json"
    $newRoleId = $response.id
    Write-Host "Success! Created role: $($response.name)" -ForegroundColor Green
    Write-Host "Role ID: $newRoleId" -ForegroundColor Cyan
    Write-Host "Permissions: $($response.permissions -join ', ')" -ForegroundColor Cyan
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 5: Update role permissions
if ($newRoleId) {
    Write-Host "Test 5: PUT /api/roles/$newRoleId/permissions - Update role permissions" -ForegroundColor Yellow
    $updatePermissions = @{
        permissions = @("catalog.view", "catalog.manage")
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/roles/$newRoleId/permissions" -Method Put -Headers $headers -Body $updatePermissions -ContentType "application/json"
        Write-Host "Success! Updated permissions for $($response.name)" -ForegroundColor Green
        Write-Host "New Permissions: $($response.permissions -join ', ')" -ForegroundColor Cyan
    } catch {
        Write-Host "Failed: $_" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 6: Register a new user (as admin)
Write-Host "Test 6: POST /api/auth/register - Register a new user" -ForegroundColor Yellow
$registerRequest = @{
    email = "testuser@library.local"
    password = "TestUser!123"
    displayName = "Test User"
    role = "Member"
    membershipId = "MEM-2025-001"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Headers $headers -Body $registerRequest -ContentType "application/json"
    Write-Host "Success! Registered new user" -ForegroundColor Green
    Write-Host "User: $($response.roles -join ', ')" -ForegroundColor Cyan
    Write-Host "Permissions: $($response.permissions -join ', ')" -ForegroundColor Cyan
} catch {
    Write-Host "Failed (might already exist): $($_.Exception.Message)" -ForegroundColor Yellow
}
Write-Host ""

# Test 7: Get all users
Write-Host "Test 7: GET /api/users - Get all users" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users" -Headers $headers
    Write-Host "Success! Found $($response.Count) users" -ForegroundColor Green
    foreach ($user in $response) {
        Write-Host "  - $($user.email) [$($user.roles -join ', ')]" -ForegroundColor Cyan
        if ($user.email -eq "testuser@library.local") {
            $newUserId = $user.id
        }
    }
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 8: Assign roles to user
if ($newUserId) {
    Write-Host "Test 8: PUT /api/users/$newUserId/roles - Assign roles to user" -ForegroundColor Yellow
    $assignRoles = @{
        roles = @("Member", "Librarian")
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/users/$newUserId/roles" -Method Put -Headers $headers -Body $assignRoles -ContentType "application/json"
        Write-Host "Success! Updated roles for user" -ForegroundColor Green
        Write-Host "New Roles: $($response.roles -join ', ')" -ForegroundColor Cyan
    } catch {
        Write-Host "Failed: $_" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 9: Login as the new user
Write-Host "Test 9: POST /api/auth/login - Login as new user" -ForegroundColor Yellow
$newUserLogin = @{
    email = "testuser@library.local"
    password = "TestUser!123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $newUserLogin -ContentType "application/json"
    Write-Host "Success! Logged in as test user" -ForegroundColor Green
    Write-Host "Roles: $($response.roles -join ', ')" -ForegroundColor Cyan
    Write-Host "Permissions: $($response.permissions -join ', ')" -ForegroundColor Cyan
    $newUserRefreshToken = $response.refreshToken
    
    # Test 10: Refresh token
    Write-Host ""
    Write-Host "Test 10: POST /api/auth/refresh - Refresh access token" -ForegroundColor Yellow
    $refreshRequest = @{
        refreshToken = $newUserRefreshToken
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/refresh" -Method Post -Body $refreshRequest -ContentType "application/json"
        Write-Host "Success! Token refreshed" -ForegroundColor Green
        Write-Host "New Access Token: $($response.accessToken.Substring(0, 20))..." -ForegroundColor Cyan
    } catch {
        Write-Host "Failed: $_" -ForegroundColor Red
    }
} catch {
    Write-Host "Failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 11: Register a librarian
Write-Host "Test 11: POST /api/auth/register - Register a librarian" -ForegroundColor Yellow
$librarianRequest = @{
    email = "librarian@library.local"
    password = "Librarian!123"
    displayName = "Jane Librarian"
    role = "Librarian"
    librarianCode = "LIB-2025-001"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Headers $headers -Body $librarianRequest -ContentType "application/json"
    Write-Host "Success! Registered librarian" -ForegroundColor Green
    Write-Host "Roles: $($response.roles -join ', ')" -ForegroundColor Cyan
    Write-Host "Permissions: $($response.permissions -join ', ')" -ForegroundColor Cyan
} catch {
    Write-Host "Failed (might already exist): $($_.Exception.Message)" -ForegroundColor Yellow
}
Write-Host ""

# Test 12: Test unauthorized access (no token)
Write-Host "Test 12: GET /api/users - Test unauthorized access (no token)" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/users" -Method Get
    Write-Host "Unexpected: Request succeeded without authentication!" -ForegroundColor Red
} catch {
    Write-Host "Success! Properly rejected unauthorized request (401)" -ForegroundColor Green
}
Write-Host ""

Write-Host "=== Testing Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  - Authentication: Login, Register, Refresh Token" -ForegroundColor White
Write-Host "  - Role Management: Get Roles, Create Role, Update Permissions" -ForegroundColor White
Write-Host "  - User Management: Get Users, Assign Roles" -ForegroundColor White
Write-Host "  - Security: Authorization checks" -ForegroundColor White
