using CarInsuranceBot.Domain;

namespace CarInsuranceBot.Application.Services;

public static class UserPromptFactory
{
    public static string ForTextMessage(ConversationState state, string userMessage)
    {
        var text = string.IsNullOrWhiteSpace(userMessage) ? "<empty message>" : userMessage.Trim();
        return state switch
        {
            ConversationState.AwaitingPassport => $"The user is in the passport upload stage and wrote: '{text}'. Reply briefly, then remind them to upload a clear passport photo.",
            ConversationState.AwaitingVin => $"The user is in the VIN upload stage and wrote: '{text}'. Reply briefly, explain why VIN is required, then remind them to upload the vehicle title photo.",
            ConversationState.Completed => $"The user has completed the insurance flow and wrote: '{text}'. Reply briefly and help with policy questions or next steps.",
            _ => $"The user is in state '{state}' and wrote: '{text}'. Reply briefly, professionally, and helpfully."
        };
    }
}