using TestPerformance;
using TibberRobotService.Models;
using TibberRobotService.Services;

static Movement Move(Direction direction, int steps) => new()
    { Direction = direction, Steps = steps };

var repo = new RepoMock();
var random = new Random();
var service = new RobotService(repo);

var commands = 10000;
var maxStep = 100000;
var movementBackForth = new List<Movement>();
var movementRandom = new List<Movement>();

var alternatives = Enum.GetValues(typeof(Direction));

// Maximize number of overlapping points
for (int i = 0; i < commands; i++)
{
    var steps = maxStep;
    if (i % 2 == 0)
        movementBackForth.Add(Move(Direction.East, steps));
    else
        movementBackForth.Add(Move(Direction.West, steps));
        
    steps = random.Next(maxStep);
    var direction = (Direction)alternatives.GetValue(random.Next(alternatives.Length));
    movementRandom.Add(Move(direction, steps));
}
var requestBackForth = new MovementRequest()
{
    Start = new() { X = 0, Y = 0 },
    Commands = movementBackForth
};
var requestRandom = new MovementRequest()
{
    Start = new() { X = 0, Y = 0 },
    Commands = movementRandom
};

// var summaryBackForth = await service.PerformRobotMovement(requestBackForth);
var summaryRandom = await service.PerformRobotMovement(requestRandom);
// Console.WriteLine($"BACK FORTH: {commands} iterations :{summaryBackForth.duration}");
Console.WriteLine($"RANDOM: {commands} iterations :{summaryRandom.Duration}");
