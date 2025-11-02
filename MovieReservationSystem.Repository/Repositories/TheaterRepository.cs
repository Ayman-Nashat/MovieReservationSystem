using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Repository.Contract;
using MovieReservationSystem.Repository.Data;

namespace MovieReservationSystem.Repository.Repositories
{
    public class TheaterRepository : GenericRepository<Theater, int>, ITheaterRepository
    {
        private readonly AppDbContext _context;

        public TheaterRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
