using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RestAPI.Models;
using RestAPI.Models.Services;

namespace RestAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkersController : ControllerBase
{
    private const string CacheKey = "workers";
    private readonly WorkerService service;
    private readonly IMemoryCache cache;

    public WorkersController(WorkerService service, IMemoryCache cache)
    {
        this.service = service;
        this.cache = cache;
    }

    [HttpGet]
    [Produces(typeof(Worker[]))]
    public async Task<ActionResult<IEnumerable<Worker>>> Get([FromQuery] int? page, [FromQuery] int? workshopId)
    {
        if (workshopId is not null)
            return Ok(await service.GetByWorkshop(workshopId.Value));
        if (page is not null)
            return Ok(await service.GetPage(page));
        if (!cache.TryGetValue(CacheKey, out IEnumerable<Worker>? cached))
        {
            cached = await service.GetAll();
            cache.Set(CacheKey, cached, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(5),
                Size = cached.Count()
            });
        }
        return Ok(cached ?? Enumerable.Empty<Worker>());
    }

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
    public async Task<ActionResult<Worker>> GetById(int id)
    {
        var item = await service.Get(id);
        return item is null ? NotFound(new { message = "Работник не найден" }) : Ok(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Worker>> Create([FromBody] Worker worker)
    {
        var result = await service.Create(worker);
        if (!result.Ok)
            return this.FromResult(result);
        cache.Remove(CacheKey);
        return CreatedAtAction(nameof(GetById), new { id = worker.Id }, worker);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Worker>> Update(int id, [FromBody] Worker worker)
    {
        var result = await service.Update(id, worker);
        if (!result.Ok)
            return this.FromResult(result);
        cache.Remove(CacheKey);
        worker.Id = id;
        return Ok(worker);
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
}
