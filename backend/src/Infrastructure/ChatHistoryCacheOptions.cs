namespace CorsoApi.Infrastructure;

public sealed class ChatHistoryCacheOptions
{
    public const string SectionName = "ChatHistory";

    public int MaxMessages { get; set; } = 20;

    public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(30);
}
