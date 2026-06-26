namespace CarInsuranceBot.Domain;

public sealed record ConversationSession(long ChatId, ConversationState State, PassportData? Passport, VehicleData? Vehicle)
{
    public static ConversationSession Start(long chatId) => new(chatId, ConversationState.AwaitingPassport, null, null);
    public ConversationSession Cancel() => this with { State = ConversationState.Canceled };
    public ConversationSession WithState(ConversationState state) => this with { State = state };
    public ConversationSession WithPassport(PassportData passport) => this with { Passport = passport, State = ConversationState.AwaitingPassport };
    public ConversationSession ConfirmPassport() => this with { State = ConversationState.AwaitingVin };
    public ConversationSession RejectPassport() => this with { Passport = null, State = ConversationState.AwaitingPassport };
    public ConversationSession WithVehicle(VehicleData vehicle) => this with { Vehicle = vehicle, State = ConversationState.AwaitingVin };
    public ConversationSession ConfirmVehicle() => this with { State = ConversationState.PriceOffered };
    public ConversationSession RejectVehicle() => this with { Vehicle = null, State = ConversationState.AwaitingVin };
}