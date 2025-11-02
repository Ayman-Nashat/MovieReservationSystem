# Movie Reservation System 🎬

A comprehensive ASP.NET Core 8.0 Web API application for managing movie theaters, showtimes, and seat reservations.

## 📋 Features

- **User Authentication**: Registration, login, email confirmation, and OTP-based password reset
- **Movie Management**: CRUD operations for movies with genre support
- **Theater Management**: Create and manage theaters with seating configuration
- **Reservation System**: Book seats for showtimes with conflict prevention
- **Role-Based Access Control**: Admin and User roles
- **Email Notifications**: Email confirmation and password reset via OTP

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- SQL Server (or SQL Server Express)
- Visual Studio 2022 / VS Code / Rider (or any .NET IDE)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd "Movie Reservation System"
   ```

2. **Configure Database Connection**
   
   Update `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=MovieReservationDB;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Configure Email Settings** (Optional - required for email features)
   
   Add email configuration in `appsettings.json`:
   ```json
   {
     "MailSettings": {
       "Host": "smtp.gmail.com",
       "Port": 587,
       "From": "your-email@gmail.com",
       "Password": "your-app-password"
     }
   }
   ```

4. **Configure Admin Account** (Optional)
   
   Set admin credentials in `appsettings.json`:
   ```json
   {
     "AdminConfiguration": {
       "Email": "admin@example.com",
       "Password": "Admin@123"
     }
   }
   ```

5. **Run Database Migrations**
   ```bash
   cd "Movie Reservation System"
   dotnet ef database update --project ../MovieReservationSystem.Repository
   ```

6. **Run the Application**
   ```bash
   dotnet run --project "Movie Reservation System"
   ```

7. **Access Swagger UI**
   
   Navigate to: `https://localhost:5001/swagger` (or the port shown in terminal)

## 🏗️ Project Structure

```
Movie Reservation System/
├── Movie Reservation System/          # API Layer (Controllers, DTOs)
├── MovieReservationSystem.Core/        # Domain Layer (Entities, Interfaces)
├── MovieReservationSystem.Service/     # Business Logic Layer
└── MovieReservationSystem.Repository/  # Data Access Layer
```

## 📡 API Endpoints

### Authentication (`/api/Account`)
- `POST /api/Account/register` - Register new user
- `POST /api/Account/login` - User login
- `GET /api/Account/ConfirmEmail` - Confirm email
- `POST /api/Account/ForgotPassword` - Request password reset OTP
- `POST /api/Account/ResetPassword` - Reset password with OTP

### Movies (`/api/Movies`)
- `GET /api/Movies` - Get all movies
- `GET /api/Movies/{id}` - Get movie by ID
- `POST /api/Movies` - Create movie

### Theaters (`/api/Theater`)
- `GET /api/Theater` - Get all theaters
- `GET /api/Theater/{id}` - Get theater by ID
- `POST /api/Theater` - Create theater
- `PUT /api/Theater/{id}` - Update theater
- `DELETE /api/Theater/{id}` - Delete theater

### Genres (`/api/Genres`)
- `GET /api/Genres` - Get all genres
- `GET /api/Genres/{id}` - Get genre by ID
- `POST /api/Genres` - Create genre
- `PUT /api/Genres/{id}` - Update genre

### Reservations (`/api/Reservations`)
- `POST /api/Reservations` - Create reservation
- `GET /api/Reservations/{id}` - Get reservation by ID
- `GET /api/Reservations/user/{userId}` - Get user's reservations
- `GET /api/Reservations/showtime/{showtimeId}` - Get showtime reservations
- `PUT /api/Reservations/{id}/confirm` - Confirm reservation
- `DELETE /api/Reservations/{id}` - Cancel reservation

## 🗄️ Database Schema

The system uses SQL Server with Entity Framework Core. Key entities:

- **User**: Authentication and user management
- **Movie**: Movie information with genres
- **Theater**: Theater locations with seating configuration
- **Showtime**: Movie showtimes at theaters
- **Seat**: Individual seats in theaters
- **Reservation**: User seat reservations
- **Genre**: Movie genres
- **SeatHold**: Temporary seat holds during booking

See [SYSTEM_DESCRIPTION.md](SYSTEM_DESCRIPTION.md) for detailed entity relationships and schema.

## 🔧 Technology Stack

- **.NET 8.0** - Framework
- **ASP.NET Core Identity** - Authentication & Authorization
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Database
- **MailKit** - Email service
- **Swagger** - API documentation

## 📝 Configuration

### Required Configuration

1. **Database Connection String** in `appsettings.json`
2. **Email Settings** (if using email features) in `appsettings.json`

### Optional Configuration

- **Admin Account**: Configured in `appsettings.json` → `AdminConfiguration`
- **Secrets**: Can be stored in `appsettings.Secrets.json` (not tracked in git)

## 🔐 Security

- Email confirmation required for registration
- Password reset via OTP (6-digit code, 10-minute expiration)
- Role-based access control (Admin/User)
- All foreign keys use `DeleteBehavior.Restrict` to prevent data loss

## 📚 Documentation

For detailed technical documentation, architecture details, and complete API reference, see:
- **[SYSTEM_DESCRIPTION.md](SYSTEM_DESCRIPTION.md)** - Comprehensive system documentation

## 🐛 Known Issues / Notes

- JWT token generation is commented out (currently returns user info without token)
- Payment functionality is planned but not yet implemented

## 👤 Default Admin

On first run, an admin user is automatically seeded based on `AdminConfiguration` in `appsettings.json`. Make sure to configure this section or modify the default credentials.

## 📄 License

[Add your license here]

## 🤝 Contributing

[Add contribution guidelines here]

---

**Note**: This README provides a quick overview. For detailed technical information, entity relationships, and architecture details, refer to `SYSTEM_DESCRIPTION.md`.

