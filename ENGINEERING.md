# Hotel Manager — Engineering Case Study

This document walks through the reasoning behind Hotel Manager's architecture: the decisions, the tradeoffs, and what's still missing for an actual commercial deployment. For setup and a feature overview, see the [README](README.md).

## Background

I worked as a hotel receptionist before moving into software development — reservations, check-ins and check-outs, room availability, payments, the daily routines that keep a front desk running. That's the reason this project exists in the form it does: not as a generic booking CRUD app, but as a system built around the rules a real front desk actually works under.

The clearest example is the **Business Day**. Most systems treat "today" as whatever the calendar says. Hotels don't — the operational day continues past midnight until the night audit closes it out, typically around standard checkout time. Hotel Manager's Business Day decouples "the day for reporting purposes" from "the calendar date," so occupancy, revenue, and outstanding-balance reports line up with how the hotel actually operates rather than how a clock does.

## Architecture

Four layers, Clean Architecture, dependencies point inward:

```
React 19 (TypeScript)
        │  HTTP / REST
        ▼
ASP.NET Core API        controllers, JWT auth, exception handling
        │
        ▼
Application              use cases, DTOs, validation, orchestration
        │
        ▼
Domain                   entities, enums, business language
        ▲
        │
Infrastructure            EF Core, SQL Server, JWT generation
```

The Domain layer has zero dependency on ASP.NET Core, EF Core, or SQL Server — it's the one place that represents the business rather than the technology. Infrastructure depends on Domain, not the other way around, so persistence and auth mechanics can change without touching business logic.

### Request lifecycle: creating a booking

1. React sends `POST /api/bookings` with a Bearer token attached by an Axios interceptor.
2. ASP.NET Core validates the JWT (signature, expiry, issuer, audience) before the request reaches the controller.
3. `BookingsController` — thin, no business logic — hands off to `IBookingService`.
4. `BookingService` orchestrates the use case: calls `BookingAvailabilityService` to confirm the room is free and non-overlapping, calls `BookingQueryService` for any read-only lookups it needs, then applies the remaining rules (chronological dates, primary guest present, room not under maintenance).
5. On success, EF Core persists through `IApplicationDbContext`; the controller returns a `BookingDto` — never the EF entity — with `201 Created`.

Read paths (search, filtering, `GetById`) go through `BookingQueryService` and use `AsNoTracking()`, since they never modify data.

## Key Decisions

### Splitting BookingService by responsibility, not by convention

`BookingService` originally did everything — CRUD, availability checks, searching, filtering, mapping, business validation — in one class that had grown past 340 lines. That's a common shape for a service that grows one feature at a time without anyone stepping back to look at it.

It's now three services with one job each:

| Service | Responsibility |
|---|---|
| `BookingService` | Coordinates the booking use cases — create, extend, complete, cancel |
| `BookingQueryService` | Read-only: search, filter, get-by-id, active/completed lists |
| `BookingAvailabilityService` | Room availability and date-overlap validation |

The existing unit tests kept passing through the whole refactor, which is really the point of having them — the split was a maintainability change, not a behavior change, and the test suite is what let me be confident of that rather than a manual click-through.

### JWT lifetime tied to a shift, not a security default

The access token expires after 8 hours instead of the shorter lifetimes most JWT tutorials recommend. That's deliberate: a receptionist authenticates once at the start of a shift and shouldn't be interrupted mid-shift by a token timing out, and the token naturally expiring at shift's end means the next employee authenticates with their own credentials rather than inheriting an open session.

The tradeoff: there's no refresh-token flow yet, so a token can't be silently renewed — it's a flat 8-hour window. For a system with longer-lived sessions or a mobile client, I'd add refresh tokens. For a single-shift front-desk tool, the simpler model matched the actual usage pattern.

### Cancellation is Owner-only

| Action | Employee | Owner |
|---|:---:|:---:|
| Create / extend / complete booking | ✅ | ✅ |
| Register payment | ✅ | ✅ |
| Manage guests & rooms | ✅ | ✅ |
| Cancel booking | ❌ | ✅ |
| Manage employee accounts | ❌ | ✅ |

Every other booking operation is available to any authenticated employee. Cancellation isn't, because cancelling a paid or partially-paid reservation carries financial and dispute implications that the rest of the workflow doesn't. It's a business policy enforced in code rather than left to staff discipline.

### SQL Server over PostgreSQL — a deployment-driven pivot, not a technical one

The original plan was PostgreSQL on Render. Hosting-cost constraints during deployment made MonsterASP.NET's free tier — Windows/IIS with native SQL Server — the more practical option, so production runs on SQL Server instead.

Because the Application layer only ever depends on `IApplicationDbContext`, and EF Core provides the provider abstraction underneath it, the switch was a configuration change, not a rewrite. PostgreSQL support is still there if the hosting situation changes again. This is the kind of decision that looks bad in isolation ("didn't use the database in the plan") and is actually just normal engineering — infrastructure served the architecture, not the other way around.

### No repository pattern

