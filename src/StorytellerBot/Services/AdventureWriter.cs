using Ink.Runtime;
using Microsoft.Extensions.Options;
using StorytellerBot.Data;
using StorytellerBot.Models;
using StorytellerBot.Settings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using Path = System.IO.Path;

namespace StorytellerBot.Services;

public class AdventureWriter : IAdventureWriter
{
    private string _baseScriptsPath;

    private readonly AdventureContext _context;
    private readonly ILogger<AdventureWriter> _logger;
    private const string EXTENSION = ".json";

    public AdventureWriter(
        IOptions<GameConfiguration> gameConfig,
        IHostEnvironment environment,
        AdventureContext context,
        ILogger<AdventureWriter> logger)
    {
        _baseScriptsPath = Path.Combine(environment.ContentRootPath, gameConfig.Value.ScriptsDirectory);
        _context = context;
        _logger = logger;
    }

    async Task<string> IAdventureWriter.AdvanceAdventureAsync(SavedStatus savedStatus, int decisionIndex)
    {
        var story = await LoadStory(savedStatus);
        if (story == null)
        {
            return string.Empty;
        }

        while (story.canContinue)
        {
            story.Continue();
        }
        story.ChooseChoiceIndex(decisionIndex);
        return story.state.ToJson();
    }

    async Task<IEnumerable<Response>> IAdventureWriter.GetCurrentStepMessagesAsync(ChatId chatId, SavedStatus? currentGame)
    {
        if (currentGame == null)
        {
            _logger.LogError("Unable to find next step for null save");
            return Enumerable.Empty<Response>();
        }

        AdventureStep? adventureStep = await GetNextStep(currentGame);
        if (adventureStep == null)
        {
            _logger.LogError(
                "Unable to find next step for adventure {AdventureId}, save #{SaveId}",
                currentGame.Adventure.Id, currentGame.Id);
            return Enumerable.Empty<Response>();
        }

        var result = adventureStep.Paragraphs
            .Select((p, idx) => new Response
            {
                ChatId = chatId,
                Text = p.Text,
                Image = p.Image,
                ReplyMarkup = idx == adventureStep.Paragraphs.Count() - 1
                    ? BuildDecisionsInlineKeyboard(adventureStep)
                    : null,
            });

        if (adventureStep.IsEnding)
        {
            result = result.Append(new Response
            {
                ChatId = chatId,
                Text = $"La aventura ha llegado a su fin. Utiliza /{Commands.Start} para comenzar una nueva.",
                ReplyMarkup = new ReplyKeyboardRemove(),
                IsEndOfAdventure = true,
            });
        }
        return result;
    }

    private async Task<AdventureStep?> GetNextStep(SavedStatus? savedStatus)
    {
        var story = await LoadStory(savedStatus);
        if (story == null)
            return null;

        var paragraphs = new List<Paragraph>();
        while (story.canContinue)
        {
            story.Continue();
            paragraphs.Add(new Paragraph
            {
                Text = story.currentText,
                Tags = story.currentTags,
            });
        }

        return new AdventureStep
        {
            Paragraphs = paragraphs,
            Decisions = story.currentChoices.Select((choice, index) => new Decision
            {
                Text = choice.text,
                ChoiceIndex = index,
            }),
            IsEnding = !story.currentChoices.Any(),
            Story = story,
        };
    }

    private async Task<Story?> LoadStory(SavedStatus? savedStatus)
    {
        if (savedStatus?.Adventure == null)
            return null;

        var storyPath = Path.Combine(_baseScriptsPath, $"{savedStatus!.Adventure.ScriptFileName}{EXTENSION}");
        if (!File.Exists(storyPath))
            return null;

        var jsonContent = await File.ReadAllTextAsync(storyPath);
        var story = new Story(jsonContent.Trim());
        if (!string.IsNullOrEmpty(savedStatus?.StoryState))
            story.state.LoadJson(savedStatus.StoryState);

        return story;
    }

    private static InlineKeyboardMarkup BuildDecisionsInlineKeyboard(AdventureStep adventureStep)
    {
        if (adventureStep.IsEnding)
            return InlineKeyboardMarkup.Empty();

        return new InlineKeyboardMarkup(adventureStep.Decisions.Select(decision => new[]
        {
            InlineKeyboardButton.WithCallbackData(decision.Text, decision.ChoiceIndex.ToString()),
        }));
    }
}
