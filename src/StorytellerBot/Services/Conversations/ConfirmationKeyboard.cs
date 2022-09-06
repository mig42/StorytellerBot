using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Services;

internal static class ConfirmationKeyboard
{
    private const string YES = "Sí";
    private const string NO = "No";

    internal static bool IsAccept(string? response)
    {
        return string.Compare(
            response, YES, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
    }

    public static ReplyKeyboardMarkup Create()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton(YES),
            new KeyboardButton(NO),
        });
    }
}
