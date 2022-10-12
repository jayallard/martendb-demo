using Marten;
using Marten.Events;
using Marten.Events.Projections;

namespace Demo.Tests;

public class OtherDbProjection : IProjection
{
    // hackis extremis
    // this is just for a quick test to make sure the class is called as many times as we need, and that
    // everything commits even though it's not actually doing anything.
    // a real test would not rely on such nonsense.
    public static int StreamCount;
    public static int EventCount;
    
    
    public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams)
    {
        Interlocked.Add(ref StreamCount, streams.Count);
        var count = streams.Sum(s => s.Events.Count);
        Interlocked.Add(ref EventCount, count);
    }
    
    public Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<StreamAction> streams, CancellationToken cancellation)
    {
        Interlocked.Add(ref StreamCount, streams.Count);

        var count = streams.Sum(s => s.Events.Count);
        Interlocked.Add(ref EventCount, count);
        return Task.CompletedTask;
    }
}

public class Junk
{
    public Guid Id { get; set; }
}