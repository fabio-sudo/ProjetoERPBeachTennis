$baseUrl = "http://localhost:5151/api"

$loginBody = @{
    Username = "admin"
    Password = "admin123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.token
$headers = @{ "Authorization" = "Bearer $token" }

Write-Host "✅ Login successful. Token obtained."

# 1. Open Cash Register
$openRegisterBody = @{
    OpeningAmount = 100.00
} | ConvertTo-Json

Invoke-RestMethod -Uri "$baseUrl/cashregister/open" -Method Post -Body $openRegisterBody -ContentType "application/json" -Headers $headers -ErrorAction SilentlyContinue | Out-Null
Write-Host "✅ Ensured cash register is open."

# 2. Create Plan
$planBody = @{
    Name         = "GymPass Pro"
    Price        = 150.00
    DurationDays = 30
    Description  = "Access to all courts"
    IsActive     = $true
} | ConvertTo-Json

$plan = Invoke-RestMethod -Uri "$baseUrl/plans" -Method Post -Body $planBody -ContentType "application/json" -Headers $headers
Write-Host "✅ Plan created: $($plan.name) with ID $($plan.id)"

# 3. Create Student
$studentBody = @{
    Name      = "Test Student"
    Phone     = "11999999999"
    Email     = "test@student.com"
    PlanName  = "GymPass Pro"
    StartDate = (Get-Date).ToString("yyyy-MM-dd")
} | ConvertTo-Json

$student = Invoke-RestMethod -Uri "$baseUrl/students" -Method Post -Body $studentBody -ContentType "application/json" -Headers $headers
Write-Host "✅ Student created: $($student.name) with ID $($student.id)"

# 4. Create Subscription
$subBody = @{
    StudentId = $student.id
    PlanId    = $plan.id
    AutoRenew = $true
} | ConvertTo-Json

try {
    $subscription = Invoke-RestMethod -Uri "$baseUrl/subscriptions" -Method Post -Body $subBody -ContentType "application/json" -Headers $headers
    Write-Host "✅ Subscription created with ID $($subscription.id)"
}
catch {
    Write-Host "❌ Failed to create subscription."
    $responseStream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($responseStream)
    $errorResponseBody = $reader.ReadToEnd()
    Write-Host "Error details: $errorResponseBody"
    exit
}

# 5. Fetch Pending Payments
$payments = Invoke-RestMethod -Uri "$baseUrl/payments/student/$($student.id)" -Method Get -Headers $headers
$pendingPayment = $payments | Where-Object { $_.status -eq "Pending" } | Select-Object -First 1

if ($pendingPayment) {
    Write-Host "✅ Pending payment found: $($pendingPayment.id) for $($pendingPayment.amount)"
    
    # 6. Process Payment
    $payBody = @{
        PaymentId     = $pendingPayment.id
        PaymentMethod = "PIX"
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$baseUrl/payments/pay" -Method Post -Body $payBody -ContentType "application/json" -Headers $headers | Out-Null
    Write-Host "✅ Payment Processed successfully"
}
else {
    Write-Host "❌ No pending payment generated."
}

# 7. Verify Cash Register Transactions
$currentCash = Invoke-RestMethod -Uri "$baseUrl/cashregister/current" -Method Get -Headers $headers
$paymentInHistory = $currentCash.transactions | Where-Object { $_.description -match "GymPass Pro" }

if ($paymentInHistory) {
    Write-Host "✅ Transaction successfully found in Cash Register!"
    Write-Host "   -> $($paymentInHistory.description) : $($paymentInHistory.amount)"
}
else {
    Write-Host "❌ Transaction NOT found in Cash Register."
}

Write-Host "============================"
Write-Host "Test Execution Finished"
