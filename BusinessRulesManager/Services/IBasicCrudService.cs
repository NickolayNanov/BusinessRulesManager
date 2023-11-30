using BusinessRulesManager.Data.Contracts;
using System.Linq.Expressions;

namespace BusinessRulesManager.Services
{
    public interface IBasicCrudService<T, TKey> where T : IEntity<TKey>
    {
        Task<T> GetByIdAsync(TKey id);

        Task<TModel> GetByIdAsync<TModel>(TKey id);

        Task<T> GetWithFilterAsync(Expression<Func<T, bool>> expression);

        Task<IList<T>> ListAsync();

        Task<IList<T>> ListAsync(Expression<Func<T, bool>> expression);

        Task<IList<TModel>> ListAsync<TModel>(Expression<Func<T, bool>> expression);

        Task<IList<TModel>> ListAsync<TModel>();

        Task<T> CreateAsync(T model);

        Task<T> CreateAsync<TModel>(TModel model);

        Task<int> BulkInsertAsync(IEnumerable<T> entities);

        Task<T> UpdateAsync(T model);

        Task<bool> SoftDeleteAsync(TKey id);

        Task<int> GetTotalRecordsAsync();

        IQueryable<T> RawQuery(string query, params string[] parameters);

        IQueryable<T> Query();

        Task<int> DeleteManyAsync(IEnumerable<T> entities);
    }
}
