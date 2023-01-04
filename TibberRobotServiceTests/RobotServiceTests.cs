using FluentAssertions;
using NSubstitute;
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
                .ReturnsForAnyArgs(x => new Db.Executions
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
                        Direction = direction
                    }
                }
            };

            var summary = await _sut.PerformRobotMovement(request);
            summary.Commands.Should().Be(1);
            summary.Result.Should().Be(steps + 1);
        }

        private Movement Move(Direction direction, int steps) => new()
        { Direction = direction, Steps = steps };

        [Test]
        public async Task PerformRobotMovement_should_handle_crossings_correctly()
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

            var summary = await _sut.PerformRobotMovement(request);
            summary.Result.Should().Be(44);
        }

        [Test]
        public async Task PerformRobotMovement_should_handle_complete_overlaps_correctly()
        {
            var request = new MovementRequest()
            {
                Start = new() { X = 0, Y = 0 },
                Commands = new()
                {
                    Move(Direction.East, 10),
                    Move(Direction.West, 10),
                    Move(Direction.South, 10),
                    Move(Direction.North, 10),
                }
            };

            var summary = await _sut.PerformRobotMovement(request);
            summary.Result.Should().Be(21);
        }

        [Test]
        public async Task PerformRobotMovement_should_handle_partial_horizontal_overlaps_correctly()
        {
            var request = new MovementRequest()
            {
                Start = new() { X = 0, Y = 0 },
                Commands = new()
                {
                    Move(Direction.East, 10),
                    Move(Direction.North, 1),
                    Move(Direction.East, 2),
                    Move(Direction.South, 1),
                    Move(Direction.West, 13),
                    Move(Direction.East, 3),
                    Move(Direction.East, 5),
                    Move(Direction.East, 5)
                }
            };

            var summary = await _sut.PerformRobotMovement(request);
            summary.Result.Should().Be(17);
        }

        [Test]
        public async Task PerformRobotMovement_should_handle_partial_vertical_overlaps_correctly()
        {
            var request = new MovementRequest()
            {
                Start = new() { X = 0, Y = 0 },
                Commands = new()
                {
                    Move(Direction.North, 10),
                    Move(Direction.West, 1),
                    Move(Direction.North, 2),
                    Move(Direction.East, 1),
                    Move(Direction.South, 13),
                    Move(Direction.North, 3),
                    Move(Direction.North, 5),
                    Move(Direction.North, 5)
                }
            };

            var summary = await _sut.PerformRobotMovement(request);
            summary.Result.Should().Be(17);
        }

        [Test]
        public async Task PerformRobotMovement_should_be_performant_in_worst_case_scenario()
        {
            var steps = 100000;
            var commands = 100;
            var movement = new List<Movement>();
            // Maximize number of overlapping points
            for (int i = 0; i < commands; i++)
            {
                if (i%2 == 0)
                {
                    movement.Add(Move(Direction.East, steps));
                } else
                {
                    movement.Add(Move(Direction.West, steps));
                }
            }
            var request = new MovementRequest()
            {
                Start = new() { X = 0, Y = 0 },
                Commands = movement
            };

            var summary = await _sut.PerformRobotMovement(request);
            summary.Result.Should().Be(steps + 1);
            summary.Duration.Should().BeLessThan(0.2f);
        }
    }
}