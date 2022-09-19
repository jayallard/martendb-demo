using DomainModel;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Weasel.Postgresql.Tables;

namespace ProjectionsWebApi.Projections;

public class PersonTableProjectAggregation : EventProjection
{

    public PersonTableProjectAggregation()
    {
        var table = new Table("people_stuff");
        table.AddColumn<Guid>("id").AsPrimaryKey();
        table.AddColumn<string>("first_name").NotNull();
        table.AddColumn<string>("last_name").NotNull();
        table.AddColumn<bool>("is_married").NotNull();
        table.AddColumn<string>("spouse_name").AllowNulls();
        
        // Telling Marten to delete the table data as the 
        // first step in rebuilding this projection
        Options.DeleteDataInTableOnTeardown(table.Identifier);
        SchemaObjects.Add(table);
    }

    public void Project(IEvent<PersonCreatedEvent> evt, IDocumentOperations ops)
    {
        const string sql = "insert into people_stuff(id, first_name, last_name, is_married) values(?, ?, ?, ?)";
        ops.QueueSqlCommand(sql, evt.StreamId, evt.Data.FirstName, evt.Data.LastName, false);
    }
    
    public void Project(IEvent<GotMarriedEvent> evt, IDocumentOperations ops)
    {
        const string sql = "update people_stuff set is_married=true, spouse_name=? where id=?";
        ops.QueueSqlCommand(sql, evt.Data.SpouseName, evt.StreamId);
    }

    public void Project(IEvent<GotDivorcedEvent> evt, IDocumentOperations ops)
    {
        const string sql = "update people_stuff set is_married=false, spouse_name=null where id=?";
        ops.QueueSqlCommand(sql, evt.StreamId);
    }
}