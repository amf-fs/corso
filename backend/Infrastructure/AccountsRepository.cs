using CorsoApi.Core;

namespace CorsoApi.Infrastructure.Data
{
    public interface IAccountsRepository
    {
        Task AddAsync(Account @new);
        Task<bool> ExistsAsync(int id);
        Task<List<Account>> GetAllAsync();
        Task UpdateAsync(Account newValues);
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

        public Task<bool> ExistsAsync(int id) =>
            Task.FromResult(accounts.Any(_ => id == _.Id));

        public Task UpdateAsync(Account newValues)
        {
            var target = accounts.SingleOrDefault(_ => _.Id == newValues.Id);

            if (target is null)
            {
                throw new InvalidOperationException($"account with id : {newValues.Id} was not found");
            }

            target.Name = newValues.Name;
            target.Username = newValues.Username;
            target.Password = newValues.Password;

            return Task.CompletedTask;
        }
    }
}