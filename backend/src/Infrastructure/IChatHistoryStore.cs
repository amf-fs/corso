using Microsoft.Extensions.AI;

namespace CorsoApi.Infrastructure;

public interface IChatHistoryStore
{
    IReadOnlyList<ChatMessage> Get(string conversationId);

    void Set(string conversationId, IReadOnlyList<ChatMessage> messages);
}
