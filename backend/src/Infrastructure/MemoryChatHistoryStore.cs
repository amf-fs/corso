using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace CorsoApi.Infrastructure;

public sealed class MemoryChatHistoryStore(
    IMemoryCache cache,
    IOptions<ChatHistoryCacheOptions> options) : IChatHistoryStore
{
    private readonly ChatHistoryCacheOptions _options = options.Value;

    public IReadOnlyList<ChatMessage> Get(string conversationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);

        if (!cache.TryGetValue(GetKey(conversationId), out ChatMessage[]? messages) || messages is null)
        {
            return [];
        }

        return Array.AsReadOnly(messages);
    }

    public void Set(string conversationId, IReadOnlyList<ChatMessage> messages)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conversationId);
        ArgumentNullException.ThrowIfNull(messages);

        cache.Set(
            GetKey(conversationId),
            messages.TakeLast(_options.MaxMessages).ToArray(),
            new MemoryCacheEntryOptions
            {
                SlidingExpiration = _options.SlidingExpiration
            });
    }

    private static string GetKey(string conversationId) =>
        $"chat-history:{conversationId}";
}
