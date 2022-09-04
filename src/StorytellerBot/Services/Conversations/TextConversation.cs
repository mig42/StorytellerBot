using Ink.Runtime;
using Microsoft.Extensions.Options;
using StorytellerBot.Data;
using StorytellerBot.Models;
using StorytellerBot.Settings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using Path = System.IO.Path;

namespace StorytellerBot.Services.Conversations;

public class TextConversation : ITextConversation
{
    private string _baseScriptsPath;
    private readonly AdventureContext _context;
    private IResponseSender _responseSender;
    private const string EXTENSION = ".json";

    public TextConversation(
        IOptionsSnapshot<GameConfiguration> gameConfig,
        IHostEnvironment environment,
        AdventureContext context,
        IResponseSender responseSender)
    {
        _baseScriptsPath = Path.Combine(environment.ContentRootPath, gameConfig.Value.ScriptsDirectory);
        _context = context;
        _responseSender = responseSender;
    }

    async Task<IEnumerable<Message>> IConversation.SendResponsesAsync(Update update)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == update.Message!.From!.Id);
        if (user?.CurrentGame?.Adventure == null)
        {
            return Enumerable.Empty<Message>();
        }


        AdventureStep? adventureStep = await GetNextStep(user.CurrentGame);

        if (adventureStep == null)
        {
            return await _responseSender.SendResponseAsync(new Response
            {
                ChatId = update.Message!.Chat,
                Text = "No te entiendo!",
            });
        }

        var responses = new List<Response>();
        responses.AddRange(adventureStep.Paragraphs.SkipLast(1).Select(p => new Response
        {
            ChatId = update.Message!.Chat,
            Text = p,
        }));
        responses.Add(new Response
        {
            ChatId = update.Message!.Chat,
            Text = adventureStep.Paragraphs.Last(),
            ReplyMarkup = BuildDecisionsInlineKeyboard(adventureStep),
        });

        return await _responseSender.SendResponsesAsync(responses);
    }

    internal async Task<AdventureStep?> GetNextStep(SavedStatus? savedStatus)
    {
        if (savedStatus?.Adventure == null)
            return null;

        var storyPath = Path.Combine(_baseScriptsPath, $"{savedStatus!.Adventure.ScriptFileName}{EXTENSION}");
        if (!File.Exists(storyPath))
            return null;

        var jsonContent = await File.ReadAllTextAsync(storyPath);
        var story = new Story(jsonContent.Trim());
        if (savedStatus != null)
            story.state.LoadJson(savedStatus.StoryState);

        var paragraphs = new List<string>();
        while (story.canContinue)
        {
            story.Continue();
            paragraphs.Add(story.currentText);
        }

        return new AdventureStep
        {
            Paragraphs = paragraphs,
            Decisions = story.currentChoices.Select(choice => new Decision
            {
                Text = choice.text,
                Path = choice.pathStringOnChoice,
            }),
            IsEnding = !story.currentChoices.Any(),
            Story = story,
        };
    }

    private static InlineKeyboardMarkup BuildDecisionsInlineKeyboard(AdventureStep adventureStep)
    {
        if (adventureStep.IsEnding)
            return InlineKeyboardMarkup.Empty();

        return new InlineKeyboardMarkup(adventureStep.Decisions.Select(decision => new[]
        {
            InlineKeyboardButton.WithCallbackData(decision.Text, decision.Path),
        }));
    }
}
