using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Abstractions;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services;

public class ReceiptService : AbstractionService, ICommonService<Receipt, int>
{
    private readonly AppDbContext db;

    public ReceiptService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<Result> Create(Receipt model)
    {
        var check = await Validate(model);
        if (!check.Ok)
            return check;

        model.Id = 0;
        return await DoAction(async () =>
        {
            db.Receipts.Add(model);
            await db.SaveChangesAsync();
        });
    }

    public async Task<Result> Delete(int id)
    {
        var receipt = await db.Receipts.FirstOrDefaultAsync(r => r.Id == id);
        if (receipt is null)
            return Result.NotFound("Получение не найдено");

        var receiptsForWear = await db.Receipts.CountAsync(r => r.WorkwearId == receipt.WorkwearId);
        if (receiptsForWear <= 1)
            return Result.Conflict("Каждый вид спецодежды должен участвовать хотя бы в одном получении");

        return await DoAction(async () =>
        {
            db.Receipts.Remove(receipt);
            await db.SaveChangesAsync();
        });
    }

    public Task<Receipt?> Get(int id) =>
        db.Receipts.FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Receipt>> GetAll() =>
        await db.Receipts.OrderBy(r => r.Id).ToListAsync();

    public async Task<IEnumerable<Receipt>> GetPage(int? page) =>
        await db.Receipts.OrderBy(r => r.Id)
            .Skip(((page ?? 1) - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

    public async Task<IEnumerable<Receipt>> Filter(int? workerId, int? workwearId)
    {
        var query = db.Receipts.AsQueryable();
        if (workerId is not null)
            query = query.Where(r => r.WorkerId == workerId);
        if (workwearId is not null)
            query = query.Where(r => r.WorkwearId == workwearId);
        return await query.OrderBy(r => r.Id).ToListAsync();
    }

    public async Task<Result> Update(int id, Receipt model)
    {
        var receipt = await db.Receipts.FirstOrDefaultAsync(r => r.Id == id);
        if (receipt is null)
            return Result.NotFound("Получение не найдено");

        var check = await Validate(model);
        if (!check.Ok)
            return check;

        return await DoAction(async () =>
        {
            receipt.WorkerId = model.WorkerId;
            receipt.WorkwearId = model.WorkwearId;
            receipt.ReceivedAt = model.ReceivedAt;
            receipt.Signed = model.Signed;
            await db.SaveChangesAsync();
        });
    }

    private async Task<Result> Validate(Receipt model)
    {
        if (!await db.Workers.AnyAsync(w => w.Id == model.WorkerId))
            return Result.Fail("В получении должен участвовать существующий работник");
        if (!await db.Workwears.AnyAsync(w => w.Id == model.WorkwearId))
            return Result.Fail("Получение должно относиться к существующему виду спецодежды");
        return Result.Success();
    }
}
