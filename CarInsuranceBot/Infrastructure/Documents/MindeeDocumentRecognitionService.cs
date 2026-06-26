using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Application.Options;
using CarInsuranceBot.Domain;
using Microsoft.Extensions.Options;
using Mindee.Input;
using Mindee.V1;
using Mindee.V1.Http;
using Mindee.V1.Product.Generated;
using Mindee.V1.Product.InternationalId;

namespace CarInsuranceBot.Infrastructure.Documents;

public sealed class MindeeDocumentRecognitionService : IDocumentRecognitionService
{
    private readonly Client _mindeeClient;
    private readonly MindeeOptions _options;

    public MindeeDocumentRecognitionService(IOptions<MindeeOptions> options)
    {
        _options = options.Value;
        _mindeeClient = new Client(_options.ApiKey);
    }

    public async Task<PassportData> ReadPassportAsync(string filePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        cancellationToken.ThrowIfCancellationRequested();

        var response = await _mindeeClient.EnqueueAndParseAsync<InternationalIdV2>(new LocalInputSource(filePath));
        var prediction = (response?.Document?.Inference?.Prediction) ?? throw new InvalidOperationException("Passport recognition returned an empty prediction.");

        var givenName = prediction.GivenNames?.FirstOrDefault()?.Value;
        var surname = prediction.Surnames?.FirstOrDefault()?.Value;
        var fullName = string.Join(' ', new[] { givenName, surname }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return new PassportData(NormalizeOrUnknown(fullName), NormalizeOrUnknown(prediction.DocumentNumber?.Value));
    }

    public async Task<VehicleData> ReadVehicleAsync(string filePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        cancellationToken.ThrowIfCancellationRequested();

        var endpoint = new CustomEndpoint(_options.VinEndpointName, _options.VinAccountName, _options.VinEndpointVersion);
        var response = await _mindeeClient.EnqueueAndParseAsync<GeneratedV1>(new LocalInputSource(filePath), endpoint);

        if (response.Document.Inference.Prediction.Fields.TryGetValue("vin", out var vinField))
        {
            var vin = vinField.ToString()?.Replace(":value:", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
            return new VehicleData(NormalizeVin(vin));
        }

        return new VehicleData(string.Empty);
    }

    private static string NormalizeOrUnknown(string? value) => string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();

    private static string NormalizeVin(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
}