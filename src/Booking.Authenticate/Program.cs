using Booking.Authenticate;
using Serilog;
using System.Net;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

Log.Information("Starting up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc.Enrich.FromLogContext().ReadFrom.Configuration(ctx.Configuration));

    builder.WebHost.UseKestrel();

    builder.WebHost.ConfigureKestrel((context, options) =>
    {
        var configuration = context.Configuration;
        options.Listen(IPAddress.Any, configuration.GetValue<int>("Kestrel:Port"));
    });

    var app = builder.ConfigureServices().ConfigurePipeline();

    Log.Information("Start processing seed database...");

    SeedData.EnsureSeedApplicationData(app);

    SeedData.EnsureSeedDuendeIdentityData(app);

    Log.Information("Finished processing seed database.");

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException"
&& ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");

    Log.CloseAndFlush();
}
