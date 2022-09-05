using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models;

[Table("CurrentGame")]
public class CurrentGame
{
    public long UserId { get; set; }
    public int SavedStatusId { get; set; }

    public User User { get; set; } = null!;
    public SavedStatus SavedStatus { get; set; } = null!;
}
