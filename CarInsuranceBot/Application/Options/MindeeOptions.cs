namespace CarInsuranceBot.Application.Options;

public sealed class MindeeOptions
{
    public const string SectionName = "Mindee";
    public string ApiKey { get; init; } = string.Empty;
    public string VinEndpointName { get; init; } = string.Empty;
    public string VinAccountName { get; init; } = string.Empty;
    public string VinEndpointVersion { get; init; } = string.Empty;
}