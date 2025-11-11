using Microsoft.AspNetCore.Identity;
using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Service.Contract
{
    public interface IAuthService
    {
        Task<string> CreateTokenAsync(User user, UserManager<User> userManager);
    }
}
