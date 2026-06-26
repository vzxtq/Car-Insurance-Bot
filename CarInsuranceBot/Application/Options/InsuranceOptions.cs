using CarInsuranceBot.Domain;

namespace CarInsuranceBot.Application.Options;

public sealed class InsuranceOptions
{
    public const string SectionName = "Insurance";
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = string.Empty;
    public string PolicyTemplatePath { get; init; } = string.Empty;
    public Money Price => new(PriceAmount, PriceCurrency);
}