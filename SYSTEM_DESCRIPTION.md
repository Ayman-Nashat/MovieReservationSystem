# Movie Reservation System - Complete System Documentation

## 🎬 System Overview

The **Movie Reservation System** is a comprehensive ASP.NET Core 8.0 Web API application that enables users to browse movies, reserve theater seats, and manage their movie reservations. It follows a clean architecture pattern with clear separation of concerns across multiple layers.

---

## 🏗️ Architecture & Technology Stack

### **Technology Stack**
- **Framework**: ASP.NET Core 8.0 (.NET 8.0)
- **Database**: SQL Server with Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity with Role-Based Access Control (RBAC)
- **Email Service**: MailKit/MimeKit for email functionality
- **API Documentation**: Swagger/OpenAPI

### **Project Structure (Clean Architecture)**

The solution is organized into **4 main projects**:

1. **`Movie Reservation System`** (API Layer)
   - Controllers (REST API endpoints)
   - DTOs (Data Transfer Objects)
   - Configuration files (appsettings.json, Program.cs)
   - Helpers and Settings

2. **`MovieReservationSystem.Core`** (Domain Layer)
   - Entities (Domain models)
   - Repository Contracts (Interfaces)
   - Service Contracts (Interfaces)

3. **`MovieReservationSystem.Repository`** (Data Access Layer)
   - AppDbContext (EF Core DbContext)
   - GenericRepository and specific repositories
   - Entity configurations
   - Database migrations
   - Database seeding

4. **`MovieReservationSystem.Service`** (Business Logic Layer)
   - Service implementations
   - Business logic and validation

---

## 📊 Database Schema & Entities

### **Core Entities**

#### **1. User** (Extends `IdentityUser`)
- **Properties**: 
  - `Id` (string - from IdentityUser)
  - `Name` (string)
  - `Email` (string, unique index)
  - `UserName` (string)
  - `PhoneNumber` (string)
  - `ProfilePictureUrl` (string, nullable)
  - `PasswordResetCode` (string, nullable) - For OTP-based password reset
  - `ResetCodeExpiration` (DateTime?, nullable)
- **Relationships**: 
  - One-to-Many with `Reservation`
- **Security**: Email confirmation required, password reset via OTP

#### **2. Theater**
- **Properties**:
  - `Id` (int, primary key)
  - `Name` (string, required, max 100)
  - `Location` (string, max 200)
  - `TotalSeats` (int)
  - `Rows` (int?, nullable)
  - `Columns` (int?, nullable)
  - `TheaterType` (string, max 50, default "Standard") - e.g., "IMAX", "Standard"
  - `IsActive` (bool, default true)
  - `CreatedAt` (DateTime, default UtcNow)
  - `UpdatedAt` (DateTime?, nullable)
  - `RowVersion` (byte[], timestamp for concurrency)
  - `ExternalId` (Guid)
- **Relationships**: 
  - One-to-Many with `Seat`
  - One-to-Many with `Showtime`
- **Constraints**: Unique index on (Name, Location)

#### **3. Movie**
- **Properties**:
  - `Id` (int, primary key)
  - `Title` (string)
  - `Description` (string)
  - `DurationMinutes` (int)
- **Relationships**: 
  - One-to-Many with `Showtime`
  - Many-to-Many with `Genre` (via `MovieGenre` junction table)

#### **4. Genre**
- **Properties**:
  - `Id` (int, primary key)
  - `Name` (string)
- **Relationships**: 
  - Many-to-Many with `Movie` (via `MovieGenre`)

#### **5. MovieGenre** (Junction Table)
- **Properties**:
  - `MovieId` (int, composite key)
  - `GenreId` (int, composite key)
- **Purpose**: Links movies to genres (many-to-many relationship)

#### **6. Showtime**
- **Properties**:
  - `Id` (int, primary key)
  - `StartTime` (DateTime)
  - `EndTime` (DateTime)
  - `MovieId` (int, foreign key)
  - `TheaterId` (int, foreign key)
- **Relationships**: 
  - Many-to-One with `Movie`
  - Many-to-One with `Theater`
  - One-to-Many with `Reservation`
  - One-to-Many with `SeatHold`
- **Constraints**: Non-unique index on (TheaterId, StartTime)

