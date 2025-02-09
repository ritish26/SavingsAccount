using Application.Commands;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;


// This class is used to register the external services 
namespace SavingsAccount;

public class Program
{
    public static void Main(string[] args)
    {
        var logDirectory = CreateFolder();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq("http://localhost:5341") 
            .WriteTo.File($"{logDirectory}/logDirectory.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting web host");
            var hostBuilder = CreateHostBuilder(args);
            hostBuilder.UseNServiceBus(RegisterNServiceBus);
            hostBuilder.Build().Run();
        }

        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }

        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static string CreateFolder()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        
        var parentDirectory = Directory
            .GetParent(Directory.GetParent(Directory.GetParent(currentDirectory)?.FullName).FullName).FullName;
        
        var logDirectory = Path.Combine(parentDirectory, "logs");
        
        Directory.CreateDirectory(logDirectory);
        
        return logDirectory;
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddJsonFile("/Users/ritikdhiman/Desktop/Code/SavingsAccount/WebApi/appsettings.Development.json", optional: false, reloadOnChange: true);
                builder.AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    
    // NServiceBus Configuration
    public static EndpointConfiguration RegisterNServiceBus(HostBuilderContext _)
    { 
        var endpointConfiguration = new EndpointConfiguration("SavingsAccountBus");
        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        
        endpointConfiguration.EnableInstallers();
        
        transport.Routing().RouteToEndpoint(
            assembly: typeof(CreateSavingsAccountCommand).Assembly,
            destination: "ApplicationHandlers");
        
        endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
        var recoverability = endpointConfiguration.Recoverability();
        
        recoverability.Immediate(
            immediate => immediate.NumberOfRetries(0));

        return endpointConfiguration;
    }
    
}