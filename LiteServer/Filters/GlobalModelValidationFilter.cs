using LiteServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiteServer.Filters
{
    public class ModelValidationErrorResult : ObjectResult
    {
        public ModelValidationErrorResult(ModelStateDictionary state)
            : base(new InputErrorModel(state))
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity;
        }
    }

    public class GlobalModelValidationFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ModelValidationErrorResult(context.ModelState);
            }
        }
    }
}
