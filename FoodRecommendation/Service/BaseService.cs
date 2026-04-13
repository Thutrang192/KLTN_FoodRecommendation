using FoodRecommendation.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FoodRecommendation.Service
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        protected readonly FoodContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseService(FoodContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> GetDbSet()
        {
            return _dbSet;
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<T> GetEntityById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> Get(Expression<Func<T, bool>> expresstion)
        {
            return await _dbSet.FirstOrDefaultAsync(expresstion);
        }

        public async Task<List<T>> GetList(Expression<Func<T, bool>> expresstion)
        {
            return await _dbSet.Where(expresstion).ToListAsync();
        }

        public async Task<bool> Exist(Expression<Func<T, bool>> expresstion)
        {
            return await _dbSet.AnyAsync(expresstion);
        }

        public async Task<int> Count(Expression<Func<T, bool>> expresstion)
        {
            return await _dbSet.CountAsync(expresstion);
        }

        public async Task Insert(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task InsertMulti(List<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await GetEntityById(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
