using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CorsoApi.Infrastructure;

public static class ControllerExtensions
{
    public static ActionResult BadRequestProblem(this ControllerBase controller, string category, string message)
    {
        var errorDictionary = new ModelStateDictionary();
        errorDictionary.AddModelError(category, message);
        return controller.ValidationProblem(statusCode: StatusCodes.Status400BadRequest, modelStateDictionary: errorDictionary);
    }

    public static ActionResult BadRequestProblem(this ControllerBase controller, string category, IEnumerable<string> messages)
    {
        var errorDictionary = new ModelStateDictionary();

        foreach(var message in messages)
        {
            errorDictionary.AddModelError(category, message);
        }

        return controller.ValidationProblem(statusCode: StatusCodes.Status400BadRequest, modelStateDictionary: errorDictionary);
    }
}
