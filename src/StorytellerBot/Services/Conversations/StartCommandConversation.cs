using System.Text;
using StorytellerBot.Data;
using StorytellerBot.Models;
using Telegram.Bot.Types;

namespace StorytellerBot.Services.Conversations;

public class StartCommandConversation : IConversation
{
    private readonly AdventureRepository _repo;
    private readonly IResponseSender _responseSender;
    private readonly IAdventureWriter _adventureWriter;

    public StartCommandConversation(
        AdventureRepository repo, IResponseSender responseSender, IAdventureWriter adventureWriter)
    {
        _repo = repo;
        _responseSender = responseSender;
        _adventureWriter = adventureWriter;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var user = await _repo.GetOrCreateUserAsync(update.Message!.From!.Id);
        var commandProgress = await _repo.GetCommandProgressForUserAsync(user.Id);

        if (IsInvalidState(commandProgress))
        {
            await _repo.DeleteCommandProgressAsync(commandProgress);
            await _repo.CreateCommandProgressAsync(new CommandProgress
            {
                Command = Commands.Start,
                Step = State.Start,
                UserId = user.Id,
            });

            return await _responseSender.SendResponseAsync(new Response
            {
                ChatId = update.Message!.Chat.Id,
                Text = await BuildStartMessageAsync(),
            });
        }

        if (commandProgress!.Step == State.Start)
        {
            int? adventureId = ParseAdventureId(update.Message!);
            if (adventureId == null)
            {
                return await _responseSender.SendResponseAsync(new Response
                {
                    ChatId = update.Message!.Chat.Id,
                    Text = "No entiendo ese número. Por favor, escribe el número de la aventura que quieres jugar.",
                });
            }

            if (!(await _repo.AdventureExistsAsync(adventureId.Value)))
            {
                return await _responseSender.SendResponseAsync(new Response
                {
                    ChatId = update.Message!.Chat.Id,
                    Text = "No existe ninguna aventura con ese número. Por favor, escribe el número de la aventura que quieres jugar.",
                });
            }

            if (user.SavedGames.Any(a => a.Id == adventureId))
            {
                await _repo.UpdateCommandProgressAsync(commandProgress, State.Confirm, adventureId);

                return await _responseSender.SendResponseAsync(new Response
                {
                    ChatId = update.Message!.Chat.Id,
                    Text = "Tienes una aventura guardada con ese número, se borrará su progreso. ¿Quieres continuar?",
                    ReplyMarkup = ConfirmationKeyboard.Create(),
                });
            }

            await _repo.StartGameAsync(user, adventureId.Value, DateTime.UtcNow);
            return Enumerable.Empty<Message>();
        }

        if (commandProgress!.Step == State.Confirm)
        {
            await _repo.DeleteCommandProgressAsync(commandProgress);
            if (ConfirmationKeyboard.IsAccept(update.Message!.Text))
            {
                if (!int.TryParse(commandProgress.Argument! , out int adventureId))
                {
                    await _repo.DeleteCommandProgressAsync(commandProgress);
                    return Enumerable.Empty<Message>();
                }

                var currentGame = await _repo.GetCurrentGameForUserAsync(user.Id);
                await _repo.UpdateSavedStatusAsync(currentGame?.SavedStatus, string.Empty, DateTime.UtcNow);
                return await _responseSender.SendResponsesAsync(
                    await _adventureWriter.GetCurrentStepMessagesAsync(update.Message!.Chat, currentGame?.SavedStatus));
            }
            else
            {
                await _repo.DeleteCommandProgressAsync(commandProgress);
                return Enumerable.Empty<Message>();
            }
        }

        return Enumerable.Empty<Message>();
    }

    private static bool IsInvalidState(CommandProgress? command)
    {
        if (command is null || command.Command != Commands.Start)
        {
            return true;
        }

        return command.Step != State.Start && command.Step != State.Confirm;
    }

    private static int? ParseAdventureId(Message message)
    {
        return int.TryParse(message?.Text, out int adventureId) ? adventureId : null;
    }

    private async Task<string> BuildStartMessageAsync()
    {
        var adventures = await _repo.GetAllAdventuresAsync();

        StringBuilder sb = new();
        sb.AppendLine("*Elige una aventura:*");
        sb.AppendLine();

        foreach (var adventure in adventures)
        {
            sb.AppendLine($"{adventure.Id}. {adventure.Name}");
        }

        sb.AppendLine();
        sb.AppendLine("Envíame el número de la aventura que quieras jugar.");

        return sb.ToString();
    }

    static class State
    {
        public const string Start = "start";
        public const string Confirm = "confirm";
    }
}
