namespace CarInsuranceBot.Application.Options;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";
    public string BotToken { get; init; } = string.Empty;
    public Uri ApiFileBaseUrl { get; init; } = new("https://api.telegram.org/file/bot");
}