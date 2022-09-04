using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models;

[Table("User")]
public class User
{
    [Key]
    public long Id { get; set; }
    public string? CurrentCommand { get; set; }
    public SavedStatus? CurrentGame { get; set; }
    public int CurrentGameId { get; set; }
}
