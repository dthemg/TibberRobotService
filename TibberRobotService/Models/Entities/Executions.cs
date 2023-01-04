using System.ComponentModel.DataAnnotations;

namespace TibberRobotService.Models.Entities;

public class Executions
{
    [Key]
    public Guid Id { get; set; }

    public DateTime Timestamp { get; set; }

    public int Commands { get; set; }

    public int Result { get; set; }

    public float Duration { get; set; }
}
