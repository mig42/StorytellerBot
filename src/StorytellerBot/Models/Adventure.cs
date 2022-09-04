using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorytellerBot.Models;

[Table("Adventure")]
public class Adventure
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ScriptFileName { get; set; } = string.Empty;
}
