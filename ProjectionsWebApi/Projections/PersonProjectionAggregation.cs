using Baseline;
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
        proj.UpdateDate = DateTime.Now;
    }

    public void Apply(GotDivorcedEvent evt, PersonProjection proj)
    {
        proj.Spouse = null;
        proj.UpdateDate = DateTime.Now;
    }

    public void Apply(BirthdaySetEvent evt, PersonProjection proj)
    {
        proj.Birthday = evt.Birthday;
        proj.UpdateDate = DateTime.Now;
    }

    public PersonProjection Create(PersonCreatedEvent evt) => new(evt.PersonId, evt.FirstName, evt.LastName)
    {
        UpdateDate = DateTime.Now,
        CreateDate = DateTime.Now
    };
}

public class PersonProjection
{
    public PersonProjection(Guid personId, string firstName, string lastName)
    {
        PersonId = personId;
        FirstName = firstName;
        LastName = lastName;
    }

    [Identity] public Guid PersonId { get; set; }
    public bool IsMarried { get; set; }
    public DateTime? Birthday { get; set; } = null!;
    public string FirstName { get; }
    public string LastName { get; }
    public string? Spouse { get; set; }

    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
}