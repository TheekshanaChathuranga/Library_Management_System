# Catalog Microservice for Library Management

A RESTful microservice built with ASP.NET Core 8.0 that manages book catalog data for a library management system.

## Features

- ✅ **CRUD Operations** - Create, Read, Update, Delete books
- ✅ **Advanced Search & Filtering** - Search by title, author, ISBN, genre, and availability
- ✅ **Pagination & Sorting** - Efficient data retrieval with customizable page sizes
- ✅ **Basic Authentication** - Secure API endpoints
- ✅ **PostgreSQL Database** - Robust data persistence
- ✅ **RESTful API Design** - Standard HTTP methods and status codes
- ✅ **Swagger/OpenAPI** - Interactive API documentation

## Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: Basic Authentication
- **Documentation**: Swagger/OpenAPI
- **Containerization**: Docker support

## Project Structure

```
CatalogService/
├── Authentication/
│   └── BasicAuthenticationHandler.cs    # Custom Basic Auth handler
├── Controllers/
│   ├── BooksController.cs               # Book API endpoints
│   └── WeatherForecastController.cs     # Sample controller
├── Data/
│   ├── CatalogDbContext.cs              # EF Core DbContext
│   └── DbInitializer.cs                 # Database seeding
├── Dtos/
│   ├── BookDto.cs                       # Book response model
│   ├── CreateBookDto.cs                 # Create book request model
│   ├── UpdateBookDto.cs                 # Update book request model
│   └── SearchResultDto.cs               # Search result wrapper
├── Models/
│   └── Book.cs                          # Book entity
├── Repositories/
│   ├── IBookRepository.cs               # Repository interface
│   └── BookRepository.cs                # Repository implementation
├── Program.cs                            # Application entry point
├── appsettings.json                     # Configuration
└── Dockerfile                           # Docker configuration
```

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 12+ (or Docker)
- Visual Studio 2022 / VS Code / Rider

## Getting Started

### 1. Install Required Packages

```powershell
cd c:\Users\pasin\source\repos\CatalogService\CatalogService
dotnet restore
```

### 2. Setup PostgreSQL

#### Option A: Local PostgreSQL Installation
Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "CatalogDatabase": "Host=localhost;Port=5432;Database=catalogdb;Username=postgres;Password=your_password"
}
```

#### Option B: Docker PostgreSQL
```powershell
docker run --name postgres-catalog -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=catalogdb -p 5432:5432 -d postgres:16
```

### 3. Run Database Migrations

```powershell
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

### 4. Run the Application

```powershell
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Books Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/books` | Search and filter books with pagination |
| GET | `/api/books/{id}` | Get book by ID |
| GET | `/api/books/isbn/{isbn}` | Get book by ISBN |
| POST | `/api/books` | Create a new book |
| PUT | `/api/books/{id}` | Update an existing book |
| DELETE | `/api/books/{id}` | Delete a book |
| PATCH | `/api/books/{id}/availability` | Update book availability |

### Search Parameters

- `q` - Search term (searches title, author, ISBN, genre)
- `author` - Filter by author name
- `genre` - Filter by genre
- `isbn` - Filter by ISBN
- `available` - Filter by availability (true/false)
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20, max: 100)
- `sortBy` - Sort field (title, author, isbn, genre, createdat, availability)
- `desc` - Sort descending (default: false)

## Authentication

All endpoints require Basic Authentication.

**Default Credentials:**
- Username: `admin`
- Password: `password123`

### Example Requests

#### Using PowerShell
```powershell
$credential = "admin:password123"
$bytes = [System.Text.Encoding]::UTF8.GetBytes($credential)
$base64 = [Convert]::ToBase64String($bytes)
$headers = @{ Authorization = "Basic $base64" }

# Search books
Invoke-RestMethod -Uri "https://localhost:5001/api/books?q=gatsby" -Headers $headers

# Get book by ID
Invoke-RestMethod -Uri "https://localhost:5001/api/books/{id}" -Headers $headers

# Create a book
$body = @{
    title = "New Book"
    author = "Author Name"
    isbn = "978-1-234-56789-0"
    genre = "Fiction"
    isAvailable = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/books" -Method Post -Headers $headers -Body $body -ContentType "application/json"
```

