# Hotel Manager

**Hotel Management System** — a full-stack application built with .NET 8 and React that manages the complete lifecycle of hotel operations: room inventory, guest check-ins/check-outs, booking reservations, payment tracking, and operational reporting.

Designed for **hotel front-desk operations**, this system models real-world hospitality business rules — not generic CRUD.

---

## What This Project Demonstrates

### 1. Clean Architecture & Separation of Concerns

The solution is split into **4 backend projects + 1 frontend** with strict dependency rules:

```
HotelManager.Domain          ← Entities, enums, repository interfaces (zero dependencies)
HotelManager.Application     ← Services, DTOs, validators, business logic (depends only on Domain)
HotelManager.Infrastructure  ← EF Core, persistence, JWT security, migrations (depends on Application)
HotelManager.API             ← Controllers, middleware, DI config (depends on Infrastructure)
frontend/                    ← React SPA (Vite + Tailwind + React Router)
```

The controller never touches `DbContext`. Database schema changes don't leak into the API layer. Each layer is independently testable — exactly how production .NET teams structure their code.

### 2. RESTful API Design — 7 Controllers, One Convention

Every controller follows the same consistent pattern:

| Aspect | Convention | Example |
|---|---|---|
| Routes | `api/<plural>` | `api/bookings`, `api/guests` |
| GET by ID | → 200 with DTO, 404 if missing | Every controller |
| GET list | → 200 with paginated result | 4 controllers |
| POST | → 201 with Location header | 5 controllers |
| PATCH action | → 204 No Content | `extend`, `complete`, `cancel` |
| PUT | → 200 with updated DTO | `Guests`, `Rooms` |
| DELETE | → 204 No Content | 2 controllers |

Consistent API patterns mean lower maintenance cost, predictable error handling, and reliable contracts for the frontend SPA.

### 3. Endpoints Overview

#### Auth (`/api/auth`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/login` | Anonymous (rate-limited) | Authenticate and receive JWT |
| PATCH | `/password` | Authenticated | Change current user password |

#### Bookings (`/api/bookings`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/` | Authenticated | List bookings with filtering & pagination |
| GET | `/{id}` | Authenticated | Get booking detail with payments |
| GET | `/search?q=` | Authenticated | Search by room number or guest name |
| POST | `/` | Authenticated | Create a new booking |
| PATCH | `/{id}/extend` | Authenticated | Extend checkout date |
| PATCH | `/{id}/complete` | Authenticated | Mark booking as completed |
| PATCH | `/{id}/cancel` | Owner only | Cancel a booking |

#### Guests (`/api/guests`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/` | Authenticated | List guests with filtering & pagination |
| GET | `/{id}` | Authenticated | Get guest details |
| GET | `/search?q=` | Authenticated | Search by name or National ID |
| POST | `/` | Authenticated | Register a new guest |
| PUT | `/{id}` | Authenticated | Update guest information |

#### Rooms (`/api/rooms`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/` | Authenticated | List rooms with real-time status |
| GET | `/{id}` | Authenticated | Get room details |
| POST | `/` | Owner only | Add a new room |
| PUT | `/{id}` | Owner only | Update room configuration |
| PATCH | `/{id}/maintenance` | Owner only | Toggle maintenance mode |

#### Payments (`/api/payments`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/booking/{bookingId}` | Authenticated | Get all payments for a booking |
| POST | `/` | Authenticated | Record a payment |
| DELETE | `/{id}` | Owner only | Remove a payment |

#### Reports (`/api/reports`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/daily` | Owner only | Today's operational snapshot |
| GET | `/outstanding` | Owner only | All active bookings with unpaid balance |
| GET | `/weekly` | Owner only | Weekly revenue summary |
| GET | `/monthly` | Owner only | Monthly revenue summary |

#### Users (`/api/users`)
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/` | Owner only | List all staff accounts |
| POST | `/` | Owner only | Create employee account |
| DELETE | `/{id}` | Owner only | Remove staff account |

### 4. Authentication & Authorization — JWT with Role-Based Access

```
POST /api/auth/login  →  { token, role, expiresAt }
PATCH /api/auth/password  →  204 No Content
```

- **Passwords**: BCrypt (work factor 10+) — never stored in plaintext
- **JWT tokens**: HMAC-SHA256 signed, configurable expiry (default 8 hours)
- **Two roles**: `Owner` (full access) and `Employee` (operational access)
- **Access control**: `[Authorize(Roles = "Owner")]` on sensitive endpoints
- **Auto seed**: First run creates `admin` / `Admin123!` — password change enforced on first login

### 5. Input Validation — FluentValidation + AutoValidation

Every request is automatically validated before reaching the controller — 9 validators, zero manual wiring:

```csharp
// Single line registers ALL validators in the assembly:
services.AddValidatorsFromAssemblyContaining<CreateRoomRequestValidator>();
```

Invalid requests never hit business logic. Validators are decoupled, reusable, and testable in isolation.

### 6. Domain Logic — Hotel Operations Engine

This system encodes actual hotel management workflows:

- **Booking lifecycle**: Active → Completed / Cancelled — with state transition guards
- **Room availability**: Overlap detection prevents double-booking
- **Maintenance mode**: Rooms with active bookings cannot enter maintenance
- **Business day**: Noon-to-noon window defines the operational day (accommodates late check-outs)
- **Financial calculations**: Night count, total cost, total paid, outstanding balance — centralized in `BookingCalculator`
- **Guest management**: National ID uniqueness enforcement, phone/address tracking
- **Dashboard reporting**: Daily snapshot (check-ins, check-outs, revenue, outstanding), weekly/monthly aggregates, outstanding balance tracking

### 7. Global Exception Handling — Consistent ProblemDetails Responses

Every exception maps to a standard RFC 7807 `ProblemDetails` response:

| Exception | Status |
|---|---|
| `ArgumentException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `NotImplementedException` | 501 Not Implemented |
| Unhandled exceptions | 500 Internal Server Error (details never leaked to client) |

