using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models.Data;

[Table("SavedStatus")]
public class SavedStatus
{
    [Key]
    public int Id { get; set; }
    public long UserId { get; set; }
    public int AdventureId { get; set; }
    public string? StoryState { get; set; }
    public DateTime LastUpdated { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Adventure Adventure { get; set; } = null!;
}
