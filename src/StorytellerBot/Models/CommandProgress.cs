using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models;

[Table("CommandProgress")]
public class CommandProgress
{
    [Key]
    public int Id { get; set; }
    public string Command { get; set; } = string.Empty;
    public string? Argument { get; set; }
    public string Step { get; set; } = string.Empty;
    public long UserId { get; set; }
    public virtual User User { get; set; } = null!;
}
