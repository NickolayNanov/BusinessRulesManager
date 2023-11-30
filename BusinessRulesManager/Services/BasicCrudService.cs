using AutoMapper;
using BusinessRulesManager.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BusinessRulesManager.Services
{
    public class BasicCrudService<T, TKey> : IBasicCrudService<T, TKey> where T : Entity<TKey>
    {
        protected readonly IMapper mapper;
        protected readonly ApplicationDbContext context;

        public BasicCrudService(
            IMapper mapper,
            ApplicationDbContext context)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public virtual async Task<T> CreateAsync(T model)
        {
            try
            {
                var entityResult = await context.Set<T>().AddAsync(model);
                await context.SaveChangesAsync();
                return entityResult.Entity;
            }
            catch (Exception ex)
            
            {
                return null;
            }
        }

        public virtual async Task<int> BulkInsertAsync(IEnumerable<T> entities)
        {
            await context.Set<T>().AddRangeAsync(entities);
            return await context.SaveChangesAsync();
        }

        public async Task<T> CreateAsync<TModel>(TModel model)
        {
            return await CreateAsync(mapper.Map<T>(model));
        }

        public virtual async Task<T> GetByIdAsync(TKey id) 
        {
            return await context.Set<T>().FindAsync(id);
        }

        public virtual async Task<TModel> GetByIdAsync<TModel>(TKey id)
        {
            return mapper.Map<TModel>(await context.Set<T>().FindAsync(id));
        }

        public virtual async Task<T> GetWithFilterAsync(Expression<Func<T, bool>> expression)
        {
            return await context.Set<T>().SingleOrDefaultAsync(expression);
        }

        public virtual async Task<TModel> GetWithFilterAsync<TModel>(Expression<Func<T, bool>> expression)
        {
            return mapper.Map<TModel>(await context.Set<T>().SingleOrDefaultAsync(expression));
        }

        public virtual async Task<IList<T>> ListAsync()
        {
            return await Query().ToListAsync();
        }

        public virtual async Task<IList<TModel>> ListAsync<TModel>()
        {
            return await mapper.ProjectTo<TModel>(context.Set<T>().AsQueryable()).ToListAsync();
        }

        public virtual async Task<IList<T>> ListAsync(Expression<Func<T, bool>> expression)
        {
            return await context.Set<T>().AsQueryable().Where(expression).ToListAsync();
        }

        public virtual async Task<IList<TModel>> ListAsync<TModel>(Expression<Func<T, bool>> expression)
        {
            return await mapper.ProjectTo<TModel>(context.Set<T>().Where(expression)).ToListAsync();
        }

        public virtual async Task<T> UpdateAsync(T model)
        {
            var entityResult = context.Set<T>().Update(model);
            await context.SaveChangesAsync();

            return entityResult.Entity;
        }

        public virtual async Task<bool> SoftDeleteAsync(TKey id)
        {
            var entity = await context.Set<T>().FindAsync(id);
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();

            return true;
        }

        public virtual async Task<int> GetTotalRecordsAsync()
        {
            return await context.Set<T>().CountAsync();
        }

        public virtual IQueryable<T> RawQuery(string query, params string[] parameters)
        {
            return context.Set<T>().FromSqlRaw(query, parameters);
        }

        public virtual IQueryable<T> Query()
        {
            return context.Set<T>().AsQueryable<T>();
        }

        public virtual async Task<int> DeleteManyAsync(IEnumerable<T> entities)
        {
            context.Set<T>().RemoveRange(entities);
            return await context.SaveChangesAsync();
        }
    }
}
