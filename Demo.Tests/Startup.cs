using Demo.Tests.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weasel.Core;

namespace Demo.Tests;

public class Startup
{
    public static void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureAppConfiguration(b =>
        {
            b
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
        });
    }
    
    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        services.AddMarten(options =>
            {
                options.Connection(context.Configuration.GetSection("MartenDb:ConnectionString").Value);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Projections.Add<PersonProjectionAggregation>(ProjectionLifecycle.Inline);
                options.Projections.Add<PersonTableProjectAggregation>(ProjectionLifecycle.Inline);
            })
            .InitializeWith();
    }
}