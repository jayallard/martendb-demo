using Marten.Events.Projections.Flattened;

namespace Demo.Tests.Projections;

public class PersonTableProjectAggregation : FlatTableProjection
{
    public PersonTableProjectAggregation(): base("PeopleFlat", SchemaNameSource.EventSchema)
    {
        Table.AddColumn<Guid>("PersonId").AsPrimaryKey();
        Project<PersonCreatedEvent>(m =>
        {
            m.Map(x => x.FirstName, "first").NotNull();
            m.Map(x => x.LastName, "last").NotNull();
        });
        
        Project<GotMarriedEvent>(m =>
        {
            m.SetValue("IsMarried", 1);
            // m.Map(x => x.SpouseName, "SpouseName");
        });

        Project<GotDivorcedEvent>(m =>
        {
            m.SetValue("IsMarried", 0);
            // m.SetValue("SpouseName", null);
        });

        Project<BirthdaySetEvent>(m =>
        {
            m.Map(x => x.Birthday);
        });
    }
}