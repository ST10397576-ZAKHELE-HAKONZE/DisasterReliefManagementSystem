# 🌟 Gift of the Givers Foundation - Disaster Relief Management System

> A C# web application to empower humanitarian aid through technology — enabling volunteer coordination, disaster reporting, and resource donation management.

![Azure DevOps Build](https://img.shields.io/azure-devops/build/your-org/Disaster-Relief-Management-System/_build_id_)  
![Language - C#](https://img.shields.io/badge/Language-C%23-blue)  
![Framework - ASP.NET Core](https://img.shields.io/badge/Framework-.NET_8-lightgrey)  
![Database - Azure SQL](https://img.shields.io/badge/Database-Azure_SQL-green)  
![License - MIT](https://img.shields.io/badge/License-MIT-yellow)

This project was developed as part of an Applied Programming POE to support the **[Gift of the Givers Foundation](https://giftofthegivers.org/)** — South Africa’s largest disaster relief NGO. The web application streamlines emergency response operations by providing tools for incident reporting, donations tracking, volunteer management, and admin oversight — all built using Microsoft Azure cloud services and modern .NET technologies.

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Planning (Azure Boards)](#-project-planning---azure-boards)
- [Database Design](#-database-design)
- [Getting Started](#-getting-started)
- [Installation & Setup](#-installation--setup)
- [Contributors](#-contributors)
- [License](#-license)

---

## ✨ Features

✅ **User Authentication & Roles**  
Secure login/register system with role-based access (Volunteer, Coordinator, Admin) using ASP.NET Identity.

✅ **Disaster Incident Reporting**  
Users can submit real-time reports during emergencies with title, location, severity, and description.

✅ **Resource Donation Management**  
Track food, water, clothing, and monetary donations per relief project with status updates.

✅ **Volunteer Management System**  
Allow volunteers to register, apply for tasks, and get assigned to active projects.

✅ **Admin Dashboard**  
Overview of ongoing projects, recent donations, open incidents, and team assignments.

✅ **Reporting Tools**  
Export data to Excel for analysis and donor acknowledgments.

---

## 💻 Tech Stack

| Layer | Technology |
|------|------------|
| **Frontend** | Razor Pages / HTML5 + CSS3 + Bootstrap 5 |
| **Backend** | C#, ASP.NET Core MVC (.NET 8) |
| **Authentication** | ASP.NET Core Identity |
| **Database** | Azure SQL Database (with EF Core Code First) |
| **ORM** | Entity Framework Core 8 |
| **Version Control** | Azure Repos (Git) |
| **Project Management** | Azure Boards (Agile) |
| **CI/CD** | Azure Pipelines (YAML) |
| **Hosting** | Azure App Services |
| **Testing** | MSTest, xUnit, Selenium, JMeter |

---

## 🛠️ Project Planning - Azure Boards

The development lifecycle follows Agile methodology using **Azure Boards** for full traceability from idea to deployment.

### 🔷 Epics
- User Authentication
- Incident Reporting
- Donation Management
- Volunteer Assignment
- Admin Dashboard

### 🔹 Sprints Overview
| Sprint | Goal | Duration |
|-------|------|---------|
| **Sprint 1: Setup & Auth** | Initialize app, set up DB, implement user registration/login | Week 1 |
| **Sprint 2: Core Features** | Build incident reporting, donation tracking, project creation | Week 2 |
| **Sprint 3: Final Touches** | Admin dashboard, reporting export, performance tuning | Week 3 |

🔗 [View Azure Boards Dashboard](https://dev.azure.com/your-org/Disaster-Relief-Management-System/_boards/board/t/Team%20Backlog/My%20Board)

> *Note: Bugs are tracked separately outside backlogs for clarity.*

---

## 🗃️ Database Design

### For Visual ERD
See: [`docs/erd-diagram.png`](docs/erd-diagram.png)

### 📄 Key Tables

| Table | Description |
|------|-------------|
| `Users` | Stores registered users with roles (Volunteer, Admin, etc.) |
| `ReliefProjects` | Tracks active/past relief efforts across regions |
| `Donations` | Logs donated items or funds linked to projects |
| `VolunteerAssignments` | Manages which volunteers are assigned to which projects |
| `IncidentReports` | Captures new disaster events reported by users |

### 🔍 Indexes Applied for Performance
- `Users.Email` → Fast login and authentication
- `Donations.ProjectID`, `DonationDate` → Efficient querying and reporting
- `IncidentReports.Location`, `Severity` → Quick filtering during emergencies

---

## 🚀 Getting Started

This section helps you run the project locally for development or review.

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 **or** VS Code with C# extension
- Azure SQL Database instance (or local SQL Server)
- Azure DevOps account (for CI/CD integration)

---

## 📦 Installation & Setup

### 1. Clone the Repository
```bash
git clone https://dev.azure.com/your-org/Disaster-Relief-Management-System/_git/GiftOfTheGivers.Web
cd GiftOfTheGivers.Web
```
### 2. Configure Connection String
```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:<server>.database.windows.net,1433;Initial Catalog=GiftOfTheGiversDB;Persist Security Info=False;User ID=<user>;Password=<pass>;Encrypt=True;"
  }
}
```
📌 Replace:

- <server> → Your Azure SQL server name
- <user> → Database login username
- <pass> → Database password
- Ensure Firewall Rules on Azure SQL allow your IP address.

### 3. Apply Database Migrations
This will create the database schema based on your EF Core models.
```bash
dotnet ef database update
```
If you encounter errors:

- Run dotnet restore first
- Ensure Entity Framework Tools are installed:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 4. Run the Application
Start the web server using the .NET CLI:
```bash
dotnet run
```
🌐 Open your browser and navigate to:
👉 https://localhost:7001

## 👥 Contributors
Zakhele Hakonze (Student Developer)
- Student ID: ST10397576
- Role: Full-stack Development, Project Planning, Testing
Lecturer Reviewer
- Role: Mentor & Evaluation
- Organisation: Rosebank College
- Course: Applied Programming POE – APPR6312

