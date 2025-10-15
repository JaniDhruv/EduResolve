# EduResolve – Startup Pitch Web Application

EduResolve is our proof-of-concept startup project for the Web Application Development course. It applies the MVC pattern in ASP.NET Core to solve a real campus problem: fragmented complaint handling between students, faculty, and department leadership. The platform offers a streamlined channel to submit, track, triage, and escalate academic or administrative complaints with clear accountability.

## 1. Startup Pitch Snapshot

- **Problem**: Students lack transparency and timely feedback once a complaint leaves their hands; faculty and HODs juggle manual tracking and miss escalations.
- **Solution**: EduResolve centralizes complaint workflows, automates reminders and escalations, and gives each persona a focused dashboard to act quickly.
- **Unique Edge**: Department-aware routing plus an automated escalation service that highlights unattended items, making it easy to prove measurable response times to stakeholders.

## 2. Core Features

1. **Role-Based Dashboards** – Personas (Student, Teacher, HOD, Admin) see tailored metrics and action items, improving decision-making and accountability.
2. **Complaint Lifecycle Management** – End-to-end flow covering submission, attachments, threaded comments, and status transitions (`New → InProgress → Resolved/Closed → Reopened`).
3. **Automated Escalation Service** – Background job flags complaints older than 72 hours, resets on progress, and prioritizes them for HOD attention.
4. **Secure Identity & Authorization** – ASP.NET Core Identity enforces password policies, handles registration/login, and uses cookies for session management.

> Expand the demo video to show at least these four modules in action for full marks on “functionality” and “presentation”.

## 3. Architecture & Technology Choices

- **Framework**: ASP.NET Core 3.1 MVC
- **Language**: C#
- **Data Access**: Entity Framework Core 3.1 + SQL Server
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Background Processing**: Hosted service (`EscalationService`) running on a 24-hour interval
- **Frontend**: Razor Views with Bootstrap 4 and jQuery
- **Integrations**: No external APIs; all workflows execute within EduResolve boundaries.
- **Hosting Ready**: .NET Core cross-platform deployment; static files served via `wwwroot`

### High-Level Flow

1. Users authenticate via Identity (students and teachers self-register; admin/HOD seeded).
2. Complaints persist in SQL Server through EF Core with enforced constraints and relationships.
3. Dashboards pull filtered statistics based on logged-in user role and department.
4. Escalation service runs nightly, tagging overdue complaints for leadership attention.

## 4. Database & Seed Data

- **Tables**: `AspNetUsers`, `AspNetRoles`, `Complaints`, `ComplaintAttachments`, `Comments`, `Departments`, etc.
- **Seeding** (`Data/SeedData.cs`):
  - Creates default roles: `Admin`, `HOD`, `Teacher`, `Student`
  - Inserts baseline departments (Computer, Electrical, Mechanical Engineering, Library)
  - Generates:
    - `admin@eduresolve.local` / `Admin@123`
    - Department HOD accounts: `<department-slug>.hod@eduresolve.local` / `Hod@123`

Students and teachers register through `/Account/Register`; department selection is required for both roles.

## 5. Getting Started (Setup Instructions)

### Prerequisites

- .NET Core SDK 3.1.x – [Download](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
- SQL Server (LocalDB, Express, full SQL Server, or Azure SQL)
- Optional: `dotnet-ef` CLI tool for migrations

```powershell
dotnet tool install --global dotnet-ef
```

### Step 1 – Clone the Repository

```powershell
git clone https://github.com/JaniDhruv/EduResolve.git
cd EduResolve
```

### Step 2 – Configure Connection String

Update `EduResolve/appsettings.json` or use user secrets to keep credentials out of source control:

```powershell
cd EduResolve
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=EduResolve;Trusted_Connection=True;MultipleActiveResultSets=true"
```

> The default development connection string already points to LocalDB. Adjust for your SQL Server instance if needed.

### Step 3 – Apply Migrations & Seed Data

```powershell
dotnet ef database update
```

This runs migrations and executes the seeding routine on startup.

### Step 4 – Run the Application

```powershell
dotnet run
```

Browse to the HTTPS URL printed in console (typically `https://localhost:5001`). Login using the seeded admin or register as a student/teacher to explore dashboards.

## 6. Future Scope (Talk Track for Investors)

- Email/SMS notifications for escalations and status changes
- Analytics dashboards with charts and export options
- SLA tracking with configurable thresholds per department
- Public REST API for mobile intake or chatbot integrations
- Modern frontend rewrite (React/Angular) on top of existing APIs

## 7. Team & Contributions

| Student Name | Roll No. | Focus Area | Key Contributions |
|---------------|-----------|-------------|--------------------|
| **Dhruv Jani** | CE097 | Backend & Architecture | Modeled the complaint entities, implemented controller logic, and documented key technical decisions. |
| **Avadh Vaishnani** | CE062 | Frontend Experience | Designed responsive Razor views and delivered role-aware dashboards for students and teachers. |
| **Yug Vasava** | CE058 | Identity & Operations | Built authentication and role assignment logic, configured Identity, and prepared seed data. |

## 8. Demo Video Link

(https://drive.google.com/file/d/1lfd-ugjH01SYaywrORYf_oOFzZbPkWH3/view?usp=sharing)

## 9. Folder Layout

```
EduResolve/
├─ Controllers/        # MVC controllers for auth, complaints, dashboards, etc.
├─ Data/               # EF Core DbContext and seeding helpers
├─ Migrations/         # Generated migrations history
├─ Models/             # Entity classes (Complaint, Department, ApplicationUser, ...)
├─ Services/           # Background services (EscalationService)
├─ ViewModels/         # View-specific DTOs
├─ Views/              # Razor pages organized by feature
├─ wwwroot/            # Static assets (CSS, JS, uploads)
└─ appsettings*.json   # Configuration files
```

## 10. Development Tips

- Run `dotnet watch run` for hot reload while iterating on UI.
- When modifying data models, generate migrations via `dotnet ef migrations add <Name>` and re-run `dotnet ef database update`.
- Ensure `wwwroot/uploads` remains writable for attachment storage.
- Review `Services/EscalationService.cs` for the TODO on integrating a production notification channel.
