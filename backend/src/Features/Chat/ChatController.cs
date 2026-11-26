using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using CorsoApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CorsoApi.Features.Chat;

[Route("api/[controller]")]
[ApiController]
[SessionAuthorize]
public class ChatController : ControllerBase
{
    [HttpPost]
    public async Task Post(
        [FromBody] PostChatRequest request,
        ChatService chatService,
        ILogger<ChatController> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var responseStream = await chatService.StreamAsync(
                HttpContext.Session.Id,
                request.Prompt,
                cancellationToken);

            Response.Headers.CacheControl = "no-cache,no-transform";
            Response.ContentType = $"{MediaTypeNames.Text.Plain}; charset=utf-8";

            try
            {
                await foreach (var text in responseStream.WithCancellation(cancellationToken))
                {
                    await Response.WriteAsync(text, cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                await WriteStreamingFailureAsync(exception, logger, cancellationToken);
                return;
            }
        }
        catch (Exception exception)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation(exception, "User has canceled the request.");
                return;
            }

            if (Response.HasStarted)
            {
                await WriteStreamingFailureAsync(exception, logger, cancellationToken);
                return;
            }

            logger.LogError(exception, "Unexpected error while processing the chat request.");
            await WriteProblemDetailsAsync(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.",
                cancellationToken);
        }
    }

    public record PostChatRequest([Required] string Prompt);

    private async Task WriteStreamingFailureAsync(
        Exception exception,
        ILogger<ChatController> logger,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error while processing the chat response.");

        try
        {
            await Response.WriteAsync(
                "\n...Sorry I could not complete the request, try again.",
                cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
        catch (Exception writeException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(writeException, "Unable to write the chat stream failure message.");
        }
    }

    private async Task WriteProblemDetailsAsync(
        int statusCode,
        string title,
        string detail,
        CancellationToken cancellationToken)
    {
        Response.Clear();
        Response.StatusCode = statusCode;
        Response.ContentType = MediaTypeNames.Application.ProblemJson;

        await Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = Request.Path
            },
            cancellationToken: cancellationToken);
    }
}

