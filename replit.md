# Task Management API

## Overview
This is a Task Management API built with ASP.NET Core 9.0. It provides endpoints for user authentication and task management with JWT-based authorization.

**Current State:** Fully functional and running on Replit

## Recent Changes (September 30, 2025)
- Migrated from SQL Server to SQLite for Replit compatibility
- Configured to run on 0.0.0.0:5000 (required for Replit)
- Removed HTTPS redirection for Replit environment
- Set up automatic database creation on startup using EnsureCreated()
- Configured deployment with autoscale for production
- Created workflow to run the API server

## Project Architecture

### Technology Stack
- **Framework:** ASP.NET Core 9.0
- **Database:** SQLite (file-based at taskmanagement.db)
- **Authentication:** JWT Bearer tokens with ASP.NET Identity
- **ORM:** Entity Framework Core 9.0
- **API Documentation:** Swagger/OpenAPI 3.0

### Project Structure
```
TaskManagementApi/
├── Context/
│   ├── Configurations/     # EF Core entity configurations
│   └── AppDbContext.cs     # Database context
├── Controllers/
│   ├── AccountController.cs    # User authentication endpoints
│   ├── TaskController.cs       # Task management endpoints
│   └── UserController.cs       # User profile endpoints
├── DTO/                    # Data transfer objects
├── Enums/                  # Priority and Status enums
├── Extensions/             # Extension methods for mapping
├── Model/                  # Entity models
├── Repositories/           # Data access layer
└── wwwroot/Uploads/        # User uploaded files
```

### Key Features
1. **User Authentication**
   - Register new users
   - Login with JWT token generation
   - Password management
   - Profile picture upload

2. **Task Management**
   - CRUD operations for tasks
   - Filter by status (Todo, InProgress, Done)
   - Filter by priority (Low, Medium, High)
   - Search by title
   - Due date tracking

3. **Security**
   - JWT Bearer token authentication
   - ASP.NET Identity for user management
   - CORS enabled for all origins

## Database
- **Type:** SQLite
- **Location:** TaskManagementApi/taskmanagement.db
- **Schema:** Automatically created on first run
- **Tables:** AspNetUsers, AspNetRoles, Tasks, and Identity framework tables

## API Endpoints

### Account
- POST `/api/Account/register` - Register a new user
- POST `/api/Account/login` - Login and get JWT token

### Task
- GET `/api/Task` - Get all tasks for authenticated user
- GET `/api/Task/{id}` - Get specific task
- POST `/api/Task` - Create new task
- PUT `/api/Task/{id}` - Update task
- DELETE `/api/Task/{id}` - Delete task
- GET `/api/Task/filter/status/{status}` - Filter by status
- GET `/api/Task/filter/priority/{priority}` - Filter by priority
- GET `/api/Task/search/{title}` - Search by title

### User
- GET `/api/User/profile` - Get user profile
- PUT `/api/User/name` - Update user name
- POST `/api/User/upload-photo` - Upload profile picture
- POST `/api/User/change-password` - Change password

## Configuration

### JWT Settings (appsettings.json)
- **Key:** secertkeycreatedbyme09421THISisDEfualt
- **Issuer:** https://localhost:7111
- **Audience:** https://localhost:7111

### CORS
- Configured to allow all origins, headers, and methods

## Running Locally
The API runs automatically via the "API Server" workflow on port 5000.

## Deployment
Configured for autoscale deployment:
- Build command: `dotnet build TaskManagementApi/TaskManagement.Api.csproj -c Release`
- Run command: `dotnet run --project TaskManagementApi/TaskManagement.Api.csproj --no-build -c Release`

## Access
- **Swagger UI:** http://[your-repl-url]/swagger
- **API Base URL:** http://[your-repl-url]/api

## Notes
- The database file (taskmanagement.db) is created automatically on first run
- Database is excluded from git (.gitignore)
- Static files are served from wwwroot folder
- Profile pictures are uploaded to wwwroot/Uploads/
