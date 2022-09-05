using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models;

[Table("User")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    public List<SavedStatus> SavedGames { get; set; } = new();
    public CurrentGame? CurrentGame { get; set; }
    public CommandProgress? CommandProgress { get; set; }
}
