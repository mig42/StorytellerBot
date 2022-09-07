using System.ComponentModel.DataAnnotations;

namespace StorytellerBot.Models.Input;

public class AdventureInputModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9_\-]+$")]
    public string ScriptFileName { get; set; } = string.Empty;
}
