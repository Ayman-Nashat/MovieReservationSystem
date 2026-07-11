# Movie Reservation System - Fixes Checklist

## 🔴 Critical Issues (Must Fix First)

### Security
- [ ] Add `[Authorize]` to `MoviesController` endpoints
- [ ] Add `[Authorize]` to `TheaterController` endpoints
- [ ] Add `[Authorize]` to `ReservationsController` endpoints
- [ ] Add `[Authorize]` to `GenresController` endpoints
- [ ] Add `[Authorize]` to `ShowtimeController` endpoints
- [ ] Fix `CreateReservationDto` to extract UserId from JWT token (not from request)
- [ ] Add validation attributes to `CreateReservationDto`
- [ ] Add JWT configuration section to `appsettings.json`
- [ ] Replace `new Random()` with `RandomNumberGenerator` for OTP generation

### Data Integrity
- [ ] Fix `MoviesController.Update()` - use `UpdateMovieAsync()` instead of `AddMovieAsync()`
- [ ] Add transaction wrapper for theater + seat creation
- [ ] Add optimistic concurrency control to reservation creation
- [ ] Replace generic `Exception` with custom exceptions in `ShowtimeService`

### Error Handling
- [ ] Create global exception handler middleware
- [ ] Create standardized `ApiResponse<T>` class
- [ ] Add try-catch blocks to all controller actions

---

## 🟡 High Priority Issues

### Code Quality
- [ ] Remove direct `AppDbContext` injection from `TheaterService` (use repository)
- [ ] Add null checks to all service methods
- [ ] Create `ReservationStatus` enum (replace magic strings)
- [ ] Add `ILogger` to all services
- [ ] Fix namespace typo: `Repositort` → `Repository`

### API Design
- [ ] Add pagination to all `GetAll()` methods
- [ ] Add filtering/sorting query parameters
- [ ] Ensure all endpoints return DTOs (not entities)
- [ ] Fix HTTP status codes (204 for DELETE, etc.)

### Missing Features
- [ ] Create unit test project
- [ ] Create integration test project
- [ ] Add XML documentation comments for Swagger
- [ ] Add health check endpoints
- [ ] Configure CORS policy

---

## 🟢 Medium Priority Issues

### Performance
- [ ] Add caching for movies and theaters
- [ ] Fix N+1 queries in `MoviesController.GetAll()`
- [ ] Review and add database indexes

### Configuration
- [ ] Create `.gitignore` file
- [ ] Add `appsettings.Production.json`
- [ ] Move secrets to User Secrets or Azure Key Vault

### Documentation
- [ ] Add XML comments to all public methods
- [ ] Create API documentation
- [ ] Add architecture diagrams

---

## 🔵 Low Priority / Enhancements

### Features
- [ ] Email notifications for reservations
- [ ] User profile management
- [ ] Movie search by genre
- [ ] Showtime availability API
- [ ] Payment webhook handling
- [ ] Admin dashboard endpoints
- [ ] Audit logging
- [ ] Rate limiting

### Code Organization
- [ ] Consider MediatR for CQRS
- [ ] Consider AutoMapper for DTO mapping
- [ ] Consider FluentValidation

### Monitoring
- [ ] Add Application Insights
- [ ] Add request/response logging
- [ ] Add performance counters

---

## Quick Reference: Files to Fix

### Controllers (Add Authorization)
- `Controllers/MoviesController.cs`
- `Controllers/TheaterController.cs`
- `Controllers/ReservationsController.cs`
- `Controllers/GenresController.cs`
- `Controllers/ShowtimeController.cs`

### Services (Add Logging & Fix Issues)
- `Service/MovieService.cs`
- `Service/TheaterService.cs`
- `Service/ReservationService.cs`
- `Service/ShowtimeService.cs`

### DTOs (Add Validation)
- `DTOs/Reservation/CreateReservationDto.cs`

### Configuration
- `appsettings.json` (add JWT section)
- Create `.gitignore`

### Entities (Use Enums)
- `Core/Entities/Reservation.cs` (add ReservationStatus enum)

---

## Priority Order

1. **Week 1**: All Critical Security Issues
2. **Week 2**: Data Integrity & Bug Fixes
3. **Week 3**: Code Quality Improvements
4. **Week 4**: Testing & Documentation
5. **Week 5**: Performance & Production Readiness

---

**Last Updated**: $(Get-Date)


