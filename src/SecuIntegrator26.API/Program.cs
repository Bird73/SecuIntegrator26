using Serilog;
using Microsoft.EntityFrameworkCore;
using SecuIntegrator26.Infrastructure.Data;
using Quartz;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register HTTP Client for Line Notify
builder.Services.AddHttpClient<SecuIntegrator26.Core.Interfaces.INotificationService, SecuIntegrator26.Infrastructure.Services.LineNotifyService>();

// Register DbContext (SQLite)
builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Generic Repository
builder.Services.AddScoped(typeof(SecuIntegrator26.Core.Interfaces.IRepository<>), typeof(SecuIntegrator26.Infrastructure.Data.Repository<>));

// Register Business Services
builder.Services.AddScoped<SecuIntegrator26.Core.Interfaces.IStockService, SecuIntegrator26.Services.StockService>();
builder.Services.AddScoped<SecuIntegrator26.Core.Interfaces.ICrawlerService, SecuIntegrator26.Infrastructure.Services.TwseCrawlerService>();
builder.Services.AddScoped<SecuIntegrator26.Core.Interfaces.IWebScraper, SecuIntegrator26.Infrastructure.Services.PlaywrightBridgeService>();
builder.Services.AddScoped<SecuIntegrator26.Core.Interfaces.IFileService, SecuIntegrator26.Infrastructure.Services.FileService>();
builder.Services.AddScoped<SecuIntegrator26.Core.Interfaces.IHolidayService, SecuIntegrator26.Services.HolidayService>();
builder.Services.AddScoped<SecuIntegrator26.Core.Interfaces.ISchedulerManagementService, SecuIntegrator26.Services.SchedulerManagementService>();

// Register Quartz
builder.Services.AddQuartz(q =>
{
    // q.UseMicrosoftDependencyInjectionJobFactory();
    
    // Register HelloWorldJob
    var helloJobKey = new JobKey("HelloWorldJob");
    q.AddJob<SecuIntegrator26.Services.Jobs.HelloWorldJob>(opts => opts.WithIdentity(helloJobKey));
    
    q.AddTrigger(opts => opts
        .ForJob(helloJobKey)
        .WithIdentity("HelloWorldJob-trigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(600) // Change to infrequent
            .RepeatForever()));

    // Register SyncStockSymbolsJob
    var syncJobKey = new JobKey("SyncStockSymbolsJob");
    q.AddJob<SecuIntegrator26.Services.Jobs.SyncStockSymbolsJob>(opts => opts.WithIdentity(syncJobKey));

    q.AddTrigger(opts => opts
        .ForJob(syncJobKey)
        .WithIdentity("SyncStockSymbolsJob-trigger")
        .WithCronSchedule("0 0 17 * * ?")); // Every day at 17:00

});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
