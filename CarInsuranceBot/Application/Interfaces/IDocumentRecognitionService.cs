using CarInsuranceBot.Domain;

namespace CarInsuranceBot.Application.Interfaces;

public interface IDocumentRecognitionService
{
    Task<PassportData> ReadPassportAsync(string filePath, CancellationToken cancellationToken);
    Task<VehicleData> ReadVehicleAsync(string filePath, CancellationToken cancellationToken);
}