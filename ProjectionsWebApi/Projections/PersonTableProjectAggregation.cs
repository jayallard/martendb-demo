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
        table.AddColumn<DateTime>("insert_date");
        table.AddColumn<DateTime>("update_date");
        
        // Telling Marten to delete the table data as the 
        // first step in rebuilding this projection
        Options.DeleteDataInTableOnTeardown(table.Identifier);
        SchemaObjects.Add(table);
    }

    public void Project(IEvent<PersonCreatedEvent> evt, IDocumentOperations ops)
    {
        const string sql = "insert into people_stuff(id, first_name, last_name, is_married, insert_date, update_date) values(?, ?, ?, ?, current_timestamp, current_timestamp)";
        ops.QueueSqlCommand(sql, evt.StreamId, evt.Data.FirstName, evt.Data.LastName, false);
    }
    
    public void Project(IEvent<GotMarriedEvent> evt, IDocumentOperations ops)
    {
        const string sql = "update people_stuff set is_married=true, spouse_name=?, update_date=current_timestamp where id=?";
        ops.QueueSqlCommand(sql, evt.Data.SpouseName, evt.StreamId);
    }

    public void Project(IEvent<GotDivorcedEvent> evt, IDocumentOperations ops)
    {
        const string sql = "update people_stuff set is_married=false, spouse_name=null, update_date=current_timestamp where id=?";
        ops.QueueSqlCommand(sql, evt.StreamId);
    }
}