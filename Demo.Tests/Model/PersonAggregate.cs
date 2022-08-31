namespace Demo.Tests.Model;

public class PersonAggregate
{
    // NOTE: this is a quickie.
    // usually, the methods perform validations and create the event ONLY. Then, an event handler
    // method (such as Apply) makes the actual change. That'll be important when events are
    // loaded back into an aggregate - coming soon
    // also not shown: domain services
    
    private readonly List<IEvent> _events = new();
    public IReadOnlyList<IEvent> Events => _events.AsReadOnly();
    
    public Marriage? Marriage { get; private set; }
    public Guid PersonId { get; } = Guid.NewGuid();
    public DateTime? Birthday { get; private set; }

    public PersonAggregate(string firstName, string lastName)
    {
        _events.Add(new PersonCreatedEvent(PersonId, "Santa", "Claus"));
    }

    public PersonAggregate SetBirthday(DateTime birthday)
    {
        if (birthday > DateTime.Now) throw new InvalidOperationException("Invalid birthday.");
        Birthday = birthday;
        _events.Add(new BirthdaySetEvent(PersonId, birthday));
        return this;
    }

    public PersonAggregate GotMarried(DateTime marriedDate, string spouseName)
    {
        if (Marriage != null) throw new InvalidOperationException("already married");
        if (marriedDate > DateTime.Now) throw new InvalidOperationException("invalid marriage date");
        Marriage = new Marriage(marriedDate, spouseName);
        _events.Add(new GotMarriedEvent(PersonId, marriedDate, spouseName));
        return this;
    }

    public PersonAggregate GotDivorced()
    {
        if (Marriage == null) throw new InvalidOperationException("not married");
        Marriage = null;
        _events.Add(new GotDivorcedEvent(PersonId));
        return this;
    }
}