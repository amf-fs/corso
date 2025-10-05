using System.Text.Json;
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
        private readonly string filePath;

        public AccountsRepository(string filePath)
        {
            this.filePath = filePath;
            var text = File.ReadAllText(filePath);
            accounts = string.IsNullOrWhiteSpace(text) ? [] : JsonSerializer.Deserialize<List<Account>>(text) ?? [];
        }

        public Task<List<Account>> GetAllAsync()
        {
            return Task.FromResult(accounts);
        }

        public async Task AddAsync(Account @new)
        {
            @new.Id = accounts.Count == 0 ? 1 : accounts.Max(_ => _.Id) + 1;
            accounts.Add(@new);
            await CommitChanges();
        }

        public Task<bool> ExistsAsync(int id) =>
            Task.FromResult(accounts.Any(_ => id == _.Id));

        public async Task UpdateAsync(Account newValues)
        {
            var target = accounts.SingleOrDefault(_ => _.Id == newValues.Id);

            if (target is null)
            {
                throw new InvalidOperationException($"account with id : {newValues.Id} was not found");
            }

            target.Name = newValues.Name;
            target.Username = newValues.Username;
            target.Password = newValues.Password;

            await CommitChanges();
        }

        private async Task CommitChanges()
        {
            var json = JsonSerializer.Serialize(accounts);
            await File.WriteAllTextAsync(filePath, json);
        }
    }
}