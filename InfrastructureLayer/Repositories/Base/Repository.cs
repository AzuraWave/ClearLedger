using DomainLayer.Entities.Base;

using InfrastructureLayer.Context;
using Microsoft.EntityFrameworkCore;
using ApplicationLayer.Interfaces.Repositories.Base;

namespace InfrastructureLayer.Repositories.Base
{
    public class Repository<T> : IRepository<T> where T : Entity
    {

        protected readonly LedgerDbContext _db;

        public Repository(LedgerDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(T entity)
        {
            await _db.Set<T>().AddAsync(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            _db.Set<T>().Remove(entity);
        }

        public  async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _db.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _db.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _db.Set<T>().Update(entity); 
            await Task.CompletedTask;
        }
    }
}
