# 🌿 AgriGreen

AgriGreen is a web-based management system built with ASP.NET Core MVC. 

## 📌 Key Features

- **User Roles & Authorization**
  - Supports `Employee` and `Admin` roles with restricted access based on roles.
- **Farmer Management**
  - Employees can view, add, update, and delete farmer records.
- **Product Management**
  - Employees can manage agricultural product records.
- **User Authentication**
  - Uses ASP.NET Identity for secure login and role-based access control.
- **Database Integration**
  - Connected to a SQLite database (`AgriGreen.db`) with Entity Framework Core for data operations.
- **Razor Views**
  - Clean and structured UI for Farmers and Products management.
- **Migration Support**
  - Includes EF Core migrations and SQL script for database updates (`update-database.sql`).

## 🛠️ Tech Stack

- ASP.NET Core MVC (.NET 6 or higher)
- Entity Framework Core
- SQLite
- Razor Pages
- Bootstrap (via wwwroot)

## 📂 Folder Structure Overview

| Folder/File               | Description |
|--------------------------|-------------|
| `Controllers/`           | MVC Controllers for routing logic (`Farmers`, `Products`, `Home`) |
| `Models/`                | Entity models for Farmers, Users, Products |
| `Views/`                 | Razor Views grouped by controller name |
| `Data/`                  | Database context (`ApplicationDbContext.cs`) |
| `Migrations/`            | EF Core migration files |
| `Services/`              | App services (if extended) |
| `wwwroot/`               | Static files (CSS, JS, etc.) |
| `appsettings.json`       | Configuration including DB connection string |
| `AgriGreen.db`           | SQLite database file |
| `Program.cs`             | Application entry point |
| `README-Database-Seeding.md` | Instructions for populating test data |

## 🚀 How to Run the Project

### ✅ Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) (recommended) or VS Code with C# extension
- Git

### 📥 Step-by-Step Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/AgriGreen.git
   cd AgriGreen
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Apply migrations and create the database (if not already created)**
   ```bash
   dotnet ef database update
   ```

   Or execute the SQL file manually in a SQLite tool:
   - Open `update-database.sql`
   - Run against `AgriGreen.db`

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Visit in browser**
   ```
   http://localhost:5000
   ```
### Or to download from github 
## 📎 Download 

To download the app and run it locally:

1. Click **Code > Download ZIP** on the GitHub repo.
2. Extract the ZIP on your machine.
3. Open the extracted file and open the .sln file.
4. This will open up visual studio and from there to run it simply click the green arrow at the top of the screen.
## 🧪 Testing Credentials

Use the seeding guide in `README-Database-Seeding.md` to populate test users, such as: (The way i set it up seeding should happen automatically when you launch the project)

- **Employee**
  - Username: `employee@example.com`
  - Password: `Password123!`

- **Admin**
  - Username: `admin@example.com`
  - Password: `AdminPass123!`

## 🧭 Navigation

| Page            | URL Path         | Access Role |
|-----------------|------------------|-------------|
| Home            | `/`              | All         |
| Farmer List     | `/Farmers`       | Employee    |
| Product List    | `/Products`      | Employee    |
| Farmer Details  | `/Farmers/Details/{id}` | Employee    |



## 🔧 Recommended Tools & Extensions

### SQLite and SQL Server Compact Toolbox
A Visual Studio extension that provides a user-friendly interface for managing SQLite databases. It allows you to:
- Explore database objects
- Execute SQL queries
- Manage schema visually
- 🔗 [Download from Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ErikEJ.SQLServerCompactSQLiteToolbox)

---
## Below are brief explanations of some of the extra features i've implemented and the links to the documentation on how to implement them:(My references)




## 🏗️ Scaffolding in ASP.NET Core

This project makes use of **scaffolding** to automatically generate boilerplate code for controllers and views from models. This significantly speeds up the development process.

🔗 [ASP.NET Core Scaffolding Documentation](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/adding-model?view=aspnetcore-6.0&tabs=visual-studio)

---

## 📷 QR Code Functionality

QR code support is integrated to allow encoding of URLs or text for quick mobile access. The [`QRCoder`](https://github.com/codebude/QRCoder) library is typically used for this in .NET projects.

🔗 [How to Create QR Codes in ASP.NET Core](https://www.c-sharpcorner.com/article/creating-qrcode-in-asp-net-core/)
## ✉️ Email Functionality with Mailjet

The project uses **Mailjet** for sending transactional emails, including email verification during registration.

- Ensure that you configure your `appsettings.json` or secrets to include your Mailjet `API Key` and `Secret Key`.(I know my API keys are public in the project and thats bad, i've only done it so the marker can make use of the functionality when they run it locally)

🔗 [Mailjet SMTP & API Integration Guide](https://dev.mailjet.com/email/guides/send-api-v31/)

**⚠️ Note:** Verification emails may land in your **Spam** folder. Please check there if you do not see it in your inbox.  
➡️ If you're a marker/testing user, **bypass buttons are available in the UI** to skip verification.

---

## 🔐 Google APIs Integration

This project integrates with **Google APIs** ( OAuth login ).

- You must create a project in the [Google Cloud Console](https://console.cloud.google.com/).
- Enable the required APIs (e.g., Maps, OAuth2).
- Generate and configure your `Client ID` and `API Key` in your environment variables or `appsettings.json`.

🔗 [Getting Started with Google APIs](https://developers.google.com/api-client-library/dotnet/start)


