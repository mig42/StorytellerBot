namespace StorytellerBot.Models;

public class AdventureStep
{
    public IEnumerable<string> Paragraphs { get; set; } = Array.Empty<string>();
    public IEnumerable<Decision> Decisions { get; set; } = Array.Empty<Decision>();
    public bool IsEnding { get; set; }
}
