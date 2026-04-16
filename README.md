# BlindMatch PAS

Project Approval System with Blind Matching for academic project supervision.

## Overview

BlindMatch PAS enables a **blind matching process** between students and supervisors:

1. **Students** submit project proposals anonymously
2. **Supervisors** browse proposals without seeing student identities
3. When a **supervisor confirms a match**, both identities are revealed to each other

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (included with Visual Studio)
- Windows 10/11

## Setup Steps

```bash
# Clone and navigate to project
cd BlindMatchPAS

# Restore packages
dotnet restore

# Create database migration (if needed)
dotnet ef migrations add InitialCreate

# Apply migrations to local database
dotnet ef database update

# Run the application
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

## Default Credentials

| Role         | Email                    | Password    |
|--------------|--------------------------|-------------|
| SysAdmin     | admin@blindmatch.ac.lk   | Admin@123456|
| ModuleLeader | leader@blindmatch.ac.lk   | Leader@123456|
| Student      | (register via UI)        | (your choice)|
| Supervisor   | (register via UI)        | (your choice)|

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      PRESENTATION                          │
│  AccountController │ StudentController │ SupervisorCtrl   │
│  ModuleLeaderController │ AdminController                  │
├─────────────────────────────────────────────────────────────┤
│                      SERVICES (DI)                        │
│  ProposalService │ MatchingService │ ResearchAreaService │
│  UserManagementService │ ExpertiseService                 │
├─────────────────────────────────────────────────────────────┤
│                      DATA ACCESS                          │
│           ApplicationDbContext (EF Core)                 │
├─────────────────────────────────────────────────────────────┤
│                      DOMAIN MODELS                        │
│  ApplicationUser │ ProjectProposal │ SupervisorMatch    │
│  ResearchArea │ SupervisorExpertise                      │
└─────────────────────────────────────────────────────────────┘
```

## Key Security Features

- **BlindProposalDto**: Ensures supervisors never see student identities
- **Identity Reveal**: Only happens after supervisor explicitly confirms match
- **Role-Based Access Control**: Authorization policies enforce role separation
- **Anti-Forgery Tokens**: All POST forms protected
- **Security Headers**: X-Content-Type-Options, X-Frame-Options, Referrer-Policy

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Coverage

- **Unit Tests**: MatchingServiceTests, ProposalServiceTests
- **Integration Tests**: ProposalIntegrationTests (full workflow)
- **Functional Test Cases**: Documented as comments for manual testing

## Project Structure

```
BlindMatchPAS/
├── Controllers/          # MVC Controllers
├── Data/                  # DbContext and SeedData
├── DTOs/                  # Data Transfer Objects
├── Models/                # Domain entities
├── Services/               # Business logic layer
│   └── Interfaces/         # Service contracts
└── Views/                 # Razor views
    ├── Account/
    ├── Admin/
    ├── Error/
    ├── ModuleLeader/
    ├── Shared/
    ├── Student/
    └── Supervisor/
```

## Blind Matching Flow

```
1. Student submits proposal (anonymous)
   ↓
2. Proposal appears in Supervisor browse (NO student info)
   ↓
3. Supervisor expresses interest → Status: UnderReview
   ↓
4. Supervisor confirms match → IsRevealed = TRUE
   ↓
5. BOTH parties can now see each other's details
```

## Technologies Used

- ASP.NET Core 8 MVC
- Entity Framework Core 8
- ASP.NET Core Identity
- Bootstrap 5
- xUnit + Moq + FluentAssertions