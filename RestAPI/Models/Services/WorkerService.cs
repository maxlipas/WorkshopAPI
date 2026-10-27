using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Abstractions;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services;

public class WorkerService : AbstractionService, ICommonService<Worker, int>
{
    private readonly AppDbContext db;

    public WorkerService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<Result> Create(Worker model)
    {
        var check = await Validate(model);
        if (!check.Ok)
            return check;

        model.Id = 0;
        return await DoAction(async () =>
        {
            db.Workers.Add(model);
            await db.SaveChangesAsync();
        });
    }

    public async Task<Result> Delete(int id)
    {
        var worker = await db.Workers.Include(w => w.Receipts).FirstOrDefaultAsync(w => w.Id == id);
        if (worker is null)
            return Result.NotFound("Работник не найден");
        if (worker.Receipts.Count > 0)
            return Result.Conflict("Нельзя удалить работника, у которого есть получения спецодежды");

        var workersInShop = await db.Workers.CountAsync(w => w.WorkshopId == worker.WorkshopId);
        if (workersInShop <= 1)
            return Result.Conflict("В цехе должен оставаться хотя бы один работник");

        return await DoAction(async () =>
        {
            db.Workers.Remove(worker);
            await db.SaveChangesAsync();
        });
    }

    public Task<Worker?> Get(int id) =>
        db.Workers.FirstOrDefaultAsync(w => w.Id == id);

    public async Task<IEnumerable<Worker>> GetAll() =>
        await db.Workers.OrderBy(w => w.Id).ToListAsync();

    public async Task<IEnumerable<Worker>> GetPage(int? page) =>
        await db.Workers.OrderBy(w => w.Id)
            .Skip(((page ?? 1) - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

    public async Task<IEnumerable<Worker>> GetByWorkshop(int workshopId) =>
        await db.Workers.Where(w => w.WorkshopId == workshopId).OrderBy(w => w.Id).ToListAsync();

    public async Task<Result> Update(int id, Worker model)
    {
        var worker = await db.Workers.FirstOrDefaultAsync(w => w.Id == id);
        if (worker is null)
            return Result.NotFound("Работник не найден");

        var check = await Validate(model);
        if (!check.Ok)
            return check;

        if (worker.WorkshopId != model.WorkshopId)
        {
            var workersInOldShop = await db.Workers.CountAsync(w => w.WorkshopId == worker.WorkshopId);
            if (workersInOldShop <= 1)
                return Result.Conflict("Нельзя перевести последнего работника из цеха");
        }

        return await DoAction(async () =>
        {
            worker.FullName = model.FullName;
            worker.Position = model.Position;
            worker.DiscountPercent = model.DiscountPercent;
            worker.WorkshopId = model.WorkshopId;
            await db.SaveChangesAsync();
        });
    }

    private async Task<Result> Validate(Worker model)
    {
        if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Position))
            return Result.Fail("ФИО и должность обязательны");
        if (model.DiscountPercent < 0 || model.DiscountPercent > 100)
            return Result.Fail("Скидка должна быть от 0 до 100%");
        if (!await db.Workshops.AnyAsync(w => w.Id == model.WorkshopId))
            return Result.Fail("Работник должен быть привязан к существующему цеху");
        return Result.Success();
    }
}
