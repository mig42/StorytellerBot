using System.ComponentModel.DataAnnotations;

namespace StorytellerBot.Models;

public class CurrentGame
{
    [Key]
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public long UserId { get; set; }
    public SavedStatus SavedStatus { get; set; } = null!;
    public int SavedStatusId { get; set; }
}
