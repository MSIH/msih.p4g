using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using msih.p4g.Server.Common.Models;

namespace msih.p4g.Server.Common.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity); // Soft delete
        Task SaveChangesAsync();

        /// <summary>
        /// Gets all records, including inactive and deleted ones.
        /// </summary>
        Task<IEnumerable<T>> GetAllWithInactiveOrDeletedAsync();

        /// <summary>
        /// Finds records by predicate, including inactive and deleted ones.
        /// </summary>
        Task<IEnumerable<T>> FindWithInactiveOrDeletedAsync(Expression<Func<T, bool>> predicate);
    }
}
