using StorytellerBot.Models.Data;
using StorytellerBot.Models.Game;
using Telegram.Bot.Types;

namespace StorytellerBot.Services
{
    public interface IAdventureWriter
    {
        Task<IEnumerable<Response>> GetCurrentStepMessagesAsync(ChatId chatId, SavedStatus? savedStatus);
        Task<string> AdvanceAdventureAsync(SavedStatus savedStatus, int decisionIndex);
    }
}
