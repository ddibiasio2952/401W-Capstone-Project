# Falvey Insurance Group Client Management Site

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-6.0-blue)
![C#](https://img.shields.io/badge/C%23-.NET-239120)
![MySQL](https://img.shields.io/badge/MySQL-8.0-orange)
![License](https://img.shields.io/badge/License-Educational-lightgrey)

> A comprehensive client management web application built for an insurance company as a Rhode Island College Software Engineering Capstone Project.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Setup & Installation](#setup--installation)
- [Edwin's Map](#edwins-map)
- [Team Members](#team-members)

---

## Overview

The **Falvey Insurance Group Client Management Site** is a full-stack web application that enables insurance professionals to efficiently manage:

- Clients
- Insurance Policies
- Claims
- Announcements
- Internal Memos
- Recommendations
- Uploaded Resources

The application also includes:

- Interactive Google Maps integration for client location searches
- Dynamic dashboard
- Secure authentication with session tracking and audit logging
- Responsive user interface

This project was developed by a team of seven students for the **Rhode Island College 401W Software Engineering Capstone** course.

---

## Features

- Secure user authentication
- Session management with audit logging
- Interactive Google Maps client search
- Dynamic dashboard
- Claims management
- Policy management
- Memos and recommendations
- Uploaded resource management
- Release Notes CRUD functionality
- Responsive navigation and layout

---

## Tech Stack

| Layer | Technologies |
|---------|-------------|
| Frontend | HTML, CSS, JavaScript |
| Backend | C#, ASP.NET Core MVC |
| Database | MySQL |
| Mapping | Google Maps API |
| Architecture | MVC, Services, DTOs, Controllers, Filters |

---

## Project Structure

```
401W-Capstone-Project
│
├── Controllers/
├── Models/
├── Services/
├── DTOs/
├── Filters/
├── Views/
├── wwwroot/
│   └── EdwinMap/
├── Db/
│   └── DatabaseDump.sql
├── appsettings.json
└── Program.cs
```

---

# Setup & Installation

## Prerequisites

Install the following software before beginning:

- .NET 6 SDK or newer
- Visual Studio 2022 (recommended) or Visual Studio Code
- MySQL Server
- MySQL Workbench
- Google Maps API key (optional, for map functionality)

---

## 1. Clone the Repository

```bash
git clone https://github.com/ddibiasio2952/401W-Capstone-Project.git
cd 401W-Capstone-Project
```

---

## 2. Install MySQL

Install:

- MySQL Server
- MySQL Workbench

Installation Guide:

https://www.mysqltutorial.org/getting-started-with-mysql/install-mysql/

> Save your MySQL **root password**. You'll need it later.

---

## 3. Connect to MySQL

Open **MySQL Workbench** and connect using your root account.

Tutorial:

https://www.mysqltutorial.org/getting-started-with-mysql/connect-to-mysql-server/

Remember:

- Connection Name
- Host Name

---

## 4. Import the Database

1. Select **Server → Data Import**
2. Choose **Import from Self-Contained File**
3. Browse to:

```
Db/
```

4. Select the SQL dump.
5. Create a new schema.
6. Click **Start Import**.

---

## 5. Configure the Application

Open:

```
appsettings.json
```

Modify the connection string.

Example:

```json
"DefaultConnection": "Server=localhost;Database=FalveyDB;User=root;Password=YourPassword;"
```

Replace with your own values.

If using Google Maps, add your API key where required.

---

## 6. Run the Project

### Visual Studio

Press **F5**

### VS Code

```bash
dotnet run
```

Then open:

```
https://localhost:7288
```

---

## 7. Create an Account

1. Click **Sign Up**
2. Create a user
3. Use Employee ID **1** (or another seeded employee)
4. Log in

The application is now ready to use.

---

# Edwin's Map

Edwin's Map is a standalone version of the mapping functionality demonstrating additional filtering and sorting capabilities.

Navigate to:

```
wwwroot/
└── EdwinMap/
    └── index.html
```

Open `index.html` in your browser.

No ASP.NET server or MySQL database is required.

---

# Team Members

| Member | Responsibilities |
|---------|------------------|
| **Daniel (Repo Owner)** | Project management, backend architecture, database mapping, controllers, DTOs, services, navbar search |
| **Marten** | Dashboard, Login UI, Claims page, database design |
| **Jacob** | Release Notes page, ReleasesController, Login HTML |
| **Edwin** | Client Location page, Google Maps integration, filtering, sorting |
| **An** | Sign In/Up frontend, Resources page, Claims modal |
| **Oswald** | Technical design, backend APIs, Map backend/frontend, Session Settings |
| **Oswin** | Authentication, Sessions, Login Audit, frontend session validation |

---

## Academic Project

This repository was developed as part of the **Rhode Island College 401W Software Engineering Capstone** course.

The application is intended for educational and portfolio purposes.
