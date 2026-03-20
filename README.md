# Todo API

A RESTful Todo API built with ASP.NET Core 10, Entity Framework Core, and JWT authentication.

## Features
- ✅ JWT Authentication with refresh tokens
- ✅ Role-based authorization (Admin/User)
- ✅ CRUD operations for Todo items
- ✅ User profile management
- ✅ Refresh token rotation
- ✅ Swagger/OpenAPI documentation

## Tech Stack
- .NET 10
- Entity Framework Core
- SQL Server or Mysql
- JWT Authentication
- Swagger UI

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or full installation) or Mysql

### Installation
1. Clone the repository
2. Update connection string in `appsettings.json`
3. Create Initial Migration `dotnet ef migrations add InitialCreate`
4. Run migrations: `dotnet ef database update`
5. Run the app: `dotnet run`

---
