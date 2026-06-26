using CarInsuranceBot.Application.Interfaces;

namespace CarInsuranceBot.Infrastructure.Telegram;

public sealed class MarkdownEscaper : IMarkdownEscaper
{
    private static readonly string[] SpecialCharacters = ["_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!"];

    public string EscapeMarkdownV2(string text)
    {
        var escaped = text;
        foreach (var specialCharacter in SpecialCharacters)
        {
            escaped = escaped.Replace(specialCharacter, "\\" + specialCharacter, StringComparison.Ordinal);
        }
        return escaped;
    }
}