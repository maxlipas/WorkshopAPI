namespace RestAPI.Models.Abstractions;

public abstract class AbstractionService
{
    protected const int PageSize = 10;

    public async Task<Result> DoAction(Func<Task> action)
    {
        try
        {
            await action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.InnerException?.Message ?? ex.Message);
        }
    }
}
