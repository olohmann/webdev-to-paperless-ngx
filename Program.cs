using System.Net;
using System.Runtime.InteropServices;
using Prometheus;
using Serilog;
using Serilog.Events;
using WebDavToPaperlessNGX.Middlewares;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .CreateLogger();

/*
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder
    .AddInMemoryCollection(DefaultConfiguration)
    .AddJsonFile("appsettings.json", false, false)
    .AddEnvironmentVariables();

Configuration = configurationBuilder.Build();
*/

// Support CTRL-C
using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (sender, arguments) =>
{
    arguments.Cancel = true;

    // ReSharper disable once AccessToDisposedClosure | endless loop follows.
    cancellationTokenSource.Cancel();
};

try
{
    
    var isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    if (isOSX) // issue with TLS 1.3 on OSX
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
    }
    else
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
    }

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.UseHttpMetrics();
    app.UseWebDavToPaperless();
    await app.RunAsync(cancellationTokenSource.Token);
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

