using FluentAssertions;
using NSubstitute;
using NSubstitute.Routing.Handlers;
using TibberRobotService.Interfaces;
using TibberRobotService.Models;
using TibberRobotService.Services;
using Db = TibberRobotService.Models.Entities;

namespace TibberRobotServiceTests
{
    public class RobotServiceTests
    {
        private IRobotService _sut;
        private IRobotMovementRepository _movementRepository;

        [SetUp]
        public void Setup()
        {
            _movementRepository = Substitute.For<IRobotMovementRepository>();
            _movementRepository.AddMovementSummary(default, default, default)
                .ReturnsForAnyArgs(x => new Db.MovementSummary
                {
                    Commands = (int)x[0],
                    Result = (int)x[1],
                    Duration = (float)x[2]
                });

            _sut = new RobotService(_movementRepository);
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
            var request = new MovementRequest()
            {
                Start = new()
                {
                    X = 10,
                    Y = 20
                },
                Commands = new()
                {
                    new()
                    {
                        Steps = steps,
                        Direction = Direction.West
                    }
                }
            };

            var summary = await _sut.PerformRobotMovement(request);
            summary.Commands.Should().Be(1);
            summary.Result.Should().Be(steps + 1); 
        }

        private Movement Move(Direction direction, int steps) => new() 
            { Direction = direction, Steps= steps };

        [Test]
        public async Task PerformRobotMovement_should_handle_horizontal_crossings_correctly()
        {
            var request = new MovementRequest()
            {
                Start = new() { X = 0, Y = 0 },
                Commands = new()
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
                }
            };
            
            // Current status! This is failing miserably!

            var summary = await _sut.PerformRobotMovement(request);
            summary.Result.Should().Be(33);
        }
    }
}