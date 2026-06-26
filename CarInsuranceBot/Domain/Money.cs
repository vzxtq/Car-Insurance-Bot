namespace CarInsuranceBot.Domain;

public sealed record Money(decimal Amount, string Currency)
{
    public override string ToString() => $"{Amount:0.##} {Currency}";
}