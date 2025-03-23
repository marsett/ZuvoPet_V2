using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ZuvoPet_V2.Controllers
{
    public class BaseController : Controller
    {
        //public override void OnActionExecuting(ActionExecutingContext context)
        //{
        //    var tipoUsuario = HttpContext.Session.GetString("TIPOUSUARIO");

        //    if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario != "Adoptante")
        //    {
        //        context.Result = RedirectToAction("Login", "Managed");
        //    }

        //    base.OnActionExecuting(context);
        //}
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Skip this check for Login, Denied, Landing, and other public pages
            if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor actionDescriptor &&
                (actionDescriptor.ControllerName == "Managed" &&
                (actionDescriptor.ActionName == "Login" ||
                 actionDescriptor.ActionName == "Denied" ||
                 actionDescriptor.ActionName == "Landing" ||
                 actionDescriptor.ActionName == "Error")))
            {
                base.OnActionExecuting(context);
                return;
            }

            // If user is not authenticated, let the authentication middleware handle it
            if (!User.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Additional role-specific checks if needed
            var tipoUsuario = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            // Only check session as a fallback
            if (string.IsNullOrEmpty(tipoUsuario))
            {
                tipoUsuario = HttpContext.Session.GetString("TIPOUSUARIO");
                if (string.IsNullOrEmpty(tipoUsuario))
                {
                    context.Result = new RedirectToActionResult("Login", "Managed", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
