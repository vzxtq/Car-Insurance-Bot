using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Application.Services;
using CarInsuranceBot.Bot.Ui;
using CarInsuranceBot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.Bot.Handlers;

public sealed class TextMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IAiAssistant _aiAssistant;
    private readonly IConversationStore _conversationStore;

    public TextMessageHandler(
        ITelegramBotClient botClient,
        IAiAssistant aiAssistant,
        IConversationStore conversationStore)
    {
        _botClient = botClient;
        _aiAssistant = aiAssistant;
        _conversationStore = conversationStore;
    }

    public async Task HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var userInput = message.Text?.Trim();

        if (string.Equals(userInput, "/start", StringComparison.OrdinalIgnoreCase))
        {
            await StartAsync(chatId, cancellationToken);
            return;
        }

        if (string.Equals(userInput, "/cancel", StringComparison.OrdinalIgnoreCase))
        {
            _conversationStore.Save(new ConversationSession(chatId, ConversationState.Canceled, null, null));
            await _botClient.SendTextMessageAsync(chatId, BotMessages.Canceled, cancellationToken: cancellationToken);
            return;
        }

        var session = _conversationStore.Get(chatId);
        if (session is null || session.State is ConversationState.Idle or ConversationState.Canceled)
        {
            await _botClient.SendTextMessageAsync(chatId, BotMessages.StartRequired, cancellationToken: cancellationToken);
            return;
        }

        var prompt = UserPromptFactory.ForTextMessage(session.State, userInput ?? string.Empty);
        var reply = await _aiAssistant.ReplyAsync(prompt, cancellationToken);
        await _botClient.SendTextMessageAsync(chatId, reply, cancellationToken: cancellationToken);
    }

    private async Task StartAsync(long chatId, CancellationToken cancellationToken)
    {
        var existingSession = _conversationStore.Get(chatId);

        if (existingSession is not null
            && existingSession.State != ConversationState.Idle
            && existingSession.State != ConversationState.Canceled 
            && existingSession.State != ConversationState.Completed)
        {
            await _botClient.SendTextMessageAsync(chatId, BotMessages.ProcessAlreadyStarted, cancellationToken: cancellationToken);
            return;
        }

        _conversationStore.Save(ConversationSession.Start(chatId));

        await _botClient.SendTextMessageAsync(chatId, $"{BotMessages.Welcome}\n\n{BotMessages.DocumentsChecklist}", cancellationToken: cancellationToken);

        await _botClient.SendTextMessageAsync(chatId, BotMessages.AskPassport, cancellationToken: cancellationToken);
    }
}