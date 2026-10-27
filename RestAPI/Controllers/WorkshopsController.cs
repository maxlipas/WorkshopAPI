using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RestAPI.Models;
using RestAPI.Models.Services;

namespace RestAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkshopsController : ControllerBase
{
    private const string CacheKey = "workshops";
    private readonly WorkshopService service;
    private readonly IMemoryCache cache;
    private readonly ILogger<WorkshopsController> logger;

    public WorkshopsController(WorkshopService service, IMemoryCache cache, ILogger<WorkshopsController> logger)
    {
        this.service = service;
        this.cache = cache;
        this.logger = logger;
    }

    [HttpGet]
    [Produces(typeof(Workshop[]))]
    public async Task<ActionResult<IEnumerable<Workshop>>> GetPage([FromQuery] int? page)
    {
        if (!cache.TryGetValue(CacheKey, out IEnumerable<Workshop>? cached) || page is not null)
        {
            cached = page is null ? await service.GetAll() : await service.GetPage(page);
            cache.Set(CacheKey, cached, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(5),
                Size = cached.Count()
            });
        }

        LogCache();
        return Ok(cached ?? Enumerable.Empty<Workshop>());
    }

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
    public async Task<ActionResult<Workshop>> GetById(int id)
    {
        var item = await service.Get(id);
        return item is null ? NotFound(new { message = "Цех не найден" }) : Ok(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Workshop>> Create([FromBody] Workshop workshop)
    {
        var result = await service.Create(workshop);
        if (!result.Ok)
            return this.FromResult(result);
        cache.Remove(CacheKey);
        return CreatedAtAction(nameof(GetById), new { id = workshop.Id }, workshop);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Workshop>> Update(int id, [FromBody] Workshop workshop)
    {
        var result = await service.Update(id, workshop);
        if (!result.Ok)
            return this.FromResult(result);
        cache.Remove(CacheKey);
        workshop.Id = id;
        return Ok(workshop);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await service.Delete(id);
        if (result.Ok)
            cache.Remove(CacheKey);
        return this.FromResult(result);
    }

    private void LogCache()
    {
        var stats = cache.GetCurrentStatistics();
        logger.LogInformation("Memory cache. Total hits: {Hits}. Estimated size: {Size}.", stats?.TotalHits, stats?.CurrentEstimatedSize);
    }
}
