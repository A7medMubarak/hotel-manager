# Production Smoke Test

Run after every deploy. No frontend needed — PowerShell only.
Production URL is pre-filled below.

```powershell
$base = "https://hotel-manager.runasp.net"

# 1. Liveness — expect "Healthy" (HTTP 200)
Invoke-RestMethod "$base/health"

# 2. Login — expect token + role + expiresAt (HTTP 200)
$login = Invoke-RestMethod "$base/api/auth/login" -Method Post `
  -ContentType "application/json" `
  -Body '{"username":"admin","password":"<admin-password>"}'
$headers = @{ Authorization = "Bearer $($login.token)" }

# 3. Authenticated list — expect HTTP 200
Invoke-RestMethod "$base/api/rooms" -Headers $headers

# 4. Wrong password — expect HTTP 401, no details leaked
try {
  Invoke-RestMethod "$base/api/auth/login" -Method Post `
    -ContentType "application/json" `
    -Body '{"username":"admin","password":"wrong"}'
} catch { "OK: $($_.Exception.Response.StatusCode)" }

# 5. Missing entity — expect HTTP 404 with ProblemDetails
try {
  Invoke-RestMethod "$base/api/rooms/2147483647" -Headers $headers
} catch { "OK: $($_.Exception.Response.StatusCode)" }

# 6. Unauthenticated access — expect HTTP 401
try {
  Invoke-RestMethod "$base/api/rooms"
} catch { "OK: $($_.Exception.Response.StatusCode)" }
```

## First-deploy credential rotation (do once, immediately)

The seed creates `admin` / `Admin123!` and `employee` / `Employee123!` — both are
in git history, so treat them as public. After first login, rotate via self-service:

```powershell
Invoke-RestMethod "$base/api/auth/password" -Method Patch `
  -Headers $headers -ContentType "application/json" `
  -Body '{"currentPassword":"Admin123!","newPassword":"<new-strong-password>"}'
# expect HTTP 204. Repeat for the employee account, then store both in a password manager.
```

## Rate-limit spot check

Fire 11 rapid logins — the 11th must return **429** (`Login` policy: 10 req/min).
If *every* request 429s from different client IPs, `ForwardedHeaders` is
misconfigured (all clients share the proxy IP bucket).

## Swagger

Swagger UI is intentionally **disabled in Production** (`Program.cs` gates it to
Development). Test with the steps above or Postman instead.
