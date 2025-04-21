using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Serilog;
using Serilog.Events;
using processJobAndSmsApi.Data;
using processJobAndSmsApi.Models.Configuration;
using System.Text.RegularExpressions;  // For Regex
using processJobAndSmsApi.Services;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Database connection string is missing");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25))));


    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errorList = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new { Name = e.Key, Errors = e.Value?.Errors.Select(er => er.ErrorMessage).ToArray() })
                    .ToArray();
                foreach (var error in errorList)
                {
                    Log.Logger.Error("Model validation failed for {Name}: {Errors}", error.Name, string.Join(", ", error.Errors ?? []));
                }
                return new BadRequestObjectResult(context.ModelState);
            };
        });

    builder.Services.Configure<DlrSettings>(builder.Configuration.GetSection("DLR"));

    builder.Services.AddScoped<UserNotificationService>();
    builder.Services.AddScoped<SmsService>();
    // builder.Services.AddHostedService<ScheduledSmsProcessor>();

    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<BalanceService>();
    builder.Services.AddScoped<NumberService>();
    builder.Services.AddScoped<UserStatusService>();

    builder.Services.AddSingleton<DlrLogParserService>(provider => 
        new DlrLogParserService(
            connectionString,
            provider.GetRequiredService<IOptions<DlrSettings>>(),  // Pass IOptions
            provider.GetRequiredService<ILogger<DlrLogParserService>>()
        ));

    builder.Services.AddSingleton<IHostedService, FileWatcherService>();

    builder.Services.AddScoped<SmartLinkService>();

    builder.Services.AddHttpClient();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }


    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}