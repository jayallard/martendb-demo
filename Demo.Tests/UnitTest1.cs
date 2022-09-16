using System.Diagnostics;
using Demo.Tests.Model;
using Marten;
using Xunit.Abstractions;

namespace Demo.Tests;

public class UnitTest1
{
    private readonly Random _random = new();
    private readonly ISessionFactory _sessionFactory;
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ISessionFactory sessionFactory, ITestOutputHelper testOutputHelper)
    {
        _sessionFactory = sessionFactory;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task DeleteStream()
    {
        var events = Enumerable.Range(0, 50_000)
            .Select(r =>
            {
                var i = _random.Next(0, 100);
                return new DeleteTest(r, i.ToString());
            })
            .ToArray<object>();

        var streamId = Guid.NewGuid();
        _testOutputHelper.WriteLine(streamId.ToString());
        // ReSharper disable once ConvertToUsingDeclaration
        await using (var get = _sessionFactory.OpenSession())
        {
            get.Events.StartStream<DeleteTest>(streamId, events);
            await get.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task PerfSingle()
    {
        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 1_000; i++)
        {
            // the first iteration is slow, so ignore it
            if (i == 1) watch.Restart();
            
            // create and save a person
            var santa = new PersonAggregate("Santa", "Claus");
            santa.SetBirthday(new DateTime(1993, 12, 25));
            santa.GotMarried(new DateTime(2020, 12, 24), "Gertrude Claus");

            await using var create = _sessionFactory.OpenSession();
            create.Events.StartStream<PersonAggregate>(santa.PersonId, santa.Events);
            await create.SaveChangesAsync();
        }
        
        _testOutputHelper.WriteLine(watch.ElapsedMilliseconds.ToString());
    }

    [Fact]
    public async Task PerfParallel()
    {
        //for (var i = 0; i < 10_000; i++)
        var tasks = Enumerable
            .Range(0, 90)
            .Select(async i =>
            {
                var santa = new PersonAggregate("Santa", "Claus");
                santa.SetBirthday(new DateTime(1993, 12, 25));
                santa.GotMarried(new DateTime(2020, 12, 24), "Gertrude Claus");

                await using (var create = _sessionFactory.OpenSession())
                {
                    create.Events.StartStream<PersonAggregate>(santa.PersonId, santa.Events);
                    await create.SaveChangesAsync();
                }
            });

        await Task.WhenAll(tasks);
    }
}

public record DeleteTest(int Number, string Name);

public record Marriage(DateTime Date, string SpouseName);

public interface IEvent
{
}

public record PersonCreatedEvent(Guid PersonId, string FirstName, string LastName) : IEvent;

public record BirthdaySetEvent(Guid PersonId, DateTime Birthday) : IEvent;

public record GotMarriedEvent(Guid PersonId, DateTime MarriedDate, string SpouseName) : IEvent;

public record GotDivorcedEvent(Guid PersonId) : IEvent;