namespace CarInsuranceBot.Domain;

public enum ConversationState
{
    Idle = 0,
    AwaitingPassport = 1,
    AwaitingVin = 2,
    PriceOffered = 3,
    Completed = 4,
    Canceled = 5
}