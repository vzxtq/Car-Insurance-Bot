using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Bot.Ui;

public static class BotKeyboards
{
    public static InlineKeyboardMarkup PassportConfirmation() => Confirmation(CallbackActions.ConfirmPassport, CallbackActions.RejectPassport);
    public static InlineKeyboardMarkup VehicleConfirmation() => Confirmation(CallbackActions.ConfirmVin, CallbackActions.RejectVin);
    public static InlineKeyboardMarkup PriceConfirmation() => Confirmation(CallbackActions.AgreePrice, CallbackActions.DisagreePrice);
    public static InlineKeyboardMarkup FinalPriceConfirmation() => Confirmation(CallbackActions.FinalAgree, CallbackActions.FinalDisagree);

    private static InlineKeyboardMarkup Confirmation(string yesAction, string noAction) => new(new[]
    {
        new[] { InlineKeyboardButton.WithCallbackData("Yes", yesAction), InlineKeyboardButton.WithCallbackData("No", noAction) }
    });
}