using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Application.Options;
using CarInsuranceBot.Domain;
using Microsoft.Extensions.Options;

namespace CarInsuranceBot.Infrastructure.Policies;

public sealed class TemplatePolicyGenerator : IPolicyGenerator
{
    private readonly IHostEnvironment _environment;
    private readonly InsuranceOptions _options;

    public TemplatePolicyGenerator(IHostEnvironment environment, IOptions<InsuranceOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public async Task<string> GenerateAsync(PassportData passport, VehicleData vehicle, Money price, CancellationToken cancellationToken)
    {
        var templatePath = Path.IsPathRooted(_options.PolicyTemplatePath)
            ? _options.PolicyTemplatePath
            : Path.Combine(_environment.ContentRootPath, _options.PolicyTemplatePath);

        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["POLICY_NUMBER"] = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
            ["NAME"] = passport.FullName,
            ["PASSPORT"] = passport.DocumentNumber,
            ["VIN"] = vehicle.Vin,
            ["PRICE"] = price.ToString()
        };

        return values.Aggregate(template, (current, pair) => current.Replace($"{{{pair.Key}}}", pair.Value, StringComparison.OrdinalIgnoreCase));
    }
}