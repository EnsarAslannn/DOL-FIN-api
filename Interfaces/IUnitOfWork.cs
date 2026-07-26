namespace api.Interfaces
{
    public interface IUnitOfWork
    {
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    }
}
