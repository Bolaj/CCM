# CityChoir Management API

A RESTful API for managing choir operations including member management, rehearsals, attendance tracking, and permission requests.

## Tech Stack

- **Framework**: ASP.NET Core (.NET 10)
- **Database**: MySQL (Entity Framework Core)
- **Authentication**: JWT Bearer Token
- **Email**: MailKit (SMTP)
- **Architecture**: Clean Architecture


## Project Structure

CityChoir.API → Controllers, Program.cs
CityChoir.Application → Interfaces, DTOs, Services (business logic)
CityChoir.Domain → Entities, Enums
CityChoir.Infrastructure → Repositories, Service implementations, DbContext


## Features

- **Auth** — Register, Login, Email Verification, Password Reset
- **Admin** — Manage users, assign roles, CRUD rehearsals, approve/decline permissions, attendance reports
- **Attendance** — Geo-validated attendance marking using Haversine formula
- **Permissions** — Members request absence, admin approves/declines with email notifications
- **Email Notifications** — Triggered on registration, attendance, permission lifecycle

## Roles

| Role | Access |
|------|--------|
| `SUPER_ADMIN` | Full access |
| `ADMIN` | Full admin access |
| `DIRECTOR` | TBD |
| `PART_LEADER` | TBD |
| `MEMBER` | Attendance, permissions, view rehearsals |

## API Endpoints

### Auth — `/api/auth` (Public)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/register` | Register a new member |
| POST | `/login` | Login and get JWT token |
| GET | `/verify-email` | Verify email address |
| POST | `/resend-verification` | Resend verification email |

### Admin — `/api/admin` (ADMIN, SUPER_ADMIN only)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/users` | Get all members |
| GET | `/users/{id}` | Get member by ID |
| PUT | `/users/assign-role` | Assign role to member |
| DELETE | `/users/{id}` | Delete member |
| GET | `/rehearsals` | Get all rehearsals |
| GET | `/rehearsals/{id}` | Get rehearsal by ID |
| POST | `/rehearsals` | Create rehearsal |
| PUT | `/rehearsals` | Update rehearsal |
| DELETE | `/rehearsals/{id}` | Delete rehearsal |
| GET | `/permissions` | Get all permission requests |
| GET | `/permissions/{id}` | Get permission request by ID |
| PUT | `/permissions/{id}/approve` | Approve permission request |
| PUT | `/permissions/{id}/decline` | Decline permission request |
| GET | `/attendance` | Get attendance report |

### Member — `/api/member` (Authenticated)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/rehearsals` | Get upcoming rehearsals |
| GET | `/rehearsals/{id}` | Get rehearsal by ID |

### Attendance — `/api/attendance` (Authenticated)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/mark` | Mark attendance (geo-validated) |

### Permissions — `/api/permissions` (Authenticated)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Submit permission request |
| GET | `/my/{userId}` | Get my permission requests |

## Getting Started

### Prerequisites
- .NET 10 SDK
- MySQL
- SMTP account (Outlook or custom)

### Setup

1. Clone the repo
```bash
git clone https://github.com/Bolaj/CCM.git
cd CCM
```

2. Set these environment variables in the shell that starts the API (ASP.NET Core does not load a `.env` file by default):
```dotenv
ConnectionStrings__DBConnectionString=Server=localhost;Database=citychoirapi;User=root;Password=yourpassword;
Email__Username=youremail@outlook.com
Email__FromEmail=youremail@outlook.com
Email__AuthenticationMethod=OAuth2
Email__AccessToken=your-oauth2-access-token
```

Outlook.com SMTP requires Modern Auth. The API uses MailKit XOAUTH2 with port 587 and STARTTLS; it does not use the Microsoft account password directly. Set these values as process environment variables (or your local secret-management solution). A short-lived access token must include the `https://outlook.office.com/SMTP.Send` scope and will need to be refreshed when it expires. Never commit the token or account password to `appsettings.json`.

3. Update `appsettings.json` with your JWT and email settings

4. Run migrations
```bash
dotnet ef database update --project CityChoir.Infrastructure --startup-project CityChoir.API
```

5. Run the API
```bash
dotnet run --project CityChoir.API
```

6. Open Swagger at `http://localhost:5243/swagger`

## Attendance Flow

Members mark attendance by submitting their geolocation. The API validates they are within the rehearsal venue radius using the Haversine formula.


Member clicks "Mark Attendance"
↓
Browser gets geolocation (lat, lng)
↓
POST /api/attendance/mark
↓
Backend finds active rehearsal
↓
Haversine distance check against venue (lat, lng, radiusMeters)
↓
Within radius → mark present + send confirmation email
Outside radius → return error with distance


## Permission Request Flow

Member submits request → PENDING + email to member + email to admins
Admin approves → APPROVED + email to member
Admin declines → DECLINED + decline reason + email to member