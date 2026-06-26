using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Application.Options;
using CarInsuranceBot.Common;
using CarInsuranceBot.Infrastructure.Ai.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace CarInsuranceBot.Infrastructure.Ai;

public sealed class GeminiAssistant : IAiAssistant
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiAssistant> _logger;

    public GeminiAssistant(IHttpClientFactory httpClientFactory, IOptions<GeminiOptions> options, ILogger<GeminiAssistant> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> ReplyAsync(string prompt, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = _options.SystemInstruction },
                        new { text = prompt }
                    }
                }
            }
        };

        var client = _httpClientFactory.CreateClient(nameof(GeminiAssistant));
        var requestUri = new Uri(_options.EndpointBaseUrl, $"{_options.Model}:generateContent?key={Uri.EscapeDataString(_options.ApiKey)}");
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(requestUri, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Gemini request failed with status {StatusCode}", response.StatusCode);
            return "I could not process that message right now. Please try again in a moment.";
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson, JsonOptions.Default);
            var text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
            return string.IsNullOrWhiteSpace(text) ? "I did not receive a useful AI response. Please try again." : text.Trim();
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Failed to parse Gemini response");
            return "I could not understand the AI response. Please try again.";
        }
    }
}