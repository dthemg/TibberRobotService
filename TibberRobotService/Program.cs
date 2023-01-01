using Microsoft.EntityFrameworkCore;
using Npgsql;
using TibberRobotService.Interfaces;
using TibberRobotService.Options;
using TibberRobotService.Repositories;
using TibberRobotService.Services;
using Db = TibberRobotService.Models.Entities;

namespace TibberRobotService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        DatabaseOptions dbOptions = new();
        config.GetSection("DatabaseOptions").Bind(dbOptions);

        builder.Services.AddPooledDbContextFactory<Db.RobotServiceDbContext>((sp, options) =>
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder()
            {
                Host = dbOptions.Host,
                Username = dbOptions.Username,
                Password = dbOptions.Password,
                Database = dbOptions.DatabaseName
            };
            options.UseNpgsql(connectionStringBuilder.ConnectionString);
        });

        builder.Services.AddTransient<IRobotMovementRepository, RobotMovementRepository>();
        builder.Services.AddTransient<IRobotService, RobotService>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}