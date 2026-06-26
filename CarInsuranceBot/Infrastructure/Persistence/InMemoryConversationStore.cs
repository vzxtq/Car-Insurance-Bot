using System.Collections.Concurrent;
using CarInsuranceBot.Application.Interfaces;
using CarInsuranceBot.Domain;

namespace CarInsuranceBot.Infrastructure.Persistence;

public sealed class InMemoryConversationStore : IConversationStore
{
    private readonly ConcurrentDictionary<long, ConversationSession> _sessions = new();

    public ConversationSession? Get(long chatId) => _sessions.TryGetValue(chatId, out var session) ? session : null;

    public ConversationSession GetOrCreate(long chatId) => _sessions.GetOrAdd(chatId, ConversationSession.Start);

    public void Save(ConversationSession session) => _sessions[session.ChatId] = session;
}