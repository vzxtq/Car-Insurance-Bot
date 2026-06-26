using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Bot.Ui;
using CarInsuranceBot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Bot.Handlers;

public sealed class FileMessageHandler
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

    private readonly ITelegramBotClient _botClient;
    private readonly IDocumentRecognitionService _documentRecognitionService;
    private readonly ITelegramFileDownloader _fileDownloader;
    private readonly IConversationStore _conversationStore;

    public FileMessageHandler(
        ITelegramBotClient botClient,
        IDocumentRecognitionService documentRecognitionService,
        ITelegramFileDownloader fileDownloader,
        IConversationStore conversationStore)
    {
        _botClient = botClient;
        _documentRecognitionService = documentRecognitionService;
        _fileDownloader = fileDownloader;
        _conversationStore = conversationStore;
    }

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var session = _conversationStore.Get(chatId);
        if (session is null)
        {
            await _botClient.SendTextMessageAsync(chatId, BotMessages.StartRequired, cancellationToken: cancellationToken);
            return;
        }

        var telegramFilePath = await ResolveTelegramFilePathAsync(message, cancellationToken);
        if (telegramFilePath is null)
        {
            await _botClient.SendTextMessageAsync(chatId, BotMessages.ValidImageRequired, cancellationToken: cancellationToken);
            return;
        }

        string? localPath = null;
        try
        {
            localPath = await _fileDownloader.DownloadAsync(telegramFilePath, cancellationToken);
            await ProcessByStateAsync(session, localPath, cancellationToken);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(localPath) && System.IO.File.Exists(localPath))
            {
                System.IO.File.Delete(localPath);
            }
        }
    }

    private async Task<string?> ResolveTelegramFilePathAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.Document is not null && IsImageDocument(message.Document))
        {
            var file = await _botClient.GetFileAsync(message.Document.FileId, cancellationToken);
            return file.FilePath;
        }

        if (message.Photo is { Length: > 0 })
        {
            var photo = message.Photo.OrderByDescending(item => item.FileSize).First();
            var file = await _botClient.GetFileAsync(photo.FileId, cancellationToken);

            return file.FilePath;
        }

        return null;
    }

    private static bool IsImageDocument(Document document)
    {
        var extension = Path.GetExtension(document.FileName ?? string.Empty);

        return document.MimeType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true || SupportedExtensions.Contains(extension);
    }

    private async Task ProcessByStateAsync(ConversationSession session, string localPath, CancellationToken cancellationToken)
    {
        switch (session.State)
        {
            case ConversationState.AwaitingPassport:
                await ProcessPassportAsync(session, localPath, cancellationToken);
                break;

            case ConversationState.AwaitingVin:
                await ProcessVehicleAsync(session, localPath, cancellationToken);
                break;

            default:
                await _botClient.SendTextMessageAsync(session.ChatId, BotMessages.UnexpectedDocument, cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task ProcessPassportAsync(ConversationSession session, string localPath, CancellationToken cancellationToken)
    {
        await _botClient.SendTextMessageAsync(session.ChatId, BotMessages.ProcessingPassport, cancellationToken: cancellationToken);

        var passport = await _documentRecognitionService.ReadPassportAsync(localPath, cancellationToken);
        _conversationStore.Save(session.WithPassport(passport));

        await _botClient.SendTextMessageAsync(
            session.ChatId,
            BotMessages.PassportConfirmation(
                passport.FullName,
                passport.DocumentNumber),
            replyMarkup: BotKeyboards.PassportConfirmation(),
            cancellationToken: cancellationToken);
    }

    private async Task ProcessVehicleAsync(ConversationSession session, string localPath, CancellationToken cancellationToken)
    {
        await _botClient.SendTextMessageAsync(session.ChatId, BotMessages.ProcessingVehicle, cancellationToken: cancellationToken);
        var vehicle = await _documentRecognitionService.ReadVehicleAsync(localPath, cancellationToken);

        if (!vehicle.IsComplete)
        {
            await _botClient.SendTextMessageAsync(session.ChatId, BotMessages.VinNotFound, cancellationToken: cancellationToken);
            return;
        }

        _conversationStore.Save(session.WithVehicle(vehicle));
        await _botClient.SendTextMessageAsync(session.ChatId, BotMessages.VehicleConfirmation(vehicle.Vin), replyMarkup: BotKeyboards.VehicleConfirmation(), cancellationToken: cancellationToken);
    }
}