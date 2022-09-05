using StorytellerBot.Data;
using StorytellerBot.Models;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class RestartCommandConversation : IConversation
{
    private readonly AdventureRepository _repo;
    private readonly IResponseSender _responseSender;
    private readonly IAdventureWriter _adventureWriter;

    public RestartCommandConversation(
        AdventureRepository repo, IResponseSender responseSender, IAdventureWriter adventureWriter)
    {
        _repo = repo;
        _responseSender = responseSender;
        _adventureWriter = adventureWriter;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var user = await _repo.GetUserAsync(update.Message!.From!.Id);

        if (user?.CurrentGame == null)
        {
            return await _responseSender.SendResponseAsync(new Response
            {
                ChatId = update.Message!.Chat,
                Text = $"No estás jugando ninguna aventura. Usa el comando /{Commands.Start} para empezar a jugar.",
            });
        }

        if (IsInvalidState(user.CommandProgress))
        {
            await _repo.ReplaceCommandProgressAsync(user, new CommandProgress
            {
                Command = Commands.Restart,
                Step = State.Confirm,
                UserId = user.Id,
            });

            return await _responseSender.SendResponseAsync(new Response
            {
                ChatId = update.Message!.Chat.Id,
                Text = "Reiniciar la aventura borrará todos los datos de la partida actual. ¿Quieres continuar?",
            });
        }

        if (user.CommandProgress!.Step == State.Confirm)
        {
            await _repo.ResetGameAsync(user.CommandProgress, user.CurrentGame.SavedStatus, DateTime.UtcNow);

            return await _responseSender.SendResponsesAsync(
                await _adventureWriter.GetCurrentStepMessagesAsync(update.Message!.Chat, user.CurrentGame.SavedStatus));
        }

        return Enumerable.Empty<Message>();
    }

    private bool IsInvalidState(CommandProgress? commandProgress)
    {
        if (commandProgress == null || commandProgress.Command != Commands.Restart)
        {
            return true;
        }
        return commandProgress.Step != State.Confirm;
    }

    private static class State
    {
        public const string Confirm = "confirm";
    }
}
