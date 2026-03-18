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
- SQL Server
- JWT Authentication
- Swagger UI

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or full installation)

### Installation
1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run migrations: `dotnet ef database update`
4. Run the app: `dotnet run`

### API Endpoints
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get tokens
- `GET /api/todoitems` - Get todo items
- `POST /api/todoitems` - Create todo item
- `PUT /api/todoitems/{id}` - Update todo item
- `DELETE /api/todoitems/{id}` - Delete todo item