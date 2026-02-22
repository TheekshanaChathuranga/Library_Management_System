# BorrowingReturnsService

A .NET 9.0 microservice for managing book borrowing and returns in a library system. Integrates with UserIdentityService, CatalogService, and InventoryService to provide a complete borrowing workflow with user validation and channel-aware inventory management (Physical and Digital).

## Features

- ✅ **User Validation**: Validates user existence and active status before borrowing
- ✅ **Book Borrowing**: Physical and Digital channel support with automatic due date calculation (14-day period)
- ✅ **Book Returns**: Return processing with late fee calculation ($1 per day overdue)
- ✅ **Late Fee Management**: Automatic calculation and payment tracking
- ✅ **UserIdentityService Integration**: JWT-based user authentication and profile retrieval
- ✅ **CatalogService Integration**: Book metadata and availability synchronization
- ✅ **InventoryService Integration**: Real-time stock management by channel
- ✅ **Caching**: Redis-based caching for improved performance
- ✅ **Authentication**: JWT Bearer token authentication
- ✅ **Database**: PostgreSQL with Entity Framework Core 9.0.1
- ✅ **API Documentation**: Swagger/OpenAPI
- ✅ **Docker Support**: Multi-container deployment with docker-compose

## Prerequisites

- .NET 9.0 SDK
- PostgreSQL
- Redis
- Docker (optional, for containerized deployment)
- UserIdentityService (running on port 5192)
- CatalogService (running on port 5000)
- InventoryService (running on port 5002)

## Configuration

Update `appsettings.json` with your connection strings and service URLs:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=borrowing_returns_db;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Issuer": "UserIdentityService",
    "Audience": "BorrowingReturnsService",
    "Key": "your-secret-key-here"
  },
  "ServiceUrls": {
    "UserIdentityService": "http://localhost:5192",
    "CatalogService": "http://localhost:5000",
    "InventoryService": "http://localhost:5002"
  },
  "UserIdentity": {
    "ServiceAccount": {
      "Email": "admin@library.local",
      "Password": "ChangeMe!123"
    },
    "TokenCacheSeconds": 3600
  },
  "CatalogService": {
    "Username": "admin",
    "Password": "admin123"
  }
}
```

## Running Locally

### 1. Start PostgreSQL and Redis

```powershell
# Using Docker
docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:15
docker run -d --name redis -p 6379:6379 redis:7
```

### 2. Apply Database Migrations

```powershell
cd BorrowingReturnsService
dotnet ef database update
```

### 3. Run the Service

```powershell
dotnet run
```

The service will be available at `https://localhost:5006` (or the port configured in `launchSettings.json`).

## Running with Docker Compose

```powershell
docker-compose up
```

This will start:
- BorrowingReturnsService on port 5005
- PostgreSQL on port 5433
- Redis on port 6380

## API Endpoints

### Borrowing Operations

#### Borrow a Book
```http
POST /api/borrowing
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "bookId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "channel": 0
}
```

**Channel Values:**
- `0` = Physical
- `1` = Digital

**Response:** `201 Created` with BorrowingDto

**Validation:**
- Validates user exists in UserIdentityService
- Checks user IsActive status (blocks if inactive)
- Checks for unpaid late fees (blocks borrowing if found)
- Verifies book exists in CatalogService
- Validates inventory availability by channel in InventoryService
- Adjusts inventory (decrements stock)
- Updates catalog availability if last physical copy

#### Return a Book
```http
POST /api/borrowing/{id}/return
Authorization: Bearer {jwt-token}
```

**Response:** `200 OK` with ReturnDto

**Business Logic:**
- Marks borrowing as returned
- Increments inventory in InventoryService
- Updates catalog availability to true
- Calculates late fees if overdue ($1/day)

#### Get Borrowing by ID
```http
GET /api/borrowing/{id}
Authorization: Bearer {jwt-token}
```

**Response:** `200 OK` with BorrowingDto

#### Get User Borrowings
```http
GET /api/borrowing/user/{userId}
Authorization: Bearer {jwt-token}
```

**Response:** `200 OK` with List<BorrowingDto>

### Late Fee Operations

#### Get All Late Fees
```http
GET /api/latefee
Authorization: Bearer {jwt-token}
```

#### Get User Late Fees
```http
GET /api/latefee/user/{userId}
Authorization: Bearer {jwt-token}
```

#### Pay Late Fee
```http
POST /api/latefee/{id}/pay
Authorization: Bearer {jwt-token}
```

## Architecture

The service follows a layered architecture:

- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic and caching
- **Repositories**: Data access layer
- **Models**: Domain entities
- **DTOs**: Data transfer objects

## Integration with External Services

### UserIdentityService

**Purpose**: User authentication and profile management

**Endpoints Used:**
- `POST /api/auth/login` - Service account authentication (acquires JWT token)
- `GET /api/users/{userId}` - Retrieve user profile and active status

**Authentication**: JWT Bearer token (acquired via service account login)

**Token Management**:
- Service account: `admin@library.local` / `ChangeMe!123`
- Tokens cached in memory for 3600 seconds (configurable)
- Auto-refresh 60 seconds before expiry
- Thread-safe acquisition using SemaphoreSlim

### CatalogService

**Purpose**: Manages book metadata and availability

**Endpoints Used:**
- `GET /api/Books/{id}` - Retrieve book details
- `PATCH /api/Books/{id}/availability` - Update availability status

**Authentication**: HTTP Basic Auth (configured in appsettings)

### InventoryService

**Purpose**: Tracks physical and digital book inventory