#### **7. Seat**
- **Properties**:
  - `Id` (int, primary key)
  - `SeatNumber` (string) - e.g., "A1", "B5"
  - `SeatType` (string) - e.g., "Standard", "VIP", "Premium"
  - `TheaterId` (int, foreign key)
- **Relationships**: 
  - Many-to-One with `Theater`
  - One-to-Many with `Reservation`
  - One-to-Many with `SeatHold`
- **Constraints**: Unique index on (TheaterId, SeatNumber)

#### **8. Reservation**
- **Properties**:
  - `Id` (int, primary key)
  - `ReservationDate` (DateTime)
  - `Status` (string, default "Pending") - Values: "Pending", "Confirmed", "Canceled"
  - `UserId` (string, foreign key to User)
  - `ShowtimeId` (int, foreign key)
  - `SeatId` (int, foreign key)
- **Relationships**: 
  - Many-to-One with `User`
  - Many-to-One with `Showtime`
  - Many-to-One with `Seat`
- **Constraints**: 
  - Unique index on (ShowtimeId, SeatId) - Prevents double booking
  - DeleteBehavior.Restrict on all foreign keys

#### **9. SeatHold**
- **Properties**:
  - `Id` (int, primary key)
  - `ShowtimeId` (int, foreign key)
  - `SeatId` (int, foreign key)
  - `UserId` (string)
  - `ExpiresAt` (DateTime) - When the hold expires
  - `CreatedAt` (DateTime, default UtcNow)
- **Purpose**: Temporarily holds seats during reservation process (prevents double booking)
- **Relationships**: 
  - Many-to-One with `Showtime`
  - Many-to-One with `Seat`
- **Constraints**: Index on (ShowtimeId, SeatId)

#### **10. Email** (Domain Model)
- **Properties**:
  - `To` (string)
  - `Subject` (string)
  - `Body` (string)
- **Purpose**: Represents email data for sending emails

---

## 🔗 Entity Relationships Summary

```
User
  └── 1:N ──> Reservation

Theater
  ├── 1:N ──> Seat
  └── 1:N ──> Showtime

Movie
  ├── 1:N ──> Showtime
  └── N:M ──> Genre (via MovieGenre)

Showtime
  ├── N:1 ──> Movie
  ├── N:1 ──> Theater
  ├── 1:N ──> Reservation
  └── 1:N ──> SeatHold

Seat
  ├── N:1 ──> Theater
  ├── 1:N ──> Reservation
  └── 1:N ──> SeatHold

Reservation
  ├── N:1 ──> User
  ├── N:1 ──> Showtime
  └── N:1 ──> Seat
```

---

## 🌐 API Endpoints

### **Authentication & Account Management** (`/api/Account`)

1. **POST** `/api/Account/register`
   - **Purpose**: Register a new user
   - **Body**: `RegisterDTO` (FullName, UserName, Email, PhoneNumber, Password)
   - **Behavior**: Creates user, assigns "User" role, sends email confirmation link
   - **Response**: Success message

2. **POST** `/api/Account/login`
   - **Purpose**: User login (currently returns user info, JWT commented out)
   - **Body**: `LoginDTO` (Email, Password)
   - **Response**: User info with role

3. **GET** `/api/Account/ConfirmEmail`
   - **Purpose**: Confirm user email via token
   - **Query Params**: `token`, `email`
   - **Response**: Success/Error message

4. **POST** `/api/Account/ForgotPassword`
   - **Purpose**: Send OTP code for password reset
   - **Query Param**: `email`
   - **Behavior**: Generates 6-digit OTP, stores in DB with 10-minute expiration, sends email
   - **Response**: Success message

5. **POST** `/api/Account/ResetPassword`
   - **Purpose**: Reset password using OTP code
   - **Body**: `ResetPasswordDto` (Email, Code, NewPassword)
   - **Response**: Success/Error message

### **Movies** (`/api/Movies`)

1. **GET** `/api/Movies`
   - **Purpose**: Get all movies with genres
   - **Response**: List of `MovieDTO` (Id, Title, Description, DurationMinutes, Genres[])

2. **GET** `/api/Movies/{id}`
   - **Purpose**: Get movie by ID with genres
   - **Response**: `MovieDTO`

