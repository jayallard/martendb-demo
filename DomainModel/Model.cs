namespace DomainModel;

public record DeleteTest(int Number, string Name);

public record Marriage(DateTime Date, string SpouseName);

public interface IEvent
{
}

public record PersonCreatedEvent(Guid PersonId, string FirstName, string LastName) : IEvent;

public record BirthdaySetEvent(Guid PersonId, DateTime Birthday) : IEvent;

public record GotMarriedEvent(Guid PersonId, DateTime MarriedDate, string SpouseName) : IEvent;

public record GotDivorcedEvent(Guid PersonId) : IEvent;