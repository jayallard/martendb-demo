using Demo.Tests.Model;
using Marten;
using Xunit.Abstractions;

namespace Demo.Tests;

public class UnitTest1
{
    private readonly ISessionFactory _sessionFactory;
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ISessionFactory sessionFactory, ITestOutputHelper testOutputHelper)
    {
        _sessionFactory = sessionFactory;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Test1()
    {
        // create and save a person
        var santa = new PersonAggregate("Santa", "Claus");
        santa.SetBirthday(new DateTime(1993, 12, 25));
        santa.GotMarried(new DateTime(2020, 12, 24), "Gertrude Claus");

        await using var create = _sessionFactory.OpenSession();
        {
            create.Events.StartStream<PersonAggregate>(santa.PersonId, santa.Events);
            await create.SaveChangesAsync();
        }

        // ReSharper disable once ConvertToUsingDeclaration
        // get the events
        await using (var get = _sessionFactory.QuerySession())
        {
            var events = await get.Events.FetchStreamAsync(santa.PersonId);
            foreach (var evt in events)
            {
                _testOutputHelper.WriteLine(evt.EventTypeName);
            }
        }
    }
}

public record Marriage(DateTime Date, string SpouseName);

public interface IEvent
{
}

public record PersonCreatedEvent(Guid PersonId, string FirstName, string LastName) : IEvent;
public record BirthdaySetEvent(Guid PersonId, DateTime Birthday) : IEvent;
public record GotMarriedEvent(Guid PersonId, DateTime MarriedDate, string SpouseName) : IEvent;
public record GotDivorcedEvent(Guid PersonId): IEvent;