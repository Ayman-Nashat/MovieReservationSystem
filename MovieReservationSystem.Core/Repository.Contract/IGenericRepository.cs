using MovieReservationSystem.Core.Entities;

namespace MovieReservationSystem.Core.Repository.Contract
{
    public interface IGenericRepository<TEntity, Tkey> where TEntity : BaseEntity<Tkey>
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task<TEntity?> GetByIdAsync(Tkey id);
        void Remove(TEntity entity);
        void Update(TEntity entity);

    }
}
