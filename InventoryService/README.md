# Inventory Service

Inventory Service mirrors the architecture of the provided Catalog Service and is responsible for tracking the availability of both physical and digital copies of catalogued books.

## Features

- REST API built with ASP.NET Core 9 and controllers.
- Persists inventory data using Entity Framework Core with PostgreSQL (Npgsql).
- Keeps a movement history for each book to audit borrow/return operations.
- Automatically adjusts availability when copies are borrowed or returned.
- Interactive API documentation with Swagger UI.
- **Integrates with CatalogService** for book metadata - maintains proper microservice boundaries.
- Validates book existence before creating inventory records.

## Project Structure

```
InventoryService/
├── Controllers/InventoryController.cs
├── Data/
│   ├── InventoryDbContext.cs
│   └── DbInitializer.cs
├── Dtos/
├── Extensions/
├── Models/
├── Repositories/
├── Services/
├── Migrations/
├── appsettings*.json
└── README.md
```

## Getting Started

### Prerequisites
- .NET SDK 9.0+
- Docker Desktop (for PostgreSQL and Redis)

### Quick Start

**Option 1: Use the automated script (Recommended)**
```powershell
.\start.ps1
```

This will automatically:
- Start PostgreSQL and Redis containers (shared with CatalogService)
- Create the inventory database
- Apply migrations
- Start the API

**Option 2: Manual setup**
```powershell
# Start infrastructure (shares containers with CatalogService)
docker-compose up -d

# Create database
docker exec postgres-catalog psql -U postgres -c "CREATE DATABASE inventorydb_dev;"

# Apply migrations and run
dotnet restore
dotnet ef database update
dotnet run
```

### Testing the API

Run the test script to verify all endpoints:
```powershell
.\test-api.ps1
```

The application uses PostgreSQL and shares infrastructure with CatalogService. Update the connection string in `appsettings.json` or `appsettings.Development.json` if needed.

### API Endpoints

The service provides the following endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/inventory` | List all inventory records (paginated) |
| GET | `/api/inventory/{bookId}` | Get inventory for a specific book |
| POST | `/api/inventory` | Create new inventory record |
| PUT | `/api/inventory/{bookId}` | Update inventory totals |
| POST | `/api/inventory/{bookId}/borrow` | Borrow copies (decreases availability) |
| POST | `/api/inventory/{bookId}/return` | Return copies (increases availability) |

### Accessing the API

Once the service is running, you can access Swagger UI at:

- **HTTP (Development)**: http://localhost:5002/swagger ✅
- **HTTPS (Production)**: https://localhost:5003/swagger

For development, use HTTP on port 5002. HTTPS on 5003 is available but optional for production scenarios.

**Borrow a physical copy:**
```json
POST /api/inventory/{bookId}/borrow
{
  "quantity": 1,
  "channel": 0,
  "reference": "loan-123"
}
```

**Return a digital copy:**
```json
POST /api/inventory/{bookId}/return
{
  "quantity": 1,
  "channel": 1,
  "reference": "return-456"
}
```

**Channels:**
- `0` = Physical copies
- `1` = Digital copies

## Infrastructure

This service shares PostgreSQL and Redis with CatalogService:
- **PostgreSQL**: `localhost:5432` (databases: `catalogdb`, `inventorydb_dev`)
- **Redis**: `localhost:6379` (for future caching support)

### Microservice Dependencies

**InventoryService depends on CatalogService:**
- Book metadata (title, ISBN, author, genre) is fetched from CatalogService at runtime
- InventoryService only stores `BookId` references and inventory quantities
- API responses are enriched with book metadata from CatalogService
- Creating inventory requires that the book exists in CatalogService first

**Important:** CatalogService must be running for full functionality. If CatalogService is unavailable, inventory data will still be returned but without book metadata enrichment.

The database seeding creates inventory records for demonstration. Ensure corresponding books exist in CatalogService.
