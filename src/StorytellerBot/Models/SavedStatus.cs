using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models;

[Table("SavedStatus")]
public class SavedStatus
{
    [Key]
    public int Id { get; set; }
    public long UserId { get; set; }
    public Adventure? Adventure { get; set; }
    public int AdventureId { get; set; }
    public string? SavedStatusJson { get; set; }
    [Required]
    public DateTime LastUpdated { get; set; }
}
