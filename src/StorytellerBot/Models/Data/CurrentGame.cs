using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models.Data;

[Table("CurrentGame")]
public class CurrentGame
{
    public long UserId { get; set; }
    public int SavedStatusId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual SavedStatus SavedStatus { get; set; } = null!;
}
