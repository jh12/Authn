using System.Diagnostics;
using Authn.Config;
using Authn.Services.Setup;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

string? configPath = Environment.GetEnvironmentVariable("Authn_ConfigPath");

if (!string.IsNullOrEmpty(configPath))
{
    builder.Configuration
        .AddJsonFile(configPath, false, true);
}

Configuration configuration = builder.Configuration.GetRequiredSection("authn").Get<Configuration>()!;

Logger CreateLogger()
{
    LoggerConfiguration logBuilder = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext();

    if (configuration.Log.UseCompactJson)
        logBuilder.WriteTo.Console(new CompactJsonFormatter());

    Logger logger1 = logBuilder.CreateLogger();
    return logger1;
}

Logger logger = CreateLogger();

builder.Logging.AddSerilog(logger, true);

IServiceCollection services = builder.Services;

services.AddControllers();

services
    .AddOpenIddict()
    .AddCore(options =>
    {
        options.UseMongoDb();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("connect/token");

        options.AllowClientCredentialsFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        OpenIddictServerAspNetCoreBuilder aspNetBuilder = options.UseAspNetCore()
            .EnableTokenEndpointPassthrough();

        if (configuration.Server.AllowHttpTraffic)
        {
            aspNetBuilder
                .DisableTransportSecurityRequirement();
        }
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();

        options.UseAspNetCore();
    });

builder.Services.AddAuthorization()
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

if (string.IsNullOrEmpty(configuration.MongoDb.ConnectionString) || string.IsNullOrEmpty(configuration.MongoDb.Database))
    throw new ArgumentException("Configuration section MongoDb is undefined or malformed");

services.AddSingleton(new MongoClient(configuration.MongoDb.ConnectionString).GetDatabase(configuration.MongoDb.Database));

if (args.Contains("--setup"))
{
    services.AddHostedService<DatabaseSetup>();
    services.AddHostedService<UserSetup>();
}

var app = builder.Build();

if (Debugger.IsAttached)
    app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
