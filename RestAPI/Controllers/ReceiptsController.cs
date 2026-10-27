using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RestAPI.Models;
using RestAPI.Models.Services;

namespace RestAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReceiptsController : ControllerBase
{
    private const string CacheKey = "receipts";
    private readonly ReceiptService service;
    private readonly IMemoryCache cache;

    public ReceiptsController(ReceiptService service, IMemoryCache cache)
    {
        this.service = service;
        this.cache = cache;
    }

    [HttpGet]
    [Produces(typeof(Receipt[]))]
    public async Task<ActionResult<IEnumerable<Receipt>>> Get([FromQuery] int? page, [FromQuery] int? workerId, [FromQuery] int? workwearId)
    {
        if (workerId is not null || workwearId is not null)
            return Ok(await service.Filter(workerId, workwearId));
        if (page is not null)
            return Ok(await service.GetPage(page));
        if (!cache.TryGetValue(CacheKey, out IEnumerable<Receipt>? cached))
        {
            cached = await service.GetAll();
            cache.Set(CacheKey, cached, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(5),
                Size = cached.Count()
            });
        }
        return Ok(cached ?? Enumerable.Empty<Receipt>());
    }

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
    public async Task<ActionResult<Receipt>> GetById(int id)
    {
        var item = await service.Get(id);
        return item is null ? NotFound(new { message = "Получение не найдено" }) : Ok(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Receipt>> Create([FromBody] Receipt receipt)
    {
        var result = await service.Create(receipt);
        if (!result.Ok)
            return this.FromResult(result);
        cache.Remove(CacheKey);
        return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, receipt);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Receipt>> Update(int id, [FromBody] Receipt receipt)
    {
        var result = await service.Update(id, receipt);
        if (!result.Ok)
            return this.FromResult(result);
        cache.Remove(CacheKey);
        receipt.Id = id;
        return Ok(receipt);
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