**Endpoints Used:**
- `GET /api/Inventory/{bookId}` - Get inventory summary
- `POST /api/Inventory/{bookId}/borrow` - Decrement stock
- `POST /api/Inventory/{bookId}/return` - Increment stock

**Authentication**: None

---

See [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) for detailed integration documentation, including data flows, error handling, and test scenarios.

## Domain Model

### Borrowing
```csharp
public class Borrowing
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public DateTime BorrowedAt { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsReturned { get; set; }
    public BorrowChannel Channel { get; set; } // Physical or Digital
}
```

### BorrowChannel
```csharp
public enum BorrowChannel
{
    Physical = 0,
    Digital = 1
}
```

### LateFee
```csharp
public class LateFee
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BorrowingId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Business Rules

### Borrowing Rules
1. User must exist in UserIdentityService
2. User account must be active (IsActive = true)
3. Users cannot borrow if they have unpaid late fees
4. Physical books require available physical inventory
5. Digital books require available digital licenses
6. Due date is set to 14 days from borrow date
7. Catalog availability is updated when last physical copy is borrowed

### Return Rules
1. Returns are always accepted regardless of due date
2. Late fees are calculated at $1 per day overdue
3. Inventory is incremented upon return
4. Catalog availability is set to true when book is returned

### Late Fee Rules
1. Calculated as: `(Return Date - Due Date) * $1.00`
2. Only generated if book is returned late
3. Must be paid before user can borrow again

## HTTP Client Configuration

All HTTP clients use standard configuration with 30-second timeouts. Resilience policies (retry, circuit breaker) are not currently implemented. Consider adding Polly for production deployments.

## Development

### Project Structure

```
BorrowingReturnsService/
├── Controllers/
│   ├── BorrowingController.cs      # Main borrowing/return endpoints
│   └── LateFeeController.cs        # Late fee management
├── Data/
│   ├── BorrowingReturnsDbContext.cs
│   └── BorrowingReturnsDbContextFactory.cs
├── Dtos/
│   ├── BorrowingDto.cs
│   ├── CreateBorrowingDto.cs
│   ├── ReturnDto.cs
│   ├── LateFeeDto.cs
│   ├── CatalogBookDto.cs           # CatalogService integration
│   ├── InventorySummaryDto.cs      # InventoryService integration
│   └── AdjustInventoryDto.cs       # InventoryService requests
├── Models/
│   ├── Borrowing.cs                # Core domain entity
│   ├── BorrowChannel.cs            # Physical/Digital enum
│   ├── Return.cs
│   └── LateFee.cs
├── Repositories/
│   ├── IBorrowingRepository.cs
│   ├── BorrowingRepository.cs
│   ├── ILateFeeRepository.cs
│   └── LateFeeRepository.cs
├── Services/
│   ├── ICacheService.cs
│   ├── RedisCacheService.cs
│   ├── ILateFeeService.cs
│   ├── LateFeeService.cs
│   ├── IUserIdentityClient.cs      # Typed HTTP client
│   ├── UserIdentityClient.cs       # UserIdentityService communication
│   ├── ICatalogClient.cs           # Typed HTTP client
│   ├── CatalogClient.cs            # CatalogService communication
│   ├── IInventoryClient.cs         # Typed HTTP client
│   └── InventoryClient.cs          # InventoryService communication
├── Migrations/
│   ├── 20251117115304_InitialCreate.cs
│   └── 20251119025215_AddBorrowChannel.cs
├── Program.cs                      # DI, middleware, Polly policies
├── appsettings.json
└── docker-compose.yml
```

### Adding New Features

1. Create new models in the `Models` folder
2. Update the `DbContext` to include the new entities
3. Create migrations: `dotnet ef migrations add YourMigrationName`
4. Apply migrations: `dotnet ef database update`
5. Implement repository and service layers
6. Add controller endpoints

## Troubleshooting

### Common Issues

#### "User not found"
- Verify UserIdentityService is running on port 5192
- Check user ID exists in UserIdentityService database
- Review UserIdentityService logs for errors

#### "User account is not active"
- User exists but IsActive flag is false
- Check user status in UserIdentityService
- Activate user account if needed

#### "Failed to authenticate with UserIdentityService"
- Verify service account credentials in appsettings.json
- Check UserIdentityService has seeded admin account
- Review UserIdentityService authentication logs

#### "Book not found in catalog"
- Verify CatalogService is running on port 5000
- Check Basic Auth credentials in configuration
- Ensure book exists in CatalogService database

#### "Book unavailable for borrowing"
- Verify inventory has available copies for the requested channel
- Check InventoryService is responding on port 5002
- Review inventory quantities in InventoryService

#### Database Connection Issues
- Verify PostgreSQL is running
- Check connection string in appsettings.json
- Ensure database exists and migrations are applied

#### Redis Connection Issues
- Verify Redis is running on the configured host/port
- Check Redis connection string in appsettings.json
- Test Redis connectivity: `redis-cli ping`

## Testing

The service includes Swagger UI for API testing. Navigate to `/swagger` when the service is running.

For integration testing scenarios, see [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md#testing-integration).

## Monitoring

### Logging
The service logs to console with the following levels:
- **Information**: API requests, successful operations
- **Warning**: Retry attempts, validation failures
- **Error**: Exceptions, circuit breaker openings

### Health Checks
- Database connectivity (PostgreSQL)
- Redis connectivity
- External service availability (CatalogService, InventoryService)

## License

MIT

---

**Last Updated**: November 2025  
**Version**: 1.0.0
