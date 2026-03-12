$baseUrl = "http://localhost:5151/api"

# 1. Login as Admin
$adminCreds = @{ Username = "admin"; Password = "admin123" } | ConvertTo-Json
$adminToken = (Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $adminCreds -ContentType "application/json").token
$adminHeaders = @{ "Authorization" = "Bearer $adminToken" }

Write-Host "✅ Logged in as Admin"

# 2. Check Roles and Users (Create Test Role)
$roleBody = @{ Name = "Atendente" } | ConvertTo-Json
try {
    $role = Invoke-RestMethod -Uri "$baseUrl/roles" -Method Post -Body $roleBody -ContentType "application/json" -Headers $adminHeaders -ErrorAction SilentlyContinue
    Write-Host "✅ Created Role Atendente (ID: $($role.id))"
}
catch {
    Write-Host "ℹ️ Role might already exist or failed."
    # Fetch roles
    $roles = Invoke-RestMethod -Uri "$baseUrl/roles" -Method Get -Headers $adminHeaders
    $role = $roles | Where-Object { $_.name -eq "Atendente" } | Select-Object -First 1
}

# 3. Create a User for Atendente
$userBody = @{
    Name     = "João Atendente"
    Username = "joao.atendente"
    Password = "password123"
    RoleId   = $role.id
} | ConvertTo-Json

try {
    $user = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method Post -Body $userBody -ContentType "application/json" -Headers $adminHeaders -ErrorAction SilentlyContinue
    Write-Host "✅ Created User joao.atendente"
}
catch {
    Write-Host "ℹ️ User joao.atendente might already exist."
}

# 4. Login as 'joao.atendente'
$limitedCreds = @{ Username = "joao.atendente"; Password = "password123" } | ConvertTo-Json
$limitedToken = (Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $limitedCreds -ContentType "application/json").token
$limitedHeaders = @{ "Authorization" = "Bearer $limitedToken" }
Write-Host "✅ Logged in as joao.atendente (Limited user)"

# 5. Access Restricted Endpoints
# Example: DELETE /api/products/1 (Should be forbidden)
Write-Host "Testing Access to DELETE /products/1 ..."
try {
    Invoke-RestMethod -Uri "$baseUrl/products/1" -Method Delete -Headers $limitedHeaders
    Write-Host "❌ VULNERABILITY: joao.atendente successfully deleted or accessed DELETE /products/1 without proper permissions." -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403) {
        Write-Host "✅ Permission enforcement works! Return 403 Forbidden as expected." -ForegroundColor Green
    }
    else {
        Write-Host "❌ VULNERABILITY/WARNING: Server returned $statusCode instead of 403 Forbidden for unauthorized access." -ForegroundColor Yellow
    }
}

Write-Host "============================"
Write-Host "Permission Test Execution Finished"
