using System.Runtime.CompilerServices;
using System.Text;
using CorsoApi.Core;
using CorsoApi.Infrastructure;
using Microsoft.Extensions.AI;

namespace CorsoApi.Features.Chat;

public sealed class ChatService(
    IChatClient chatClient,
    IAccountsVault vault,
    IChatHistoryStore chatHistoryStore)
{
    public async Task<IAsyncEnumerable<string>> StreamAsync(
        string conversationId,
        string prompt,
        CancellationToken cancellationToken)
    {
        await vault.UnLockAsync();
        List<Account> accounts;
        try
        {
            accounts = vault.GetAll();
        }
        finally
        {
            await vault.LockAsync();
        }

        var previousMessages = chatHistoryStore.Get(conversationId);
        var conversation = new List<ChatMessage>
        {
            new(ChatRole.System, ChatPrompt.CreateInstructions(accounts))
        };

        conversation.AddRange(previousMessages);
        conversation.Add(new ChatMessage(ChatRole.User, prompt));

        return StreamResponseAsync(
            conversationId,
            prompt,
            previousMessages,
            conversation,
            cancellationToken);
    }

    private async IAsyncEnumerable<string> StreamResponseAsync(
        string conversationId,
        string prompt,
        IReadOnlyList<ChatMessage> previousMessages,
        List<ChatMessage> conversation,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var assistantResponse = new StringBuilder();
        await foreach (var update in chatClient
            .GetStreamingResponseAsync(conversation, cancellationToken: cancellationToken)
            .WithCancellation(cancellationToken))
        {
            var text = update.Text;
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            assistantResponse.Append(text);
            yield return text;
        }

        var updatedHistory = previousMessages
            .Append(new ChatMessage(ChatRole.User, prompt))
            .Append(new ChatMessage(ChatRole.Assistant, assistantResponse.ToString()))
            .ToList();

        chatHistoryStore.Set(conversationId, updatedHistory);
    }
}
