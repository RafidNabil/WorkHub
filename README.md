Overview

WorkHub is a project management application built using the ASP.NET MVC framework with MongoDB as the database. It supports three user roles: Admin, Manager, and Developer. The system allows for task and project management with various features including file attachments, notifications, Kanban boards, Gantt charts, and user role management.
Features

    Project Management: Admins and Managers can create and manage projects, assign tasks, and monitor project progress.
    Task Management: Developers can view, comment on, and complete tasks, while Managers can assign and review them.
    Kanban Board: Visual representation of tasks categorized by status.
    Gantt Chart: Project timeline visualization for tasks.
    File Uploads: Ability to attach files to tasks using MongoDB GridFS.
    Notifications: Task-related notifications are sent to users in real-time.
    Roles: Admins, Managers, and Developers have different levels of access and responsibilities.

Technologies Used

    Backend: ASP.NET MVC (not .NET Core)
    Frontend: HTML, CSS, JavaScript, DHTMLX Gantt Chart, jQuery
    Database: MongoDB (MongoDB Atlas)
    ORM: MongoDB.Driver for database interaction
    File Storage: MongoDB GridFS for file attachments

Setup Instructions
Prerequisites

    Visual Studio (with ASP.NET MVC support)
    MongoDB Atlas account (or local MongoDB server)
    .NET Framework 4.7.2 or higher

MongoDB Configuration

    Configure the MongoDB connection string in Web.config:

    xml

    <connectionStrings>
      <add name="MongoDB" connectionString="mongodb+srv://<username>:<password>@cluster0.mongodb.net/WorkHub?retryWrites=true&w=majority" />
    </connectionStrings>

    Update the connection string with your MongoDB Atlas credentials.

Running the Project

    Clone the repository:

    bash

    git clone https://github.com/your-username/WorkHub.git

    Open the solution in Visual Studio.

    Restore NuGet packages and build the project.

    Run the application (F5).

Key Files and Structure

    Controllers/
        ManagerController.cs: Manages project and task-related operations.
        AdminController.cs: Manages user roles and admin-level operations.
        TaskController.cs: Handles task-specific functionality such as file uploads, comments, etc.
    Models/
        Project.cs: Represents a project entity.
        Task.cs: Represents a task entity.
        User.cs: Represents a user entity with roles.
    Views/
        Manager/: Views related to manager functionalities such as project and task management.
        Admin/: Admin-related views for user management.
    Scripts/
        gantt_chart.js: Custom script to display Gantt charts using DHTMLX Gantt.

Key Features in Code
Notifications

Notifications are generated when tasks are assigned or completed. These are stored in a simple Notifications collection and displayed to users across the application.
Gantt Chart Integration

The Gantt chart is implemented using DHTMLX Gantt. Task data is passed to the chart using JSON. The chart visualizes tasks with start/end dates and statuses.

File Upload via GridFS

File attachments are stored in MongoDB using GridFS, and are tied to specific tasks. The files can be uploaded and deleted, with proper cleanup of associated chunks.
