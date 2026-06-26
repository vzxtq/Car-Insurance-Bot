namespace CarInsuranceBot.Infrastructure.Ai.Models;

public sealed class GeminiResponse
{
    public List<GeminiCandidate>? Candidates { get; init; }
}

public sealed class GeminiCandidate
{
    public GeminiContent? Content { get; init; }
}

public sealed class GeminiContent
{
    public List<GeminiPart>? Parts { get; init; }
    public string? Role { get; init; }
}

public sealed class GeminiPart
{
    public string? Text { get; init; }
}