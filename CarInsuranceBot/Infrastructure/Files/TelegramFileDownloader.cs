using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Application.Options;
using Microsoft.Extensions.Options;

namespace CarInsuranceBot.Infrastructure.Files;

public sealed class TelegramFileDownloader : ITelegramFileDownloader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TelegramOptions _options;

    public TelegramFileDownloader(IHttpClientFactory httpClientFactory, IOptions<TelegramOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<string> DownloadAsync(string telegramFilePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(telegramFilePath);

        var client = _httpClientFactory.CreateClient(nameof(TelegramFileDownloader));
        var requestUri = new Uri(_options.ApiFileBaseUrl, $"{_options.BotToken}/{telegramFilePath}");
        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var extension = Path.GetExtension(telegramFilePath);
        var localPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{(string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension)}");

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destination = File.Create(localPath);
        await source.CopyToAsync(destination, cancellationToken);

        return localPath;
    }
}