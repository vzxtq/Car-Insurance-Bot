using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Application.Options;
using CarInsuranceBot.Bot.Ui;
using CarInsuranceBot.Domain;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CarInsuranceBot.Bot.Handlers;

public sealed class CallbackHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConversationStore _conversationStore;
    private readonly IPolicyGenerator _policyGenerator;
    private readonly IMarkdownEscaper _markdownEscaper;
    private readonly InsuranceOptions _insuranceOptions;

    public CallbackHandler(ITelegramBotClient botClient, IConversationStore conversationStore, IPolicyGenerator policyGenerator, IMarkdownEscaper markdownEscaper, IOptions<InsuranceOptions> insuranceOptions)
    {
        _botClient = botClient;
        _conversationStore = conversationStore;
        _policyGenerator = policyGenerator;
        _markdownEscaper = markdownEscaper;
        _insuranceOptions = insuranceOptions.Value;
    }

    public async Task HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message is null)
        {
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
            return;
        }

        var chatId = callbackQuery.Message.Chat.Id;
        var session = _conversationStore.GetOrCreate(chatId);

        switch (callbackQuery.Data)
        {
            case CallbackActions.ConfirmPassport:
                _conversationStore.Save(session.ConfirmPassport());
                await ClearButtonsAsync(callbackQuery, cancellationToken);

                await _botClient.SendTextMessageAsync(chatId, BotMessages.PassportConfirmed, cancellationToken: cancellationToken);
                await _botClient.SendTextMessageAsync(chatId, BotMessages.AskVin, cancellationToken: cancellationToken);

                break;

            case CallbackActions.RejectPassport:
                _conversationStore.Save(session.RejectPassport());

                await ClearButtonsAsync(callbackQuery, cancellationToken);
                await _botClient.SendTextMessageAsync(chatId, BotMessages.PassportRejected, cancellationToken: cancellationToken);

                break;

            case CallbackActions.ConfirmVin:
                _conversationStore.Save(session.ConfirmVehicle());
                await ClearButtonsAsync(callbackQuery, cancellationToken);

                await _botClient.SendTextMessageAsync(chatId, BotMessages.VehicleConfirmed, cancellationToken: cancellationToken);
                await _botClient.SendTextMessageAsync(chatId, BotMessages.PriceOffer(_insuranceOptions.Price.ToString()), replyMarkup: BotKeyboards.PriceConfirmation(), cancellationToken: cancellationToken);

                break;

            case CallbackActions.RejectVin:
                _conversationStore.Save(session.RejectVehicle());

                await ClearButtonsAsync(callbackQuery, cancellationToken);
                await _botClient.SendTextMessageAsync(chatId, BotMessages.VehicleRejected, cancellationToken: cancellationToken);

                break;

            case CallbackActions.AgreePrice:
            case CallbackActions.FinalAgree:
                await GenerateAndSendPolicyAsync(callbackQuery, session, cancellationToken);
                break;

            case CallbackActions.DisagreePrice:
                await ClearButtonsAsync(callbackQuery, cancellationToken);

                await _botClient.SendTextMessageAsync(chatId, BotMessages.FinalPriceOffer(_insuranceOptions.Price.ToString()), replyMarkup: BotKeyboards.FinalPriceConfirmation(), cancellationToken: cancellationToken);

                break;

            case CallbackActions.FinalDisagree:
                _conversationStore.Save(session.Cancel());
                await ClearButtonsAsync(callbackQuery, cancellationToken);

                await _botClient.SendTextMessageAsync(chatId, BotMessages.FinalDisagree, cancellationToken: cancellationToken);

                break;

            default:
                await _botClient.SendTextMessageAsync(chatId, BotMessages.UnknownAction, cancellationToken: cancellationToken);
                break;
        }

        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
    }

    private async Task GenerateAndSendPolicyAsync(CallbackQuery callbackQuery, ConversationSession session, CancellationToken cancellationToken)
    {
        await ClearButtonsAsync(callbackQuery, cancellationToken);
        if (session.Passport is null || !session.Passport.IsComplete)
        {
            await _botClient.SendTextMessageAsync(session.ChatId, BotMessages.MissingPassport, cancellationToken: cancellationToken);
            return;
        }

        if (session.Vehicle is null || !session.Vehicle.IsComplete)
        {
            await _botClient.SendTextMessageAsync(session.ChatId, BotMessages.MissingVehicle, cancellationToken: cancellationToken);
            return;
        }

        await _botClient.SendTextMessageAsync(session.ChatId, BotMessages.GeneratingPolicy, cancellationToken: cancellationToken);

        var policy = await _policyGenerator.GenerateAsync(session.Passport, session.Vehicle, _insuranceOptions.Price, cancellationToken);

        var escapedPolicy = _markdownEscaper.EscapeMarkdownV2(policy);

        await _botClient.SendTextMessageAsync(session.ChatId, escapedPolicy, parseMode: ParseMode.MarkdownV2, cancellationToken: cancellationToken);

        _conversationStore.Save(session.WithState(ConversationState.Completed));
    }

    private async Task ClearButtonsAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message is not null)
        {
            await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, replyMarkup: null, cancellationToken: cancellationToken);
        }
    }
}