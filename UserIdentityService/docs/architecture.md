# UserIdentityService Architecture Overview

## Goals
- Provide authentication and authorization APIs for librarian, member, and admin personas.
- Centralize role/permission management for other microservices (e.g., CatalogService).
- Expose JWT-based tokens consumable by downstream services.
- Persist user and role data in PostgreSQL; leverage Redis for caching session/permission lookups.

## Stack
- **Runtime:** .NET 8 ASP.NET Core Web API
- **Auth:** ASP.NET Core Identity with JWT bearer tokens (HMAC SHA256) and refresh tokens stored in PostgreSQL.
- **Database:** PostgreSQL via EF Core migrations. Connection string configurable via `UserIdentityDb` entry.
- **Caching:** Redis for storing short-lived session metadata, permission cache, and verification throttling counters.
- **Messaging:** (future) optional integration with service bus for audit events.

## Core Components
1. **Program / Startup**
   - Configures controllers, minimal APIs, Swagger, health checks.
   - Registers Postgres DbContext + Identity stores.
   - Adds Redis multiplexer and cache abstractions.
   - Configures JWT authentication and policy-based authorization.
2. **Domain Models**
   - `ApplicationUser` extends `IdentityUser<Guid>` with profile fields (display name, membershipId, librarianCode, etc.).
   - `ApplicationRole` extends `IdentityRole<Guid>` with permission descriptors.
3. **Persistence Layer**
   - EF Core migrations for identity schema + seed roles and default admin user.
   - Repository/services for user queries, role assignment, and permission retrieval with caching.
4. **Application Services**
   - `AuthService`: handles credential validation, token issuance, refresh token lifecycle, password reset flows.
   - `RoleService`: CRUD for roles/permissions, assignment to users.
   - `UserService`: onboarding librarians/members, profile updates, locking/unlocking accounts.
5. **API Layer**
   - `AuthController`: `/api/auth/register`, `/login`, `/refresh`, `/logout`.
   - `UsersController`: manage users, assign/remove roles (secured by `Admin` policy).
   - `RolesController`: manage role definitions and permissions.
6. **Caching Strategy**
   - Cache permission sets per user for 5 minutes keyed by `perm:{userId}`.
   - Cache refresh token metadata for quick validation.
7. **Security**
   - Enforce strong password policies.
   - Rate limit login endpoint via Redis-based sliding window (future enhancement).
   - All admin routes require `Admin` role; librarian-specific endpoints require `Librarian` role.

## Configuration
```jsonc
{
  "ConnectionStrings": {
    "UserIdentityDb": "Host=localhost;Port=5432;Database=useridentity;Username=postgres;Password=postgres"
  },
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "identity:"
  },
  "Jwt": {
    "Issuer": "catalog-suite",
    "Audience": "catalog-suite-clients",
    "SigningKey": "change-me",
    "AccessTokenMinutes": 30,
    "RefreshTokenDays": 7
  },
  "AdminSeed": {
    "Email": "admin@library.local",
    "Password": "ChangeMe!123",
    "DisplayName": "Default Admin"
  }
}
```

## Deployment Considerations
- Provide Dockerfile + docker-compose for Postgres + Redis + service.
- Leverage EF Core migrations at startup (`dotnet ef database update`).
- Expose health probes for liveness/readiness.

## Next Steps
- Scaffold actual project structure following this architecture.
- Implement caching abstractions and login throttling.
- Add unit/integration tests for auth flows.