A lot of Clean Architecture examples wrap EF Core in a generic `IRepository<T>`. This project doesn't — `IApplicationDbContext` is the abstraction the Application layer depends on, and EF Core's `DbSet<T>` already behaves like a repository (change tracking, LINQ querying, unit-of-work via `SaveChangesAsync`). Adding a repository on top of that mostly adds pass-through methods without adding testability or decoupling that isn't already there, so I skipped it.

Also skipped for the same reason: MediatR/full CQRS, microservices, and premature caching — none of them solve a problem this project actually has at its current scale.

### The rest of the stack, briefly

| Choice | Why |
|---|---|
| ASP.NET Core 8 | Built-in DI, strong auth support, one runtime for the whole backend |
| EF Core 8 | LINQ + change tracking without hand-written SQL for a schema this size |
| BCrypt.Net | Salted, deliberately slow password hashing |
| FluentValidation | Keeps validation rules out of controllers and services, in one place, reusable |
| React 19 + TypeScript | Component model fits an admin dashboard; static typing pays off as pages accumulate |
| i18next + RTL | Target users are Arabic-speaking hotel staff — not optional for this audience |

## Business Rules Enforced

| Rule | Why it exists |
|---|---|
| Rooms can't have overlapping active bookings | Prevents double-booking |
| Extensions re-validate availability | Prevents an extension from silently creating a conflict |
| Rooms under maintenance can't be booked | Keeps unavailable rooms out of the booking flow |
| Check-out must be after check-in, minimum one night | Basic chronological sanity |
| Every booking has a primary guest | Someone is accountable for the reservation |
| Payments only attach to active bookings | Prevents orphaned financial records |
| Outstanding balance is calculated, never stored | One source of truth — total minus payments, always |
| A booking can't complete with a balance outstanding | Checkout can't happen before the bill is settled |
| Only Owners can cancel | Cancellation carries financial/dispute weight the other actions don't |
| Reports use the Business Day, not the calendar day | Matches how the hotel's operational day actually works |

## Deployment Pipeline

```
push to main
     │
     ├──► GitHub Actions (windows-latest)
     │        dotnet publish → WebDeploy → MonsterASP.NET
     │
     └──► Vercel
              vite build → global CDN
```

Backend and frontend deploy independently, with no manual step on either side. On startup, the API creates the database and seeds a default Owner account automatically (`EnsureCreatedAsync()`), which is why a fresh clone needs no manual setup beyond a connection string — convenient for reviewers, and explicitly not how I'd handle schema changes against a real production database (see below).

The one current gap: the GitHub Actions pipeline builds and publishes but doesn't run the test suite yet. Tests pass locally; CI isn't gating on them.

## Testing

75 xUnit tests, run against EF Core's InMemory provider so the suite has no external dependencies and stays fast and deterministic. Coverage focuses on where the actual risk is — application services, FluentValidation validators, and the shared calculation helpers (`BookingCalculator`, `BusinessDateHelper`) — rather than chasing a coverage number.

The tests earned their keep during the `BookingService` refactor above: splitting one service into three is exactly the kind of change that's easy to get subtly wrong, and the existing suite staying green through it was the real confidence check, not code review.

**Not yet covered**, and next on the list: integration tests against a real database, controller-level API tests, and frontend component tests.

## What's Not Here Yet

Being upfront about the gap between "portfolio-complete" and "commercial-production-ready" — this is what I'd add before this ran a real hotel's data:

- **EF Core Migrations** instead of `EnsureCreatedAsync()` at startup. The current approach is genuinely convenient for reviewers — clone, run, done — but it's not how you evolve a production schema safely.
- **Refresh tokens**, so sessions can outlive a single 8-hour window without a flat re-login.
- **HTTP-only secure cookies** instead of `localStorage` for the JWT, to reduce XSS exposure. `localStorage` is fine for a portfolio project talking to its own API; it's not what I'd choose for a system handling real guest data.
- **Automated tests running in CI**, not just locally.
- Health check endpoints, centralized monitoring/logging, and a backup/disaster-recovery story — none of which matter for a demo, all of which matter the first time something breaks in production.

None of this was an oversight — it's scope. A single-hotel demo doesn't need Kubernetes. But knowing the difference between "works for this project" and "ready for a real one" is the actual point of listing it.

## Lessons Learned

**Domain knowledge mattered more than any framework.** The parts of this project that make it more than a tutorial clone — Business Day, guest-per-booking modeling, Owner-only cancellation — came from having actually worked a front desk, not from reading about hotel software.

**Tests are what make refactoring safe, not code review.** The `BookingService` split only felt low-risk because the suite confirmed it, not because the change looked simple on paper.

**Simplicity is a decision, not a default.** Skipping the repository pattern and CQRS took more deliberate judgment than adding them would have — it's easy to reach for abstraction, harder to justify leaving it out.

**Deployment is part of the engineering, not a step after it.** The Render → MonsterASP.NET, PostgreSQL → SQL Server pivot happened because of real hosting constraints, and the architecture only absorbed it cleanly because persistence was already behind an abstraction.
