using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using Prometheus;
using Serilog;
using Serilog.Events;
using WebDavToPaperlessNGX.Middlewares;
using WebDavToPaperlessNGX.Options;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .CreateLogger();

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

    var builder = WebApplication.CreateBuilder();
    builder.Services.Configure<WebDavToPaperlessOptions>(builder.Configuration.GetSection(WebDavToPaperlessOptions.SectionName));
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

