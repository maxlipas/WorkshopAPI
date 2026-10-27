using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Abstractions;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services;

public class WorkwearService : AbstractionService, ICommonService<Workwear, int>
{
    private readonly AppDbContext db;

    public WorkwearService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<Result> Create(Workwear model)
    {
        var check = Validate(model);
        if (!check.Ok)
            return check;

        model.Id = 0;
        return await DoAction(async () =>
        {
            db.Workwears.Add(model);
            await db.SaveChangesAsync();
        });
    }

    public async Task<Result> Delete(int id)
    {
        var item = await db.Workwears.Include(w => w.Receipts).FirstOrDefaultAsync(w => w.Id == id);
        if (item is null)
            return Result.NotFound("Вид спецодежды не найден");
        if (item.Receipts.Count > 0)
            return Result.Conflict("Нельзя удалить спецодежду, которая уже выдавалась");

        return await DoAction(async () =>
        {
            db.Workwears.Remove(item);
            await db.SaveChangesAsync();
        });
    }

    public Task<Workwear?> Get(int id) =>
        db.Workwears.FirstOrDefaultAsync(w => w.Id == id);

    public async Task<IEnumerable<Workwear>> GetAll() =>
        await db.Workwears.OrderBy(w => w.Id).ToListAsync();

    public async Task<IEnumerable<Workwear>> GetPage(int? page) =>
        await db.Workwears.OrderBy(w => w.Id)
            .Skip(((page ?? 1) - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

    public async Task<Result> Update(int id, Workwear model)
    {
        var item = await db.Workwears.FirstOrDefaultAsync(w => w.Id == id);
        if (item is null)
            return Result.NotFound("Вид спецодежды не найден");

        var check = Validate(model);
        if (!check.Ok)
            return check;

        return await DoAction(async () =>
        {
            item.Type = model.Type;
            item.WearMonths = model.WearMonths;
            item.UnitCost = model.UnitCost;
            await db.SaveChangesAsync();
        });
    }

    private static Result Validate(Workwear model)
    {
        if (string.IsNullOrWhiteSpace(model.Type))
            return Result.Fail("Вид спецодежды обязателен");
        if (model.WearMonths <= 0)
            return Result.Fail("Срок носки должен быть больше 0 месяцев");
        if (model.UnitCost < 0)
            return Result.Fail("Стоимость не может быть отрицательной");
        return Result.Success();
    }
}
