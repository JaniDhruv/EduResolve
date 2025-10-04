# Software Requirements Specification (SRS)
## EduResolve: Hierarchical Complaint Management System

**Version:** 1.0  
**Date:** September 30, 2025

---

## 1. Introduction

This document outlines the functional and non-functional requirements for **EduResolve**, a web-based Digital Complaint and Resolution Management System. The system is designed for educational institutions to streamline the process of lodging, tracking, and resolving grievances within a defined hierarchical structure.

### 1.1 Purpose

The purpose of EduResolve is to provide a centralized, transparent, and efficient platform for students, teachers, and Heads of Department (HODs) to manage complaints. It aims to replace manual or disparate methods with a structured digital workflow, ensuring accountability and timely resolutions.

### 1.2 Project Scope

The project entails the development of a complete web application using **ASP.NET Core MVC 3.1**. The scope includes:

- **In Scope:** User registration and role-based authentication, hierarchical complaint submission, status tracking, a comment-based communication system, multi-level dashboards, and an automated escalation mechanism for unresolved complaints.
- **Out of Scope:** Mobile application (initially), real-time chat functionality, integration with third-party university management systems, and advanced data analytics.

### 1.3 Intended Audience

This document is intended for project stakeholders, including the development team, project manager, and university administration, to serve as a foundational guide for design, development, and testing.

---

## 2. Overall Description

### 2.1 Product Perspective

EduResolve is a self-contained system that will operate within a university's intranet or be hosted publicly. It is designed to manage internal grievances related to academics, infrastructure, administration, or other campus-related issues.

### 2.2 User Classes and Characteristics

The system will support four distinct user roles with a clear hierarchy:

1. **Student:** The primary user who initiates complaints. They can submit grievances to their assigned Teacher or escalate them directly to the HOD.
2. **Teacher (Faculty):** The first level of resolution. They receive, view, and act upon complaints submitted by their students. They can also lodge their own complaints (e.g., regarding infrastructure or student issues) to the HOD.
3. **Head of Department (HOD):** The administrative and oversight authority. The HOD can view *all* complaints within their department, whether submitted to a teacher or directly to them. They are responsible for handling escalated complaints and have the authority to re-assign or resolve any complaint.
4. **System Administrator (Admin):** A super-user responsible for managing user accounts, departments, complaint categories, and overall system configuration.

### 2.3 Operating Environment

- **Backend:** ASP.NET Core 3.1
- **Frontend:** ASP.NET Core MVC (Views using Razor)
- **Database:** SQL Server (recommended) or SQLite
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Deployment:** IIS Server or Azure App Service

---

## 3. System Features

### 3.1 User Authentication and Authorization (FUNC-AUTH)

- **FUNC-AUTH-01: Registration:** Users shall be able to register by providing their Name, Email, Password, Role (Student/Teacher), and Department. Admin will create HOD and other Admin accounts.
- **FUNC-AUTH-02: Login:** Registered users must log in using their email and password to access the system.
- **FUNC-AUTH-03: Role-Based Access Control:** The system must restrict access to features based on the user's role. A student should not be able to access the HOD dashboard.

### 3.2 Complaint Management (FUNC-COMP)

- **FUNC-COMP-01: Submit Complaint:**
  - **Students** can submit a new complaint with a Title, Description, Category (e.g., Academic, Infrastructure, Hostel), and an optional image/document upload.
  - When submitting, a student must select a recipient: their assigned **Teacher** (default) or the **HOD**.
  - **Teachers** can submit complaints directly to their **HOD**.
- **FUNC-COMP-02: Complaint Tracking:**
  - Each complaint will be assigned a unique Ticket ID.
  - The system will support the following statuses: **New**, **In Progress**, **Resolved**, **Closed**, **Reopened**.
- **FUNC-COMP-03: View Complaints:**
  - **Students** can only view a list of their own submitted complaints and their current status.
  - **Teachers** can view complaints assigned *to them* by students, as well as their own complaints submitted to the HOD.
  - **HODs** have a global view of all complaints filed within their department. They can filter complaints by status, assigned teacher, or student.
