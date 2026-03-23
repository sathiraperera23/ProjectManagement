using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Infrastructure.Persistence;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;
using System.Linq.Expressions;

namespace TaskManagementApi.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                if (entity is BaseEntity baseEntity)
                {
                    baseEntity.IsDeleted = true;
                    baseEntity.DeletedAt = DateTime.UtcNow;
                    _dbSet.Update(entity);
                }
                else
                {
                    _dbSet.Remove(entity);
                }
                await _context.SaveChangesAsync();
            }
        }

        public IQueryable<T> Query()
        {
            return _dbSet;
        }
    }
}
