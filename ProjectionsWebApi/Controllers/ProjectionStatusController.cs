using Marten;
using Microsoft.AspNetCore.Mvc;

namespace ProjectionsWebApi.Controllers;

[Route("/api/projections")]
[ApiController]
public class ProjectionStatusController : Controller
{
    private readonly IDocumentStore _store;
    public ProjectionStatusController(IDocumentStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Return info about the current state of projections.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var stats = await _store.Advanced.FetchEventStoreStatistics();
        var y = new
        {
            stats.EventSequenceNumber,
            stats.EventCount
        };
        
        var shards = await _store.Advanced.AllProjectionProgress();
        var x = shards.Select(s => new
        {
            s.ShardName,
            Status = s.Action,
            s.Sequence,
            IsCurrent = s.Sequence == y.EventSequenceNumber
        });

        return new JsonResult(new
        {
            Shards = x,
            Stats = y
        });
    }
}