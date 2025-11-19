namespace CorsoApi.Infrastructure;

public interface IHasher
{
    string Create(string from, string salt);
}
