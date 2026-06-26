using CarInsuranceBot.Bot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace CarInsuranceBot.Services;

public sealed class BotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _handler;
    private readonly ILogger<BotBackgroundService> _logger;

    public BotBackgroundService(
        ITelegramBotClient botClient,
        UpdateHandler handler,
        ILogger<BotBackgroundService> logger)
    {
        _botClient = botClient;
        _handler = handler;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = [] };

        _botClient.StartReceiving(
            updateHandler: _handler.HandleUpdateAsync,
            pollingErrorHandler: _handler.HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cancellationToken);

        var botInfo = await _botClient.GetMeAsync(cancellationToken);

        _logger.LogInformation("Bot @{Username} is running", botInfo.Username);
    }
}