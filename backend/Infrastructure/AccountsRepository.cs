using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CorsoApi.Core;
using Konscious.Security.Cryptography;

namespace CorsoApi.Infrastructure.Data
{
    public interface IAccountsVault
    {
        void Add(Account @new);
        bool Exists(int id);
        List<Account> GetAll();
        Task LockAsync();
        Task UnLockAsync(string masterPassword);
        void Update(Account newValues);
    }

    public class AccountsVault : IAccountsVault
    {
        private List<Account> accounts;
        private readonly string filePath;
        private byte[] encryptionKey;

        public AccountsVault(string filePath)
        {
            this.filePath = filePath;
            accounts = [];
            encryptionKey = [];
        }

        public List<Account> GetAll()
        {
            return accounts;
        }

        public void Add(Account @new)
        {
            @new.Id = accounts.Count == 0 ? 1 : accounts.Max(_ => _.Id) + 1;
            accounts.Add(@new);
        }

        public bool Exists(int id) =>
            accounts.Any(_ => id == _.Id);

        public async Task LockAsync()
        {
            using var fileStream = File.Create(filePath);
            using var aes = Aes.Create();
            aes.Key = encryptionKey;
            aes.GenerateIV();

            // Write IV to beginning of file
            await fileStream.WriteAsync(aes.IV.AsMemory(0, aes.IV.Length));

            using var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var writer = new StreamWriter(cryptoStream, Encoding.UTF8);

            var json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
            await writer.WriteAsync(json);
        }

        public void Update(Account newValues)
        {
            var target = accounts.SingleOrDefault(_ => _.Id == newValues.Id) ?? throw new InvalidOperationException($"account with id : {newValues.Id} was not found");
            target.Name = newValues.Name;
            target.Username = newValues.Username;
            target.Password = newValues.Password;
        }

        public async Task UnLockAsync(string masterPassword)
        {
            encryptionKey = DeriveKeyFromPassword(masterPassword);
            accounts = await LoadAsync();
        }

        private async Task<List<Account>> LoadAsync()
        {
            if (!File.Exists(filePath))
            {
                return [];
            }

            using var fileStream = File.OpenRead(filePath);
            using var aes = Aes.Create();
            aes.Key = encryptionKey;

            var iv = new byte[16];
            await fileStream.ReadExactlyAsync(iv);
            aes.IV = iv;

            using var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<List<Account>>(json) ?? [];
        }

        private static byte[] DeriveKeyFromPassword(string masterPassword)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(masterPassword));
            //TODO: store salt safely
            argon2.Salt = Encoding.UTF8.GetBytes("Salt2025");
            argon2.DegreeOfParallelism = Environment.ProcessorCount;
            argon2.Iterations = 4;
            argon2.MemorySize = 65536; //64MB
            return argon2.GetBytes(32);
        }
    }
}