using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Text;
using CorsoApi.Core;
using CorsoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace CorsoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[SessionAuthorize]
public class ChatController : ControllerBase
{
    [HttpPost]
    public async Task Post([FromBody] PostChatRequest request, 
        IChatClient chatClient,
        IAccountsVault vault,
        IChatHistoryStore chatHistoryStore,
        CancellationToken cancellationToken)
    {
        await vault.UnLockAsync();
        var accounts = vault.GetAll();
        await vault.LockAsync();

        var conversationId = HttpContext.Session.Id;
        var previousMessages = chatHistoryStore.Get(conversationId);

        var conversation = new List<ChatMessage>
        {
            new(ChatRole.System, ChatPrompt.CreateInstructions(accounts))
        };

        Response.Headers.CacheControl = "no-cache,no-transform";
        Response.ContentType = $"{MediaTypeNames.Text.Plain}; charset=utf-8";
        conversation.AddRange(previousMessages);
        conversation.Add(new ChatMessage(ChatRole.User, request.Prompt));

        var assistantResponse = new StringBuilder();

        await foreach (var update in chatClient.GetStreamingResponseAsync(
            conversation,
            cancellationToken: cancellationToken))
        {
            if (string.IsNullOrEmpty(update.Text))
            {
                continue;
            }

            assistantResponse.Append(update.Text);
            await Response.WriteAsync(update.Text, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        var updatedHistory = previousMessages
            .Append(new ChatMessage(ChatRole.User, request.Prompt))
            .Append(new ChatMessage(ChatRole.Assistant, assistantResponse.ToString()))
            .ToList();

        chatHistoryStore.Set(conversationId, updatedHistory);
    }

    public record PostChatRequest([Required] string Prompt);
}

