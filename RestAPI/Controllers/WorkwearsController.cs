using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RestAPI.Models;
using RestAPI.Models.Services;

namespace RestAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkwearsController : ControllerBase
{
    private const string CacheKey = "workwears";
    private readonly WorkwearService service;
    private readonly IMemoryCache cache;

    public WorkwearsController(WorkwearService service, IMemoryCache cache)
    {
        this.service = service;
        this.cache = cache;
    }

    [HttpGet]
    [Produces(typeof(Workwear[]))]
    public async Task<ActionResult<IEnumerable<Workwear>>> GetPage([FromQuery] int? page)
    {
        if (!cache.TryGetValue(CacheKey, out IEnumerable<Workwear>? cached) || page is not null)
        {
            cached = page is null ? await service.GetAll() : await service.GetPage(page);
            cache.Set(CacheKey, cached, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(5),
                Size = cached.Count()
            });
        }
        return Ok(cached ?? Enumerable.Empty<Workwear>());
    }

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
    public async Task<ActionResult<Workwear>> GetById(int id)
    {
        var item = await service.Get(id);
        return item is null ? NotFound(new { message = "Вид спецодежды не найден" }) : Ok(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Workwear>> Create([FromBody] Workwear workwear)
    {
        var result = await service.Create(workwear);
        if (!result.Ok)
            return this.FromResult(result);
        cache.Remove(CacheKey);
        return CreatedAtAction(nameof(GetById), new { id = workwear.Id }, workwear);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Workwear>> Update(int id, [FromBody] Workwear workwear)
    {
        var result = await service.Update(id, workwear);
        if (!result.Ok)
            return this.FromResult(result);
        cache.Remove(CacheKey);
        workwear.Id = id;
        return Ok(workwear);
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
