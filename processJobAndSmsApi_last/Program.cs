using System;
using System.Linq;
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
using processJobAndSmsApi.Services;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor;

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

    // 👉 MVC Support
   builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // Add this line
    builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
});

    builder.Services.Configure<DlrSettings>(builder.Configuration.GetSection("DLR"));

    builder.Services.AddScoped<UserNotificationService>();
    builder.Services.AddScoped<SmsService>();
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<BalanceService>();
    builder.Services.AddScoped<NumberService>();
    builder.Services.AddScoped<UserStatusService>();
    builder.Services.AddScoped<SmartLinkService>();

    builder.Services.AddSingleton<DlrLogParserService>(provider =>
        new DlrLogParserService(
            connectionString,
            provider.GetRequiredService<IOptions<DlrSettings>>(),
            provider.GetRequiredService<ILogger<DlrLogParserService>>()
        ));

    builder.Services.AddSingleton<IHostedService, FileWatcherService>();

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
    app.UseStaticFiles(); // To serve static files like JS/CSS from wwwroot

    app.UseRouting();

    app.UseAuthorization();

    // 👉 MVC Routing
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    });

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
