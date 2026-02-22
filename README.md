# Run LMS Microservices with Docker

This guide explains how to run the entire Library Management System (LMS) microservices suite using Docker Compose.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

## Quick Start

1. Open a terminal in the root directory of the project (where this file is located).
2. Run the following command to build and start all services:

   ```bash
   docker-compose up --build -d
   ```

3. Wait for a few minutes for all services to start and for the database to initialize.

## Services & Ports

| Service | Container Name | Port | Swagger UI |
|---------|----------------|------|------------|
| **Catalog Service** | `catalog-api` | 5001 | [http://localhost:5001/swagger](http://localhost:5001/swagger) |
| **Inventory Service** | `inventory-api` | 5002 | [http://localhost:5002/swagger](http://localhost:5002/swagger) |
| **User Identity Service** | `useridentity-api` | 5003 | [http://localhost:5003/swagger](http://localhost:5003/swagger) |
| **Borrowing Service** | `borrowingreturns-api` | 5004 | [http://localhost:5004/swagger](http://localhost:5004/swagger) |
| **Frontend** | `lms-frontend` | 3000 | [http://localhost:3000](http://localhost:3000) |
| **PostgreSQL** | `lms-postgres` | 5432 | - |
| **Redis** | `lms-redis` | 6379 | - |

## Stopping the Services

To stop all services and remove containers:

```bash
docker-compose down
```

To stop services and remove volumes (WARNING: this deletes all database data):

```bash
docker-compose down -v
```

## Troubleshooting

- **Database Connection Errors**: If services fail to connect to the database on first run, it might be because the database is still initializing. Docker Compose is configured to wait for the database to be healthy, but if issues persist, try restarting the specific service:
  ```bash
  docker-compose restart <service-name>
  ```
- **Port Conflicts**: Ensure ports 5001-5004, 5432, and 6379 are not in use by other applications.
