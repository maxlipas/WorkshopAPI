using Microsoft.EntityFrameworkCore;
using RestAPI.Models.Abstractions;
using RestAPI.Models.Data;

namespace RestAPI.Models.Services;

public class WorkshopService : AbstractionService, ICommonService<Workshop, int>
{
    private readonly AppDbContext db;

    public WorkshopService(AppDbContext db)
    {
        this.db = db;
    }

    public Task<Result> Create(Workshop model)
    {
        model.Id = 0;
        return DoAction(async () =>
        {
            db.Workshops.Add(model);
            await db.SaveChangesAsync();
        });
    }

    public async Task<Result> Delete(int id)
    {
        var workshop = await db.Workshops.Include(w => w.Workers).FirstOrDefaultAsync(w => w.Id == id);
        if (workshop is null)
            return Result.NotFound("Цех не найден");
        if (workshop.Workers.Count > 0)
            return Result.Conflict("Нельзя удалить цех, в котором работают сотрудники");

        return await DoAction(async () =>
        {
            db.Workshops.Remove(workshop);
            await db.SaveChangesAsync();
        });
    }

    public Task<Workshop?> Get(int id) =>
        db.Workshops.FirstOrDefaultAsync(w => w.Id == id);

    public async Task<IEnumerable<Workshop>> GetAll() =>
        await db.Workshops.OrderBy(w => w.Id).ToListAsync();

    public async Task<IEnumerable<Workshop>> GetPage(int? page) =>
        await db.Workshops.OrderBy(w => w.Id)
            .Skip(((page ?? 1) - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

    public async Task<Result> Update(int id, Workshop model)
    {
        var workshop = await db.Workshops.FirstOrDefaultAsync(w => w.Id == id);
        if (workshop is null)
            return Result.NotFound("Цех не найден");

        return await DoAction(async () =>
        {
            workshop.Name = model.Name;
            workshop.HeadFullName = model.HeadFullName;
            await db.SaveChangesAsync();
        });
    }
}
