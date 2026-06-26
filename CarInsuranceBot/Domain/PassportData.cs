namespace CarInsuranceBot.Domain;

public sealed record PassportData(string FullName, string DocumentNumber)
{
    public bool IsComplete => !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(DocumentNumber);
}