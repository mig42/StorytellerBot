using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models;

[Table("SavedStatus")]
public class SavedStatus
{
    [Key]
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public long UserId { get; set; }
    public Adventure Adventure { get; set; } = null!;
    public int AdventureId { get; set; }
    public string? StoryState { get; set; }
    public DateTime LastUpdated { get; set; }
}
