using System.Text;
using StorytellerBot.Data;
using StorytellerBot.Models.Data;
using StorytellerBot.Models.Game;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace StorytellerBot.Services.Conversations;

public class StartCommandConversation : IConversation
{
    private readonly AdventureRepository _repo;
    private readonly IAdventureWriter _adventureWriter;

    public StartCommandConversation(AdventureRepository repo, IAdventureWriter adventureWriter)
    {
        _repo = repo;
        _adventureWriter = adventureWriter;
    }

    async Task<IEnumerable<Response>> IConversation.GetResponsesAsync(Update update)
    {
        var user = await _repo.GetOrCreateUserAsync(update.Message!.From!.Id);
        var commandProgress = await _repo.GetCommandProgressForUserAsync(user.Id);

        if (IsInvalidState(commandProgress) || Commands.IsCommand(update.Message.Text, Commands.Start))
        {
            await _repo.DeleteCommandProgressAsync(commandProgress);
            var adventures = await _repo.GetAllAdventuresAsync();
            if (adventures.Count == 0)
            {
                return new Response[]
                {
                    new Response
                    {
                        ChatId = update.Message.Chat,
                        Text = "No hay aventuras disponibles 😭 Prueba de nuevo más tarde.",
                    },
                };
            }

            await _repo.CreateCommandProgressAsync(new CommandProgress
            {
                Command = Commands.Start,
                Step = State.Start,
                UserId = user.Id,
            });

            return new Response[]
            {
                new Response
                {
                    ChatId = update.Message!.Chat.Id,
                    Text = await BuildStartMessageAsync(adventures),
                    ReplyMarkup = new ReplyKeyboardMarkup(adventures.Select(a => new KeyboardButton(a.Id.ToString()))),
                },
            };
        }

        if (commandProgress!.Step == State.Start)
        {
            int? adventureId = ParseAdventureId(update.Message!);
            if (adventureId == null)
            {
                return new Response[]
                {
                    new Response
                    {
                        ChatId = update.Message!.Chat.Id,
                        Text = "No entiendo ese número. Por favor, escribe el número de la aventura que quieres jugar.",
                    },
                };
            }

            var adventure = await _repo.GetAdventureAsync(adventureId.Value);
            if (adventure == null)
            {
                return new Response[]
                {
                    new Response
                    {
                        ChatId = update.Message!.Chat.Id,
                        Text = "No existe ninguna aventura con ese número. Por favor, escribe el número de la aventura que quieres jugar.",
                    },
                };
            }

            if (user.SavedGames.Any(a => a.Adventure.Id == adventure.Id))
            {
                await _repo.UpdateCommandProgressAsync(commandProgress, State.Confirm, adventure.Id);

                return new Response[]
                {
                    new Response
                    {
                        ChatId = update.Message!.Chat.Id,
                        Text = "Tienes una aventura guardada con ese número, se borrará su progreso. ¿Quieres continuar?",
                        ReplyMarkup = ConfirmationKeyboard.Create(),
                    },
                };
            }

            var currentGame = await _repo.StartGameAsync(user, adventure, DateTime.UtcNow);
            return await _adventureWriter.GetCurrentStepMessagesAsync(update.Message!.Chat, currentGame.SavedStatus);
        }

        if (commandProgress!.Step == State.Confirm)
        {
            await _repo.DeleteCommandProgressAsync(commandProgress);
            if (ConfirmationKeyboard.IsAccept(update.Message!.Text))
            {
                if (!int.TryParse(commandProgress.Argument! , out int adventureId))
                {
                    await _repo.DeleteCommandProgressAsync(commandProgress);
                    return Enumerable.Empty<Response>();
                }

                var currentGame = await _repo.GetCurrentGameForUserAsync(user.Id);
                await _repo.UpdateSavedStatusAsync(currentGame?.SavedStatus, string.Empty, DateTime.UtcNow);
                return await _adventureWriter.GetCurrentStepMessagesAsync(update.Message!.Chat, currentGame?.SavedStatus);
            }
            else
            {
                await _repo.DeleteCommandProgressAsync(commandProgress);
                return Enumerable.Empty<Response>();
            }
        }

        return Enumerable.Empty<Response>();
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

    private static Task<string> BuildStartMessageAsync(IEnumerable<Adventure> adventures)
    {
        StringBuilder sb = new();
        sb.AppendLine("*Elige una aventura:*");
        sb.AppendLine();

        foreach (var adventure in adventures)
        {
            sb.AppendLine($"{adventure.Id}. {adventure.Name}");
        }

        sb.AppendLine();
        sb.AppendLine("Envíame el número de la aventura que quieras jugar.");

        return Task.FromResult(sb.ToString());
    }

    static class State
    {
        public const string Start = "start";
        public const string Confirm = "confirm";
    }
}
