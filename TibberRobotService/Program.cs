using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Text.Json.Serialization;
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

        // Database configuration
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

        // Repositories
        builder.Services.AddTransient<IRobotMovementRepository, RobotMovementRepository>();
        
        // Services
        builder.Services.AddTransient<IRobotService, RobotService>();
        
        // Configure controllers
        builder.Services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        
        
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