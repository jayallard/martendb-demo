using System.Text.Json.Serialization;
using Marten;
using ProjectionsWebApi.Projections;
using static Marten.Events.Daemon.Resiliency.DaemonMode;
using static Marten.Events.Projections.ProjectionLifecycle;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMarten(options =>
    {
        // options.Connection(builder.Configuration.GetSection("MartenDb:ConnectionString").Value);
        options.Projections.Add<PersonProjectionAggregation>(Async);
        options.Projections.Add<PersonTableProjectAggregation>(Async);
        options.MultiTenantedDatabases(x =>
        {
            x.AddSingleTenantDatabase(builder.Configuration.GetSection("MartenDb:ConnectionString-1").Value,
                "tenant-1");
            x.AddSingleTenantDatabase(builder.Configuration.GetSection("MartenDb:ConnectionString-2").Value,
                "tenant-2");
        });
    })
    .AddAsyncDaemon(HotCold);


builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();