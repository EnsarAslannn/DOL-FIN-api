using api.Models;

namespace api.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
    }
}
