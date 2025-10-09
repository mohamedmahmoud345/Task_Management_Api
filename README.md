# Task Management API

A comprehensive RESTful API for managing tasks with user authentication, built with ASP.NET Core 9.0, Entity Framework Core, and JWT authentication.

## 📋 Table of Contents

- [Features](#features)
- [Technologies](#technologies)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Configuration](#configuration)
- [Testing](#testing)
- [Contributing](#contributing)

## ✨ Features

### Core Features
- **User Authentication & Authorization**: JWT-based authentication with ASP.NET Core Identity
- **Task Management**: Full CRUD operations for tasks
- **User Profile Management**: Upload profile pictures, change name, email, and password
- **Advanced Filtering**: Filter tasks by status, priority, and search by title
- **Pagination**: Paginated responses for better performance
- **Caching**: Memory cache implementation for improved response times
- **Rate Limiting**: Per-user and anonymous rate limiting to prevent abuse
- **Global Exception Handling**: Centralized error handling with detailed logging

### Task Features
- Create, read, update, and delete tasks
- Set task priority (Low, Medium, High)
- Track task status (Todo, In Progress, Done)
- Set due dates
- Filter by status and priority
- Search tasks by title
- Pagination support

### User Features
- User registration and login
- JWT token-based authentication
- Profile picture upload (JPG, PNG, max 10MB)
- Change username, email, and password
- View user profile

## 🛠 Technologies

- **Framework**: ASP.NET Core 9.0
- **Database**: SQL Server with Entity Framework Core 9.0
- **Authentication**: ASP.NET Core Identity with JWT Bearer tokens
- **Testing**: xUnit with Moq
- **API Documentation**: Swagger/OpenAPI
- **Caching**: In-Memory Cache
- **Rate Limiting**: Token Bucket Algorithm

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd TaskManagementApi
   ```

2. **Update the connection string**
   
   Edit `appsettings.json` and update the connection string:
   ```json
   "ConnectionStrings": {
     "conStr": "server=YOUR_SERVER;database=TManagementDb;integrated security=true;trustservercertificate=true"
   }
   ```

3. **Create the database**
   
   Run the following commands in the Package Manager Console or terminal:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run --project TaskManagementApi
   ```

5. **Access Swagger UI**
   
   Navigate to: `https://localhost:7111/swagger`

## 📁 Project Structure

```
TaskManagementApi/
├── Context/
│   ├── AppDbContext.cs
│   └── Configurations/
│       └── TaskConfiguration.cs
├── Controllers/
│   ├── AccountController.cs      # Authentication endpoints
│   ├── TaskController.cs         # Task management endpoints
│   └── UserController.cs         # User profile endpoints
├── DTO/
│   ├── ChangePasswordDto.cs
│   ├── NameDto.cs
│   ├── ProfileDto.cs
│   ├── RegisterDto.cs
│   ├── TaskDto.cs
│   └── UserDto.cs
├── Enums/
│   ├── PriorityEnum.cs
│   └── StatusEnum.cs
├── Exceptions/
│   └── GlobalExceptionHandler.cs
├── Extensions/
│   └── TaskExtensions.cs
├── Model/
│   ├── ApplicationUser.cs
│   └── TaskData.cs
├── Repositories/
│   ├── IRepositories/
│   │   ├── ITaskRepository.cs
│   │   └── IUserRepository.cs
│   ├── TaskRepository.cs
│   └── UserRepository.cs
└── Program.cs

TaskManagement.Test/
├── Controllers/
│   ├── AccountControllerTest.cs
│   ├── TaskControllerTest.cs
│   └── UserControllerTest.cs
├── ExceptionHandlers/
│   └── GlobalExceptionHandlerTest.cs
└── HelperMethodes/
    └── Helpers.cs
```

## 🔌 API Endpoints

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/Account/register` | Register a new user | No |
| POST | `/api/Account/login` | Login and receive JWT token | No |

### Tasks

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/Task` | Get paginated tasks | Yes |
| GET | `/api/Task/{id}` | Get task by ID | Yes |
| POST | `/api/Task` | Create a new task | Yes |
| PUT | `/api/Task/{id}` | Update a task | Yes |
| DELETE | `/api/Task/{id}` | Delete a task | Yes |
| GET | `/api/Task/filter/status/{statusNumber}` | Filter tasks by status (0-2) | Yes |
| GET | `/api/Task/filter/priority/{priorityNumber}` | Filter tasks by priority (0-2) | Yes |
| GET | `/api/Task/search/{title}` | Search tasks by title | Yes |

### User Profile

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/User/upload-photo` | Upload profile picture | Yes |
| GET | `/api/User/Profile-Photo` | Get profile picture URL | Yes |
| GET | `/api/User/delete-photo` | Delete profile picture | Yes |
| PUT | `/api/User/Change-Name` | Change username | Yes |
| PUT | `/api/User/Change-Email/{newEmail}` | Change email | Yes |
| PUT | `/api/User/reset-password` | Reset password | Yes |
| GET | `/api/User/profile` | Get user profile | Yes |

## 🔐 Authentication

This API uses JWT (JSON Web Token) for authentication. To access protected endpoints:

1. **Register** a new user or **login** with existing credentials
2. Copy the returned JWT token
3. In Swagger, click the **Authorize** button
4. Enter: `Bearer YOUR_TOKEN_HERE`
5. Click **Authorize**

### Example Login Request

```json
POST /api/Account/login
{
  "userName": "johndoe",
  "password": "SecurePassword123!"
}
```

### Example Response

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "abc123",
  "userName": "johndoe",
  "email": "john@example.com"
}
```

## ⚙️ Configuration

### JWT Settings

Configure JWT settings in `appsettings.json`:

```json
"Jwt": {
  "Key": "your-secret-key-minimum-32-characters",
  "Issuer": "https://localhost:7111",
  "Audience": "https://localhost:7111"
}
```

### Rate Limiting

- **Authenticated Users**: 5 requests per minute (Token Bucket)
- **Anonymous Users**: 5 requests per minute (Fixed Window)

### Caching

- Task lists are cached for 5 minutes
- Cache is automatically invalidated on updates

## 🧪 Testing

The project includes comprehensive unit tests using xUnit and Moq.

### Run Tests

```bash
dotnet test
```

### Test Coverage

- **AccountController**: Registration and login flows
- **TaskController**: CRUD operations, filtering, search
- **UserController**: Profile management, photo upload
- **GlobalExceptionHandler**: Exception handling

## 📝 Database Schema

### ApplicationUser
- Id (string, PK)
- UserName (string)
- Email (string)
- ProfilePicturePath (string, nullable)
- Tasks (Collection)

### TaskData
- Id (int, PK)
- Title (string, max 200)
- Description (string, max 1000)
- DueDate (DateTime, nullable)
- Priority (enum: Low, Medium, High)
- Status (enum: Todo, InProgress, Done)
- UserId (string, FK)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is available for educational and personal use.

## 📧 Contact

For any questions or support, please contact: mohamed987456mm20@gmail.com

