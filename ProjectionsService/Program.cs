using Marten;
using ProjectionsService;
using ProjectionsService.Projections;
using static Marten.Events.Daemon.Resiliency.DaemonMode;
using static Marten.Events.Projections.ProjectionLifecycle;

var host = await Host
    .CreateDefaultBuilder(args)
    .ConfigureServices((c, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddMarten(options =>
            {
                options.Connection(c.Configuration.GetSection("MartenDb:ConnectionString").Value);
                options.Projections.Add<PersonProjectionAggregation>(Async);
                options.Projections.Add<PersonTableProjectAggregation>(Async);
            })
            .AddAsyncDaemon(HotCold);
    })
    .StartAsync();
