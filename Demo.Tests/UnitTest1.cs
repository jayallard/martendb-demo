using System.Diagnostics;
using Demo.Tests.Model;
using DomainModel;
using FluentAssertions;
using FluentAssertions.Execution;
using Marten;
using Xunit.Abstractions;

namespace Demo.Tests;

public class UnitTest1
{
    private readonly Random _random = new();
    private readonly IDocumentStore _sessionFactory;
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(IDocumentStore sessionFactory, ITestOutputHelper testOutputHelper)
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
        var tenant = "tenant-2";
        const int count = 10_000;
        for (var i = 0; i < count; i++)
        {
            tenant = tenant == "tenant-2" ? "tenant-1" : "tenant-2";
            
            // the first iteration is slow, so ignore it
            if (i == 1) watch.Restart();
            
            // create and save a person
            var santa = new PersonAggregate("Santa", "Claus");
            santa.SetBirthday(new DateTime(1993, 12, 25));
            santa.GotMarried(new DateTime(2020, 12, 24), "Gertrude Claus");

            await using (var create = _sessionFactory.OpenSession(tenant))
            {
                create.Events.StartStream<PersonAggregate>(santa.Id, santa.Events);
                await create.SaveChangesAsync();
            }

            await using (var reader = _sessionFactory.QuerySession(tenant))
            {
                var x = await reader.Events.AggregateStreamAsync<PersonAggregate>(santa.Id);
                using var _ = new AssertionScope();
                x!.FirstName.Should().Be("Santa");
                x.LastName.Should().Be("Claus");
                x.Marriage!.SpouseName.Should().Be("Gertrude Claus");
                x.Marriage.Date.Should().Be(new DateTime(2020, 12, 24));
            }
        }
        
        watch.Stop();
        _testOutputHelper.WriteLine(watch.ElapsedMilliseconds.ToString());
        _testOutputHelper.WriteLine(count / watch.Elapsed.TotalSeconds + " per second");
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
                    create.Events.StartStream<PersonAggregate>(santa.Id, santa.Events);
                    await create.SaveChangesAsync();
                }


            });

        await Task.WhenAll(tasks);
    }
}