3. **POST** `/api/Movies`
   - **Purpose**: Create a new movie
   - **Body**: `AddMovieDTO`
   - **Response**: Created movie

### **Theaters** (`/api/Theater`)

1. **GET** `/api/Theater`
   - **Purpose**: Get all theaters
   - **Response**: List of `TheaterReadDTO`

2. **GET** `/api/Theater/{id}`
   - **Purpose**: Get theater by ID
   - **Response**: `TheaterReadDTO`

3. **POST** `/api/Theater`
   - **Purpose**: Create a new theater
   - **Body**: `TheaterCreateDTO` (Name, Location, TotalSeats, Rows, Columns, TheaterType, IsActive)
   - **Response**: `TheaterReadDTO` with generated ID
   - **Note**: ✅ **Fixed** - Now properly saves to database with `SaveChangesAsync()`

4. **PUT** `/api/Theater/{id}`
   - **Purpose**: Update theater
   - **Body**: `TheaterUpdateDTO`
   - **Response**: Updated theater

5. **DELETE** `/api/Theater/{id}`
   - **Purpose**: Delete theater
   - **Response**: Success message

### **Genres** (`/api/Genres`)

1. **GET** `/api/Genres`
   - **Purpose**: Get all genres
   - **Response**: List of `Genre`

2. **GET** `/api/Genres/{id}`
   - **Purpose**: Get genre by ID
   - **Response**: `Genre`

3. **POST** `/api/Genres`
   - **Purpose**: Create a new genre
   - **Body**: `AddGenreDTO` (Name)
   - **Response**: Created genre

4. **PUT** `/api/Genres/{id}`
   - **Purpose**: Update genre
   - **Body**: `AddGenreDTO` (Name)
   - **Response**: Updated genre

### **Reservations** (`/api/Reservations`)

1. **POST** `/api/Reservations`
   - **Purpose**: Create a new reservation
   - **Body**: `CreateReservationDto` (UserId, ShowtimeId, SeatId)
   - **Behavior**: 
     - Validates seat is not already reserved
     - Validates seat is not on hold
     - Creates reservation with status "Pending"
   - **Response**: Created reservation (201) or Conflict if seat unavailable (409)

2. **GET** `/api/Reservations/{id}`
   - **Purpose**: Get reservation by ID
   - **Response**: `Reservation`

3. **GET** `/api/Reservations/user/{userId}`
   - **Purpose**: Get all reservations for a user
   - **Response**: List of `Reservation`

4. **GET** `/api/Reservations/showtime/{showtimeId}`
   - **Purpose**: Get all reservations for a showtime
   - **Response**: List of `Reservation`

5. **PUT** `/api/Reservations/{id}/confirm`
   - **Purpose**: Confirm a reservation (change status to "Confirmed")
   - **Response**: Success message

6. **DELETE** `/api/Reservations/{id}`
   - **Purpose**: Cancel a reservation (change status to "Canceled")
   - **Response**: Success message

---

## 🔧 Service Layer Architecture

### **Services Overview**

All services follow the repository pattern and implement business logic:

1. **TheaterService** ✅
   - Uses `IGenericRepository<Theater, int>`
   - **Fixed**: Now properly calls `SaveChangesAsync()` after add/update/delete operations
   - Methods: `GetAllTheatersAsync()`, `GetTheaterByIdAsync()`, `AddTheaterAsync()`, `UpdateTheaterAsync()`, `DeleteTheaterAsync()`

2. **MovieService**
   - Uses `IMovieRepository` (extends generic repository)
   - Methods: `GetAllMoviesAsync()`, `GetMovieByIdAsync()`, `SearchByNameAsync()`, `AddMovieAsync()`, `UpdateMovieAsync()`, `DeleteMovieAsync()`

3. **GenreService**
   - Uses `IGenericRepository<Genre, int>` + `AppDbContext` for SaveChanges
   - Methods: `GetAllAsync()`, `GetByIdAsync()`, `AddAsync()`, `Update()`, `Delete()`, `AddGenreToMovieAsync()`

