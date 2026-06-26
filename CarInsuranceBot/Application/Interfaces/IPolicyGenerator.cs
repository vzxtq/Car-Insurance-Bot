using CarInsuranceBot.Domain;

namespace CarInsuranceBot.Application.Interfaces;

public interface IPolicyGenerator
{
    Task<string> GenerateAsync(PassportData passport, VehicleData vehicle, Money price, CancellationToken cancellationToken);
}