using Marten;
using Microsoft.AspNetCore.Mvc;

namespace ProjectionsWebApi.Controllers;

[Route("/api/projections")]
[ApiController]
public class ProjectionStatusController : Controller
{
    private readonly IDocumentStore _store;
    private readonly IServiceProvider _services;
    public ProjectionStatusController(IDocumentStore store, IServiceProvider services)
    {
        _store = store;
        _services = services;
    }

    /// <summary>
    /// Return info about the current state of projections.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{tenantId}")]
    public async Task<IActionResult> Get(string tenantId)
    {
        var stats = await _store.Advanced.FetchEventStoreStatistics(tenantId);
        var y = new
        {
            stats.EventSequenceNumber,
            stats.EventCount
        };
        
        var shards = await _store.Advanced.AllProjectionProgress(tenantId);
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