4. **ReservationService**
   - Uses `IReservationRepository` + `ISeatHoldRepository`
   - **Business Logic**: 
     - Prevents double booking (checks existing reservations)
     - Prevents booking held seats
     - Manages reservation status (Pending → Confirmed/Canceled)
   - Methods: `CreateReservationAsync()`, `GetUserReservationsAsync()`, `GetShowtimeReservationsAsync()`, `GetReservationByIdAsync()`, `ConfirmReservationAsync()`, `CancelReservationAsync()`

5. **EmailSettings** (IMailService)
   - Handles email sending via MailKit
   - Used for: email confirmation, password reset OTP

---

## 📁 Repository Layer

### **Generic Repository Pattern**

- **GenericRepository<TEntity, TKey>**: Base repository with common CRUD operations
  - `AddAsync()`, `GetAllAsync()`, `GetByIdAsync()`, `Update()`, `Remove()`
  - **Note**: Does NOT call `SaveChangesAsync()` - must be called at service layer

### **Specific Repositories**

1. **MovieRepository** (extends GenericRepository)
   - `GetMovieWithDetailsAsync()` - Includes genres and showtimes
   - `GetAllMoviesWithDetailsAsync()` - Includes genres and showtimes
   - `SearchByNameAsync()` - Search movies by title
   - `AddMovieWithTheaterAndShowtimesAsync()` - Complex operation with multiple entities
   - `SaveChangesAsync()` - Exposes DbContext SaveChanges

2. **ReservationRepository**
   - `AddReservationAsync()` - Adds reservation
   - `GetReservationByIdAsync()` - Get by ID
   - `GetReservationsByUserAsync()` - Get user's reservations
   - `GetReservationsByShowtimeAsync()` - Get showtime's reservations
   - `IsSeatReservedAsync()` - Check if seat is reserved for a showtime
   - `UpdateReservationAsync()` - Update reservation
   - `SaveChangesAsync()` - Exposes DbContext SaveChanges

3. **SeatHoldRepository**
   - `HoldSeatAsync()` - Create a seat hold
   - `ReleaseHoldAsync()` - Remove a seat hold
   - `IsSeatHeldAsync()` - Check if seat is currently held
   - `CleanExpiredHoldsAsync()` - Clean up expired holds

---

## 🔐 Security & Authentication

### **ASP.NET Core Identity Configuration**
- **Email Confirmation**: Required (`SignIn.RequireConfirmedEmail = true`)
- **Roles**: 
  - `Admin` - Seeded on startup from `appsettings.json` (`AdminConfiguration` section)
  - `User` - Assigned to new registrations
- **Password Reset**: OTP-based (6-digit code, 10-minute expiration)
- **Token Lifespan**: 180 minutes for data protection tokens

### **Database Constraints**
- All foreign keys use `DeleteBehavior.Restrict` to prevent cascade deletes
- Unique constraints prevent:
  - Duplicate seat reservations (ShowtimeId + SeatId)
  - Duplicate seat numbers in theaters (TheaterId + SeatNumber)
  - Duplicate theaters (Name + Location)
  - Duplicate user emails

---

## 📧 Email System

### **Configuration**
- Email settings stored in `appsettings.json` under `MailSettings` section
- Uses MailKit/MimeKit for SMTP email delivery

### **Email Use Cases**
1. **Email Confirmation**: Sent on user registration with confirmation link
2. **Password Reset OTP**: 6-digit code sent to user's email (valid for 10 minutes)

---

## 🗄️ Database Configuration

### **Connection String**
- Stored in `appsettings.json` under `ConnectionStrings.DefaultConnection`
- SQL Server database

### **Migrations**
- Entity Framework Core migrations stored in `MovieReservationSystem.Repository/Data/Migrations/`
- 15 migration files exist (as of latest state)

### **Database Seeding**
- **AppDbSeeder**: Seeds admin user on application startup
- Admin credentials configured in `appsettings.json` → `AdminConfiguration` section
- Creates roles if they don't exist: "Admin", "User"

---

## 🎯 Current Implementation Status

### **✅ Fully Implemented Features**
- User registration with email confirmation
- User login (JWT generation commented out)
- Password reset via OTP
- Movie CRUD operations
- Theater CRUD operations (✅ **Fixed** - now saves to DB)
- Genre CRUD operations
- Reservation creation with conflict prevention
- Reservation status management (Confirm/Cancel)
- Seat hold mechanism
- Email service integration
- Role-based access control setup
- Database migrations and seeding

