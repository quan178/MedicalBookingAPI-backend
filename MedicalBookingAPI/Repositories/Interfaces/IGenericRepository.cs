namespace MedicalBookingAPI.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync<TKey>(TKey id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task DeleteAsync<TKey>(TKey id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsAsync<TKey>(TKey id);
}
