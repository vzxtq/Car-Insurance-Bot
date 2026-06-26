namespace CarInsuranceBot.Domain;

public sealed record VehicleData(string Vin)
{
    public bool IsComplete => !string.IsNullOrWhiteSpace(Vin);
}