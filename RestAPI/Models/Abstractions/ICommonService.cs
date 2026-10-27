namespace RestAPI.Models.Abstractions;

public interface ICommonService<T, K>
{
    Task<Result> Create(T model);
    Task<Result> Update(K id, T model);
    Task<Result> Delete(K id);
    Task<T?> Get(K id);
    Task<IEnumerable<T>> GetAll();
    Task<IEnumerable<T>> GetPage(int? page);
}
