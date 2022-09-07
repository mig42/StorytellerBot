using StorytellerBot.Data;
using StorytellerBot.Models.Data;
using StorytellerBot.Models.Game;
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
        var user = await _repo.GetOrCreateUserAsync(update.Message!.From!.Id);
        var currentGame = await _repo.GetCurrentGameForUserAsync(user.Id);

        if (currentGame == null)
        {
            return await _responseSender.SendResponseAsync(new Response
            {
                ChatId = update.Message!.Chat,
                Text = $"No estás jugando ninguna aventura. Usa el comando /{Commands.Start} para empezar a jugar.",
            });
        }

        var commandProgress = await _repo.GetCommandProgressForUserAsync(update.Message!.From!.Id);

        if (IsInvalidState(commandProgress) || Commands.IsCommand(update.Message!.Text, Commands.Restart))
        {
            await _repo.DeleteCommandProgressAsync(commandProgress);
            await _repo.CreateCommandProgressAsync(new CommandProgress
            {
                Command = Commands.Restart,
                Step = State.Confirm,
                UserId = user.Id,
            });

            return await _responseSender.SendResponseAsync(new Response
            {
                ChatId = update.Message!.Chat.Id,
                Text = "Reiniciar la aventura borrará todos los datos de la partida actual. ¿Quieres continuar?",
                ReplyMarkup = ConfirmationKeyboard.Create(),
            });
        }

        if (commandProgress!.Step == State.Confirm)
        {
            await _repo.DeleteCommandProgressAsync(commandProgress);

            if (ConfirmationKeyboard.IsAccept(update.Message!.Text))
            {
                await _repo.UpdateSavedStatusAsync(currentGame.SavedStatus, string.Empty, DateTime.UtcNow);

                return await _responseSender.SendResponsesAsync(
                    await _adventureWriter.GetCurrentStepMessagesAsync(update.Message!.Chat, currentGame.SavedStatus));
            }
            else
            {
                return Enumerable.Empty<Message>();
            }
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
