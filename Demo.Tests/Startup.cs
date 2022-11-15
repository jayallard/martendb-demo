using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectionsWebApi.Projections;
using Weasel.Core;

namespace Demo.Tests;

public class Startup
{
    public static void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureAppConfiguration(b =>
        {
            b
                .AddJsonFile("appsettings.Development.json")
                .AddEnvironmentVariables();
        });
    }
    
    public void ConfigureServices(IServiceCollection services, HostBuilderContext context)
    {
        services.AddMarten(options =>
            {
                // options.Connection(context.Configuration.GetSection("MartenDb:ConnectionString").Value);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.MultiTenantedDatabases(x =>
                {
                    x.AddSingleTenantDatabase(context.Configuration.GetSection("MartenDb:ConnectionString-1").Value,
                        "tenant-1");
                    x.AddSingleTenantDatabase(context.Configuration.GetSection("MartenDb:ConnectionString-2").Value,
                        "tenant-2");
                });

                // no di support - sad face
                options.Projections.Add<PersonProjectionAggregation>(); // requires default constructor - not di
                options.Projections.Add<OtherDbProjection>(); // only supports a particular base class...
                options.Projections.Add(new OtherDbProjection()); // ...so must do this
            })
            .InitializeWith();
    }
}