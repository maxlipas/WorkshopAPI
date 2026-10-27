using Microsoft.AspNetCore.Mvc;
using RestAPI.Models;

namespace RestAPI.Controllers;

internal static class ControllerExtensions
{
    public static ActionResult FromResult(this ControllerBase controller, Result result)
    {
        if (result.Ok)
            return controller.NoContent();
        return controller.StatusCode(result.Status, new { message = result.Error });
    }
}