### **⚠️ Partially Implemented / Notes**
- JWT token generation is commented out in `AccountController.Login()` - currently returns user info without token
- Password reset uses OTP instead of token-based links (both approaches exist in code)
- Payment entity is commented out in Reservation entity (future feature)
- Some repository methods may need explicit `SaveChangesAsync()` calls (pattern inconsistent across services)

### **📝 Code Quality Notes**
- **TheaterService**: ✅ **Recently Fixed** - Now properly saves changes to database
- Some services inject `AppDbContext` directly for `SaveChangesAsync()`, others use repository's `SaveChangesAsync()` method
- DTOs separate API layer from domain models (good practice)

---

## 🔄 Dependency Injection Setup

Registered in `Program.cs`:

```csharp
// Repositories
AddScoped<IMovieRepository, MovieRepository>()
AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>))
AddScoped<IReservationRepository, ReservationRepository>()
AddScoped<ISeatHoldRepository, SeatHoldRepository>()

// Services
AddScoped<IMovieService, MovieService>()
AddScoped<ITheaterService, TheaterService>()
AddScoped<IGenreService, GenreService>()
AddScoped<IReservationService, ReservationService>()
AddTransient<IMailService, EmailSettings>()

// Configuration
Configure<AdminConfiguration>(...)
Configure<MailSettings>(...)
```

---

## 📦 Key NuGet Packages

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (8.0.20)
- `Microsoft.EntityFrameworkCore` (8.0.20)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.20)
- `Microsoft.EntityFrameworkCore.Tools` (8.0.20)
- `MailKit` (4.14.1)
- `MimeKit` (4.14.0)
- `Swashbuckle.AspNetCore` (6.4.0) - Swagger

---

## 🚀 API Base URL & Documentation

- **Swagger UI**: Available in Development environment (`/swagger`)
- **Base Route**: `/api/[controller]`
- **Content Type**: JSON (application/json)

---

## 📌 Important File Locations

### **Controllers**
- `Movie Reservation System/Controllers/AccountController.cs`
- `Movie Reservation System/Controllers/MoviesController.cs`
- `Movie Reservation System/Controllers/TheaterController.cs`
- `Movie Reservation System/Controllers/GenresController.cs`
- `Movie Reservation System/Controllers/ReservationsController.cs`

### **Entities**
- `MovieReservationSystem.Core/Entities/` (all domain models)

### **Services**
- `MovieReservationSystem.Service/` (all service implementations)

### **Repositories**
- `MovieReservationSystem.Repository/Repositories/` (repository implementations)

### **Database Context**
- `MovieReservationSystem.Repository/Data/AppDbContext.cs`

### **Configuration**
- `Movie Reservation System/Program.cs` - Main application setup
- `Movie Reservation System/appsettings.json` - Configuration file
- `Movie Reservation System/appsettings.Secrets.json` - Secrets (optional)

---

## 🎓 Design Patterns Used

1. **Repository Pattern**: Abstraction layer for data access
2. **Generic Repository**: Reusable base repository for common operations
3. **Service Layer Pattern**: Business logic separation
4. **DTO Pattern**: Data transfer objects for API contracts
5. **Dependency Injection**: Constructor injection throughout
6. **Clean Architecture**: Clear separation of layers (API → Service → Repository → Data)

---

## 📝 Summary for AI Models

This is a **fully functional Movie Reservation System** built with ASP.NET Core 8.0, implementing:
- **User authentication** with email confirmation and OTP-based password reset
- **Movie, Theater, and Genre management** with full CRUD operations
- **Reservation system** with conflict prevention (no double booking)
- **Seat hold mechanism** for temporary reservations
- **Role-based access control** (Admin/User)
- **Email notifications** for account and security events
- **SQL Server database** with Entity Framework Core
- **RESTful API** with Swagger documentation

**Recent Fix**: Theater creation now properly persists to database (added `SaveChangesAsync()` call in `TheaterService`).

The system follows clean architecture principles with clear separation between API, Service, Repository, and Core layers. All entities have proper relationships, constraints, and business logic validation.



