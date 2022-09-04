using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models;

[Table("User")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    public int? CurrentGameId { get; set; }
    public SavedStatus? CurrentGame { get; set; }
    public int? CommandProgressId { get; set; }
    public CommandProgress? CommandProgress { get; set; } = null!;

    public List<SavedStatus> SavedGames { get; set; } = new();
}
