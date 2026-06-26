namespace CarInsuranceBot.Application.Interfaces;

public interface ITelegramFileDownloader
{
    Task<string> DownloadAsync(string telegramFilePath, CancellationToken cancellationToken);
}