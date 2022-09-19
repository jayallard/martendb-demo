using DomainModel;
using Marten.Events.Aggregation;
using Marten.Schema;

namespace ProjectionsWebApi.Projections;

public class PersonProjectionAggregation : SingleStreamAggregation<PersonProjection>
{
    public PersonProjectionAggregation()
    {
        ProjectionName = "People";
    }

    public void Apply(GotMarriedEvent evt, PersonProjection proj)
    {
        proj.Spouse = evt.SpouseName;
    }

    public void Apply(GotDivorcedEvent evt, PersonProjection proj)
    {
        proj.Spouse = null;
    }

    public void Apply(BirthdaySetEvent evt, PersonProjection proj)
    {
        proj.Birthday = evt.Birthday;
    }

    public PersonProjection Create(PersonCreatedEvent evt) =>
        new (evt.PersonId, evt.FirstName, evt.LastName);
}

public class PersonProjection
{
    public PersonProjection(Guid personId, string firstName, string lastName)
    {
        PersonId = personId;
        FirstName = firstName;
        LastName = lastName;
    }

    [Identity]
    public Guid PersonId { get; set; }
    public bool IsMarried { get; set; }
    public DateTime? Birthday { get; set; } = null!;
    public string FirstName { get; }
    public string LastName { get; }
    public string? Spouse { get; set; }
}