using CorsoApi.Core;

namespace CorsoApi.Infrastructure.Data
{
    public interface IAccountsRepository
    {
        Task AddAsync(Account @new);
        Task<List<Account>> GetAllAsync();
    }

    //TODO: refactor to persist in file
    public class AccountsRepository : IAccountsRepository
    {
        private readonly List<Account> accounts;

        public AccountsRepository()
        {
            accounts =
            [
                new() { Id = 1, Name = "Gmail", Username = "user@gmail.com", Password = "password123" },
                new() { Id = 2, Name = "Facebook", Username = "myuser", Password = "secret456" }
            ];
        }

        public Task<List<Account>> GetAllAsync()
        {
            return Task.FromResult(accounts);
        }

        public Task AddAsync(Account @new)
        {
            @new.Id = accounts.Max(_ => _.Id) + 1;
            accounts.Add(@new);
            return Task.CompletedTask;
        }
    }
}