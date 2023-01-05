using FluentAssertions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using NSubstitute.Routing.Handlers;
using TibberRobotService.Interfaces;
using TibberRobotService.Models;
using TibberRobotService.Services;
using Db = TibberRobotService.Models.Entities;

namespace TibberRobotServiceTests;

public class RobotServiceTests
{
    private IRobotService _sut;
    private IRobotMovementRepository _movementRepository;

    [SetUp]
    public void Setup()
    {
        _movementRepository = Substitute.For<IRobotMovementRepository>();
        _movementRepository.AddMovementSummary(default, default, default)
            .ReturnsForAnyArgs(x => new Db.Executions
            {
                commands = (int)x[0],
                result = (long)x[1],
                duration = (float)x[2]
            });

        var movementCalculator = new RobotMovementCalculatorService();

        _sut = new RobotService(_movementRepository, movementCalculator);
    }

    [Test]
    public async Task PerformRobotMovement_should_handle_no_movements()
    {
        var request = new MovementRequest()
        {
            Start = new() { X = 0, Y = 0 },
            Commands = new()
        };

        var summary = await _sut.PerformRobotMovement(request);

        summary.Commands.Should().Be(0);
        summary.Result.Should().Be(1, because: "the starting square should have been cleaned");
    }

    [Test]
    [TestCase(Direction.West)]
    [TestCase(Direction.North)]
    [TestCase(Direction.East)]
    [TestCase(Direction.South)]
    public async Task PerformRobotMovement_should_compute_correctly_for_movement_in_straight_line(Direction direction)
    {
        var steps = 100;
        var request = MakeRequest(new()
        {
            Move(direction, steps)
        });

        var summary = await _sut.PerformRobotMovement(request);
        summary.Commands.Should().Be(1);
        summary.Result.Should().Be(steps + 1);
    }

    [Test]
    public async Task PerformRobotMovement_should_handle_crossings_correctly()
    {
        var request = MakeRequest(new()
        {
            Move(Direction.North, 8),
            Move(Direction.West, 2),
            Move(Direction.South, 2),
            Move(Direction.East, 8),
            Move(Direction.North, 2),
            Move(Direction.West, 2),
            Move(Direction.South, 8),
            Move(Direction.East, 2),
            Move(Direction.North, 2),
            Move(Direction.West, 8),
            Move(Direction.South, 2),
            Move(Direction.East, 2)
        });

        var summary = await _sut.PerformRobotMovement(request);
        summary.Result.Should().Be(44);
    }

    [Test]
    public async Task PerformRobotMovement_should_handle_complete_overlaps_correctly()
    {
        var request = MakeRequest(new()
        {
            Move(Direction.East, 10),
            Move(Direction.West, 10),
            Move(Direction.South, 10),
            Move(Direction.North, 10),
        });

        var summary = await _sut.PerformRobotMovement(request);
        summary.Result.Should().Be(21);
    }

    [Test]
    public async Task PerformRobotMovement_should_handle_partial_horizontal_overlaps_correctly()
    {
        var request = MakeRequest(new()
        {
            Move(Direction.East, 10),
            Move(Direction.North, 1),
            Move(Direction.East, 2),
            Move(Direction.South, 1),
            Move(Direction.West, 13),
            Move(Direction.East, 3),
            Move(Direction.East, 5),
            Move(Direction.East, 5)
        });

        var summary = await _sut.PerformRobotMovement(request);
        summary.Result.Should().Be(17);
    }

    [Test]
    public async Task PerformRobotMovement_should_handle_partial_vertical_overlaps_correctly()
    {
        var request = MakeRequest(new()
        {
            Move(Direction.North, 10),
            Move(Direction.West, 1),
            Move(Direction.North, 2),
            Move(Direction.East, 1),
            Move(Direction.South, 13),
            Move(Direction.North, 3),
            Move(Direction.North, 5),
            Move(Direction.North, 5)
        });

        var summary = await _sut.PerformRobotMovement(request);
        summary.Result.Should().Be(17);
    }

    [Test]
    public async Task PerformRobotMovement_should_handle_going_in_a_circle()
    {
        var S = 10;
        var request = MakeRequest(new()
        {
            Move(Direction.North, S),
            Move(Direction.East, S),
            Move(Direction.South, S),
            Move(Direction.West, S),
            Move(Direction.North, S),
            Move(Direction.East, S),
            Move(Direction.South, S),
            Move(Direction.West, S)
        });

        var summary = await _sut.PerformRobotMovement(request);
        summary.Result.Should().Be(40L);
    }

    [Test]
    public async Task PerformRobotMovement_should_be_performant_when_going_back_and_forth()
    {
        var steps = 100000;
        var numberOfCommands = 1000;
        var commands = new List<Command>();
        // Maximize number of overlapping points
        for (int i = 0; i < numberOfCommands; i++)
        {
            if (i%2 == 0)
            {
                commands.Add(Move(Direction.East, steps));
            } else
            {
                commands.Add(Move(Direction.West, steps));
            }
        }
        var request = MakeRequest(commands);

        var summary = await _sut.PerformRobotMovement(request);
        summary.Result.Should().Be(steps + 1);
        summary.Duration.Should().BeLessThan(0.05f);
    }

    [Test]
    public async Task PerformRobotMovement_should_be_performant_in_random_movement_scenario()
    {
        var possibleDirections = Enum.GetValues(typeof(Direction));
        var commands = new List<Command>();
        var numberOfCommands = 1000;
        var random = new Random();
        for (int i = 0; i < numberOfCommands; i++)
        {
            var steps = random.Next(100000);
            if (possibleDirections.GetValue(random.Next(possibleDirections.Length)) is Direction direction)
                commands.Add(Move(direction, steps));
        }

        var request = MakeRequest(commands);

        var summary = await _sut.PerformRobotMovement(request);
        summary.Duration.Should().BeLessThan(0.05f);
    }

    private static Command Move(Direction direction, int steps) => new()
    { Direction = direction, Steps = steps };

    private static MovementRequest MakeRequest(List<Command> commands) => new MovementRequest()
    {
        Start = new() { X = 0, Y = 0 },
        Commands = commands
    };
}