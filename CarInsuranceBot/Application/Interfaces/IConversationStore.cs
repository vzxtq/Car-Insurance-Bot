using CarInsuranceBot.Domain;

namespace CarInsuranceBot.Application.Interfaces;

public interface IConversationStore
{
    ConversationSession? Get(long chatId);
    ConversationSession GetOrCreate(long chatId);
    void Save(ConversationSession session);
}