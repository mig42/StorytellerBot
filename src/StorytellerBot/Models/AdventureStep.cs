using Ink.Runtime;

namespace StorytellerBot.Models;

public class AdventureStep
{
    public Story Story { get; set; } = null!;
    public IEnumerable<string> Paragraphs { get; set; } = Array.Empty<string>();
    public IEnumerable<Decision> Decisions { get; set; } = Array.Empty<Decision>();
    public bool IsEnding { get; set; }
}
