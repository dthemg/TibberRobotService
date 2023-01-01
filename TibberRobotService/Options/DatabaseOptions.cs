using System.ComponentModel.DataAnnotations;

#nullable disable

namespace TibberRobotService.Options;

public class DatabaseOptions
{
    [Required]
    public string Host { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    public string DatabaseName { get; set; }
}