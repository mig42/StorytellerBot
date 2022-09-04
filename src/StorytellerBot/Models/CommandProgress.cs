using System.ComponentModel.DataAnnotations;

namespace StorytellerBot.Models;

public class CommandProgress
{
    [Key]
    public int Id { get; set; }
    public string Command { get; set; } = string.Empty;
    public string? Argument { get; set; }
    public string Step { get; set; } = string.Empty;
    public long UserId { get; set; }
    public User User { get; set; } = null!;
}
