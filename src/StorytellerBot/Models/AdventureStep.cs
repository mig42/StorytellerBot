using Ink.Runtime;

namespace StorytellerBot.Models;

public class AdventureStep
{
    public Story Story { get; set; } = null!;
    public IEnumerable<Paragraph> Paragraphs { get; set; } = Array.Empty<Paragraph>();
    public IEnumerable<Decision> Decisions { get; set; } = Array.Empty<Decision>();
    public bool IsEnding { get; set; }
}
