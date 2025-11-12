# MindfulMe

# Clone the repository
git clone https://github.com/yourusername/MindfulMe.git

# Navigate to API project
cd MindfulMe.API
dotnet restore
dotnet ef database update

# Run the API
dotnet run

# In another terminal, run the desktop client
cd ../MindfulMe
   dotnet run

•	API endpoint configuration (default: https://localhost:7073)
•	Prolog file paths (recommendations.pl, prolog_runner.pl)
•	Database connection string (SQLite)


MindfulMe/
├── MindfulMe.API/          # REST API backend
│├── Controllers/
│   ├── Models/
│   ├── Services/
│   └── Data/
├── MindfulMe/              # WinForms desktop client
│   ├── Forms/
│   ├── Models/
│   ├── Services/
│   └── recommendations.pl  # Prolog knowledge base

Tech Stack
•	Backend: ASP.NET Core Web API (.NET 8)
•	Frontend: Windows Forms (.NET 8)
•	Database: SQLite with Entity Framework Core
•	AI/Logic: SWI-Prolog integration for recommendations
•	Authentication: BCrypt password hashing

Prerequisites
•	.NET 8 SDK
•	SWI-Prolog installed and added to PATH
•	Visual Studio 2022 (or compatible IDE)
•	Windows OS (for WinForms client)
