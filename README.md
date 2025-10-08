📋 Table of Contents

Features
Technologies
Getting Started
Project Structure
API Endpoints
Authentication
Configuration
Testing
Contributing

✨ Features
Core Features

User Authentication & Authorization: JWT-based authentication with ASP.NET Core Identity
Task Management: Full CRUD operations for tasks
User Profile Management: Upload profile pictures, change name, email, and password
Advanced Filtering: Filter tasks by status, priority, and search by title
Pagination: Paginated responses for better performance
Caching: Memory cache implementation for improved response times
Rate Limiting: Per-user and anonymous rate limiting to prevent abuse
Global Exception Handling: Centralized error handling with detailed logging

Task Features

Create, read, update, and delete tasks
Set task priority (Low, Medium, High)
Track task status (Todo, In Progress, Done)
Set due dates
Filter by status and priority
Search tasks by title
Pagination support

User Features

User registration and login
JWT token-based authentication
Profile picture upload (JPG, PNG, max 10MB)
Change username, email, and password
View user profile

🛠 Technologies

Framework: ASP.NET Core 9.0
Database: SQL Server with Entity Framework Core 9.0
Authentication: ASP.NET Core Identity with JWT Bearer tokens
Testing: xUnit with Moq
API Documentation: Swagger/OpenAPI
Caching: In-Memory Cache
Rate Limiting: Token Bucket Algorithm

🚀 Getting Started
Prerequisites

.NET 9.0 SDK
SQL Server (Express or higher)
Visual Studio 2022 or VS Code

Installation

Clone the repository

bash   git clone <repository-url>
   cd TaskManagementApi

Update the connection string
Edit appsettings.json and update the connection string:

json   "ConnectionStrings": {
     "conStr": "server=YOUR_SERVER;database=TManagementDb;integrated security=true;trustservercertificate=true"
   }

Create the database
Run the following commands in the Package Manager Console or terminal:

bash   dotnet ef migrations add InitialCreate
   dotnet ef database update

Run the application

bash   dotnet run --project TaskManagementApi

Access Swagger UI
Navigate to: https://localhost:7111/swagger

📁 Project Structure
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
🔌 API Endpoints
Authentication
MethodEndpointDescriptionAuth RequiredPOST/api/Account/registerRegister a new userNoPOST/api/Account/loginLogin and receive JWT tokenNo
Tasks
MethodEndpointDescriptionAuth RequiredGET/api/TaskGet paginated tasksYesGET/api/Task/{id}Get task by IDYesPOST/api/TaskCreate a new taskYesPUT/api/Task/{id}Update a taskYesDELETE/api/Task/{id}Delete a taskYesGET/api/Task/filter/status/{statusNumber}Filter tasks by status (0-2)YesGET/api/Task/filter/priority/{priorityNumber}Filter tasks by priority (0-2)YesGET/api/Task/search/{title}Search tasks by titleYes
User Profile
MethodEndpointDescriptionAuth RequiredPOST/api/User/upload-photoUpload profile pictureYesGET/api/User/Profile-PhotoGet profile picture URLYesGET/api/User/delete-photoDelete profile pictureYesPUT/api/User/Change-NameChange usernameYesPUT/api/User/Change-Email/{newEmail}Change emailYesPUT/api/User/reset-passwordReset passwordYesGET/api/User/profileGet user profileYes
🔐 Authentication
This API uses JWT (JSON Web Token) for authentication. To access protected endpoints:

Register a new user or login with existing credentials
Copy the returned JWT token
In Swagger, click the Authorize button
Enter: Bearer YOUR_TOKEN_HERE
Click Authorize

Example Login Request
jsonPOST /api/Account/login
{
  "userName": "johndoe",
  "password": "SecurePassword123!"
}
Example Response
json{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "abc123",
  "userName": "johndoe",
  "email": "john@example.com"
}
⚙️ Configuration
JWT Settings
Configure JWT settings in appsettings.json:
json"Jwt": {
  "Key": "your-secret-key-minimum-32-characters",
  "Issuer": "https://localhost:7111",
  "Audience": "https://localhost:7111"
}
Rate Limiting

Authenticated Users: 5 requests per minute (Token Bucket)
Anonymous Users: 5 requests per minute (Fixed Window)

Caching

Task lists are cached for 5 minutes
Cache is automatically invalidated on updates

🧪 Testing
The project includes comprehensive unit tests using xUnit and Moq.
Run Tests
bashdotnet test
Test Coverage

AccountController: Registration and login flows
TaskController: CRUD operations, filtering, search
UserController: Profile management, photo upload
GlobalExceptionHandler: Exception handling

📝 Database Schema
ApplicationUser

Id (string, PK)
UserName (string)
Email (string)
ProfilePicturePath (string, nullable)
Tasks (Collection)

TaskData

Id (int, PK)
Title (string, max 200)
Description (string, max 1000)
DueDate (DateTime, nullable)
Priority (enum: Low, Medium, High)
Status (enum: Todo, InProgress, Done)
UserId (string, FK)

🤝 Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

Fork the project
Create your feature branch (git checkout -b feature/AmazingFeature)
Commit your changes (git commit -m 'Add some AmazingFeature')
Push to the branch (git push origin feature/AmazingFeature)
Open a Pull Request

📄 License
This project is available for educational and personal use.
📧 Contact
For any questions or support, please contact: mohamed987456mm20@gmail.com