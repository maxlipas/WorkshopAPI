namespace RestAPI.Models;

public record Result(bool Ok, string? Error = null, int Status = 400)
{
    public static Result Success() => new(true, null, 200);
    public static Result Fail(string error, int status = 400) => new(false, error, status);
    public static Result NotFound(string error) => new(false, error, 404);
    public static Result Conflict(string error) => new(false, error, 409);
}
