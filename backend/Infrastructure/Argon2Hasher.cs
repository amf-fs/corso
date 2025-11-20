using System.Text;
using Konscious.Security.Cryptography;

namespace CorsoApi.Infrastructure
{
    public class Argon2Hasher : IHasher
    {
        public string Create(string from, string salt)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(from));
            argon2.Salt = Encoding.UTF8.GetBytes(salt);
            argon2.DegreeOfParallelism = Environment.ProcessorCount;
            argon2.Iterations = 4;
            argon2.MemorySize = 65536; //64MB
            return Convert.ToBase64String(argon2.GetBytes(32));
        }
    }
}