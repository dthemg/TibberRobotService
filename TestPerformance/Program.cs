using TestPerformance;
using TibberRobotService.Models;
using TibberRobotService.Services;

static Movement Move(Direction direction, int steps) => new()
    { Direction = direction, Steps = steps };

var repo = new RepoMock();
var random = new Random();
var service = new RobotService(repo);

var commands = 100;
var movement = new List<Movement>();

var alternatives = Enum.GetValues(typeof(Direction));

// Maximize number of overlapping points
for (int i = 0; i < commands; i++)
{
    var steps = 100000;
    if (i % 2 == 0)
    {
        movement.Add(Move(Direction.East, steps));
    }
    else
    {
        movement.Add(Move(Direction.West, steps));
    }
    
    /*
    var steps = random.Next(100000);
    var direction = (Direction)alternatives.GetValue(random.Next(alternatives.Length));
    movement.Add(Move(direction, 100000));
    */
}
var request = new MovementRequest()
{
    Start = new() { X = 0, Y = 0 },
    Commands = movement
};

var summary = await service.PerformRobotMovement(request);
Console.WriteLine($"Completed in {summary.Duration}");