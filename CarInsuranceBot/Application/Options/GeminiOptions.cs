namespace CarInsuranceBot.Application.Options;

public sealed class GeminiOptions
{
    public const string SectionName = "GeminiAi";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public Uri EndpointBaseUrl { get; init; } = new("https://generativelanguage.googleapis.com/v1beta/models/");
    public string SystemInstruction { get; init; } = string.Empty;
}