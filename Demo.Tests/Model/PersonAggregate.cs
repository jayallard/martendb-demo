using DomainModel;

namespace Demo.Tests.Model;

public abstract class AggregateBase
{
    
    public IReadOnlyList<object> Events => _uncommittedEvents.AsReadOnly();

    private readonly List<object> _uncommittedEvents = new();
    public IEnumerable<object> UncommittedEvents => new List<object>(_uncommittedEvents);

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected void AddUncommittedEvent(object obj) => _uncommittedEvents.Add(obj);
}

public class PersonAggregate : AggregateBase
{
    private PersonAggregate(){}
    public Marriage? Marriage { get; private set; }
    
    public Guid Id { get; private set; }
    public DateTime? Birthday { get; private set; }

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;

    public PersonAggregate(string firstName, string lastName)
    {
        var evt = new PersonCreatedEvent(Guid.NewGuid(), firstName, lastName);
        Apply(evt);
        AddUncommittedEvent(evt);
    }

    public PersonAggregate SetBirthday(DateTime birthday)
    {
        if (birthday > DateTime.Now) throw new InvalidOperationException("Invalid birthday.");

        var evt = new BirthdaySetEvent(Id, birthday);
        Apply(evt);
        AddUncommittedEvent(evt);
        return this;
    }

    public PersonAggregate GotMarried(DateTime marriedDate, string spouseName)
    {
        if (Marriage != null) throw new InvalidOperationException("already married");
        if (marriedDate > DateTime.Now) throw new InvalidOperationException("invalid marriage date");

        var evt = new GotMarriedEvent(Id, marriedDate, spouseName);
        Apply(evt);
        AddUncommittedEvent(evt);
        return this;
    }

    public PersonAggregate GotDivorced()
    {
        if (Marriage == null) throw new InvalidOperationException("not married");
        var evt = new GotDivorcedEvent(Id);
        Apply(evt);
        AddUncommittedEvent(evt);
        return this;
    }

    private void Apply(BirthdaySetEvent evt) => Birthday = evt.Birthday;

    private void Apply(GotMarriedEvent evt) =>
        Marriage = new Marriage(evt.MarriedDate, evt.SpouseName);

    private void Apply(GotDivorcedEvent evt) =>
        Marriage = null;

    private void Apply(PersonCreatedEvent evt)
    {
        Id = evt.PersonId;
        FirstName = evt.FirstName;
        LastName = evt.LastName;
    }
}