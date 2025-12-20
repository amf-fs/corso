using System.Text;

namespace CorsoApiTests;

public static class Extensions
{
    public static MemoryStream ToMemoryStream(this string content) 
        => new(Encoding.UTF8.GetBytes(content));
}
