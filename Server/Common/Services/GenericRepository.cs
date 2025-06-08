using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Interfaces;
using msih.p4g.Server.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace msih.p4g.Server.Common.Services
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsActive && !e.IsDeleted);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.Where(e => e.IsActive && !e.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(e => e.IsActive && !e.IsDeleted).Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Gets all records, including inactive and deleted ones.
        /// </summary>
        public async Task<IEnumerable<T>> GetAllWithInactiveOrDeletedAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Finds records by predicate, including inactive and deleted ones.
        /// </summary>
        public async Task<IEnumerable<T>> FindWithInactiveOrDeletedAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.IsActive = false;
            _dbSet.Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
