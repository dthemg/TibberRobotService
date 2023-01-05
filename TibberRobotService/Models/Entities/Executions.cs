using System.ComponentModel.DataAnnotations;

namespace TibberRobotService.Models.Entities;

public class Executions
{
    [Key]
    public Guid id { get; set; }

    public DateTime timestamp { get; set; }

    public int commands { get; set; }

    public long result { get; set; }

    public float duration { get; set; }
}