**500 errors never leak stack traces to the client** — the real exception is logged server-side, and the client receives only "An unexpected error occurred. Please try again later."

### 8. Rate Limiting — Login Brute-Force Protection

| Policy | Limit | Applies To |
|---|---|---|
| Login | 10 req/min per client | `POST /api/auth/login` |

Rate limiting is enforced at the middleware level — excess requests receive `429 Too Many Requests` without consuming server resources.

### 9. Structured Logging — Serilog with Rolling Files

```
logs/hotelmanager-20260617.log    ← General app activity (14 day retention)
```

Every request is observable. Log output includes timestamps, log levels, and structured message templates. Console output in development, rolling file sink in production.

### 10. Data Model — EF Core Code-First with Migrations

```
Users ──┐
         ├── Bookings ──── BookingGuests ──── Guests
         │       │
Rooms ───┘       │
                  └── Payments
```

- **6 entities**: `User`, `Room`, `Guest`, `Booking`, `BookingGuest`, `Payment`
- **Composite key**: `BookingGuest` uses `(BookingId, GuestId)` as its primary key
- **Restrict deletes**: All foreign keys use `DeleteBehavior.Restrict` — prevents accidental data loss
- **Unique constraints**: `Room.Number`, `Guest.NationalId`, `User.Username` — enforced at the database level
- **Value conversions**: Enums stored as integers (`BookingStatus`, `UserRole`, `BathroomType`)

### 11. Testing — Comprehensive Coverage

```
dotnet test
```

| Layer | Tests | Framework |
|---|---|---|
| Services | 42 tests | xUnit + Moq + FluentAssertions |
| Validators | 9 tests | xUnit + FluentAssertions |
| Common utilities | 6 tests | xUnit + FluentAssertions |
| **Total** | **57 tests** | All pass |

Test scenarios include:
- **Happy paths**: CRUD operations return correct data
- **Business rules**: duplicate National ID, overlapping bookings, invalid state transitions
- **Edge cases**: null inputs, short search queries, non-existent entities
- **Financial calculations**: night count, total cost, balance computation edge cases

### 12. Frontend SPA — React + Vite + Tailwind

The frontend is a mobile-first PWA built with modern React:

- **React Router v7** — declarative routing with protected routes
- **Axios client** — automatic JWT attachment, 401 interceptor redirects to login
- **Tailwind CSS v4** — utility-first styling, responsive design
- **Role-aware UI** — Owner-only sections (Reports) hidden from Employees
- **Pages**: Login, Bookings (list/detail/create), Guests (list/detail/create), Rooms, Reports

---

## Quick Start

```bash
# Prerequisites: .NET 8 SDK, SQL Server, Node.js 20+

# Clone & build backend
git clone https://github.com/A7medMubarak/hotel-manager.git
cd hotel-manager
dotnet build

# Setup database (update connection string in appsettings.json if needed)
dotnet ef database update --project src/HotelManager.Infrastructure --startup-project src/HotelManager.API

# Run API (Swagger at /swagger)
dotnet run --project src/HotelManager.API

# In a separate terminal — run frontend
cd frontend
npm install
npm run dev

# Default credentials
# Username: admin
# Password: Admin123!
```

---

## Project Structure

```
HotelManager/
├── src/
│   ├── HotelManager.Domain/          Entities, enums, repository interfaces
│   ├── HotelManager.Application/     Services, DTOs, validators, business logic
│   ├── HotelManager.Infrastructure/  EF Core, migrations, JWT, persistence
│   └── HotelManager.API/             Controllers, middleware, DI, config
├── tests/
│   └── HotelManager.Application.Tests/  Service, validator, and common tests
├── frontend/                         React SPA (Vite + Tailwind)
├── HotelManager.sln
├── AGENTS.md                         Architecture conventions & onboarding
└── README.md
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 8 (C# 12) |
| API Framework | ASP.NET Core Minimal + Controllers |
| ORM | Entity Framework Core 8 (SQL Server) |
| Auth | JWT Bearer (HMAC-SHA256) |
| Validation | FluentValidation 11 |
| Logging | Serilog |
| Testing | xUnit + Moq + FluentAssertions |
| Frontend | React 19 + Vite 8 + Tailwind 4 |
| HTTP Client | Axios |

---

**Built by [A7medMubarak](https://github.com/A7medMubarak)** — a production-grade hotel management system demonstrating clean architecture, domain-driven design, and full-stack .NET + React development.