- **FUNC-COMP-04: Update Complaint Status:**
  - **Teachers** can change the status of complaints assigned to them (e.g., from 'New' to 'In Progress').
  - **HODs** can change the status of any complaint within their department.
- **FUNC-COMP-05: Communication Log:** Users (submitter and assignee) can add comments to a complaint ticket to facilitate communication, ask for clarifications, or provide updates. Each comment will be timestamped.

### 3.3 Hierarchical Dashboards (FUNC-DASH)

- **FUNC-DASH-01: Student Dashboard:** Displays a summary of the user's complaints (e.g., Total Submitted, Open, Resolved). Shows a list of recent complaints and their statuses.
- **FUNC-DASH-02: Teacher Dashboard:** Displays key metrics like 'New Complaints Assigned', 'In Progress', and 'Resolved by Me'. Includes a list of pending complaints requiring attention.
- **FUNC-DASH-03: HOD Dashboard:** Provides a high-level overview of the entire department's grievance status.
  - Displays statistics (e.g., Total complaints, Resolution Rate, Average Resolution Time).
  - Features a dedicated panel for **"Escalated Complaints"** that require immediate action.
  - Allows filtering of all departmental complaints.

### 3.4 Automated Escalation Mechanism (FUNC-ESC)

- **FUNC-ESC-01: Escalation Rule:** A complaint assigned to a Teacher that remains in the **"New"** status for more than **72 hours** (3 days) will be automatically flagged as 'Escalated'.
- **FUNC-ESC-02: HOD Notification:**
  - Escalated complaints will appear in a special, highlighted section on the HOD's dashboard.
  - The system will send a daily summary email to the HOD listing all newly escalated and pending escalated complaints.
- **FUNC-ESC-03: Action on Escalation:** The HOD can then re-assign the complaint to another teacher or take direct ownership to resolve it.

---

## 4. Non-Functional Requirements

### 4.1 Performance

- Web pages should load within 3-4 seconds under normal network conditions.
- Database queries, especially for the HOD dashboard, must be optimized to handle a large volume of complaints without significant delay.

### 4.2 Security

- All user passwords must be securely hashed and salted.
- The system must be protected against common web vulnerabilities like SQL Injection and Cross-Site Scripting (XSS).
- Role-based authorization must be enforced on both the client-side (hiding UI elements) and server-side (validating every request).

### 4.3 Usability

- The user interface should be clean, intuitive, and easy to navigate for all user roles.
- The application must have a responsive design, ensuring it is usable on desktops, tablets, and mobile devices.

---

## 5. Appendix

### A. Conceptual Database Schema

Here's a simplified schema to support the required features.

- **Users**
  - `Id` (PK)
  - `FirstName`, `LastName`
  - `Email` (Unique)
  - `PasswordHash`
  - `RoleId` (FK to Roles table)
  - `DepartmentId` (FK to Departments table)

- **Roles**
  - `Id` (PK)
  - `Name` (e.g., "Student", "Teacher", "HOD", "Admin")

- **Departments**
  - `Id` (PK)
  - `Name` (e.g., "Computer Engineering", "Library")

- **Complaints**
  - `Id` (PK, TicketID)
  - `Title`
  - `Description`
  - `Status` (e.g., New, InProgress, Resolved)
  - `Category`
  - `CreatedAt`
  - `UpdatedAt`
  - `SubmittedByUserId` (FK to Users)
  - `AssignedToUserId` (FK to Users - Teacher or HOD)
  - `IsEscalated` (Boolean flag)

- **ComplaintAttachments**
  - `Id` (PK)
  - `ComplaintId` (FK to Complaints)
  - `FilePath`

- **Comments**
  - `Id` (PK)
  - `Content`
  - `CreatedAt`
  - `ComplaintId` (FK to Complaints)
  - `UserId` (FK to Users)
