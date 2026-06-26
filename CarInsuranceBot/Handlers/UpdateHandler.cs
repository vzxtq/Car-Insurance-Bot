using CarInsuranceBot.Bot.Ui;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CarInsuranceBot.Bot.Handlers;

public sealed class UpdateHandler
{
    private readonly TextMessageHandler _textMessageHandler;
    private readonly FileMessageHandler _fileMessageHandler;
    private readonly CallbackHandler _callbackHandler;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(
        TextMessageHandler textMessageHandler,
        FileMessageHandler fileMessageHandler,
        CallbackHandler callbackHandler,
        ILogger<UpdateHandler> logger)
    {
        _textMessageHandler = textMessageHandler;
        _fileMessageHandler = fileMessageHandler;
        _callbackHandler = callbackHandler;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update)
        {
            case { Type: UpdateType.Message, Message: { Type: MessageType.Text } message }:
                await _textMessageHandler.HandleAsync(message, cancellationToken);
                break;

            case { Type: UpdateType.Message, Message: { Type: MessageType.Photo or MessageType.Document } message }:
                await _fileMessageHandler.HandleAsync(message, cancellationToken);
                break;

            case { Type: UpdateType.Message, Message: { } message }:
                await botClient.SendTextMessageAsync(message.Chat.Id, BotMessages.UnsupportedMessage(message), cancellationToken: cancellationToken);
                break;

            case { Type: UpdateType.CallbackQuery, CallbackQuery: { } callbackQuery }:
                await _callbackHandler.HandleAsync(callbackQuery, cancellationToken);
                break;
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram polling error");
        return Task.CompletedTask;
    }
}