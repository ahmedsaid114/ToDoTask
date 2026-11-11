# ToDoTask

A RESTful API for managing tasks, built with ASP.NET Core and Entity Framework Core.

## Features
- Create, read, update, and delete tasks
- [Mark tasks as completed]
- [Filtering / sorting / pagination]
- [Authentication & authorization (JWT)]
- [Input validation and error handling]

## Tech Stack
- C#, ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- [Swagger / Postman, AutoMapper, etc.]

## Getting Started
1. Clone the repo: `git clone https://github.com/ahmedsaid114/ToDoTask.git`
2. Update the connection string in `appsettings.json`
3. Run migrations: `dotnet ef database update`
4. Run the project: `dotnet run`
5. Open Swagger at `https://localhost:[port]/swagger`

## API Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/tasks | Get all tasks |
| POST | /api/tasks | Create a task |
| PUT | /api/tasks/{id} | Update a task |
| DELETE | /api/tasks/{id} | Delete a task |

## Author
Ahmed Said Shaker, [LinkedIn link]
