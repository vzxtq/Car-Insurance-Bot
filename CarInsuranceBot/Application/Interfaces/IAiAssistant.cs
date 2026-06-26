namespace CarInsuranceBot.Application.Interfaces;

public interface IAiAssistant
{
    Task<string> ReplyAsync(string prompt, CancellationToken cancellationToken);
}