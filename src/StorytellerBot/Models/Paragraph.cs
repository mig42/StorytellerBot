namespace StorytellerBot.Models;

public class Paragraph
{
    public string Text { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string Image => Tags
        .FirstOrDefault(tag => tag.StartsWith(KnownTags.Image, StringComparison.OrdinalIgnoreCase))?
        .Substring(KnownTags.Image.Length)?
        .Trim() ?? string.Empty;
}