#### Using cURL
```bash
# Search books
curl -X GET "https://localhost:5001/api/books?q=tolkien&available=true" \
  -H "Authorization: Basic YWRtaW46cGFzc3dvcmQxMjM="

# Create a book
curl -X POST "https://localhost:5001/api/books" \
  -H "Authorization: Basic YWRtaW46cGFzc3dvcmQxMjM=" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "The Lord of the Rings",
    "author": "J.R.R. Tolkien",
    "isbn": "978-0-618-00222-1",
    "genre": "Fantasy",
    "isAvailable": true
  }'
```

## Sample Data

The database is automatically seeded with 10 sample books on first run:
- The Great Gatsby
- To Kill a Mockingbird
- 1984
- Pride and Prejudice
- The Catcher in the Rye
- The Hobbit
- Harry Potter and the Philosopher's Stone
- The Da Vinci Code
- The Alchemist
- Brave New World

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "CatalogDatabase": "Host=localhost;Port=5432;Database=catalogdb;Username=postgres;Password=postgres"
  },
  "BasicAuth": {
    "Username": "admin",
    "Password": "password123"
  }
}
```

### Environment Variables (Production)
```
ConnectionStrings__CatalogDatabase=Host=prod-db;Port=5432;Database=catalogdb;Username=dbuser;Password=****
BasicAuth__Username=****
BasicAuth__Password=****
```

## Docker Deployment

### Build Docker Image
```powershell
docker build -t catalog-service:latest .
```

### Run Container
```powershell
docker run -d -p 8080:8080 `
  -e ConnectionStrings__CatalogDatabase="Host=host.docker.internal;Port=5432;Database=catalogdb;Username=postgres;Password=postgres" `
  -e BasicAuth__Username="admin" `
  -e BasicAuth__Password="password123" `
  --name catalog-service `
  catalog-service:latest
```

## Database Schema

### Books Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | UUID | PRIMARY KEY |
| Title | VARCHAR(500) | NOT NULL, INDEXED |
| Author | VARCHAR(200) | NOT NULL, INDEXED |
| ISBN | VARCHAR(20) | NOT NULL, INDEXED |
| Genre | VARCHAR(100) | NOT NULL, INDEXED |
| IsAvailable | BOOLEAN | NOT NULL, INDEXED |
| CreatedAt | TIMESTAMP | NOT NULL |
| UpdatedAt | TIMESTAMP | NULL |

## Security Considerations

⚠️ **For Production Use:**

1. **Authentication**: Replace Basic Auth with OAuth2/JWT or integrate with identity providers (Azure AD, Auth0, etc.)
2. **Secrets Management**: Use Azure Key Vault, AWS Secrets Manager, or environment variables
3. **Password Hashing**: Implement proper password hashing (bcrypt, PBKDF2)
4. **HTTPS Only**: Enforce HTTPS in production
5. **Rate Limiting**: Add rate limiting to prevent abuse
6. **API Versioning**: Implement API versioning for future changes
7. **Input Validation**: Enhanced validation and sanitization
8. **Logging & Monitoring**: Implement structured logging and monitoring

## Future Enhancements

- [ ] Implement JWT authentication
- [ ] Add caching with Redis
- [ ] Implement event-driven architecture (RabbitMQ/Kafka)
- [ ] Add health check endpoints
- [ ] Implement API versioning
- [ ] Add rate limiting
- [ ] Implement comprehensive logging (Serilog)
- [ ] Add integration tests
- [ ] Implement CQRS pattern
- [ ] Add GraphQL support

## License

MIT License

## Support

For issues and questions, please create an issue in the repository.
