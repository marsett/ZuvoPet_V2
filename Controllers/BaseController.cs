using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ZuvoPet_V2.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var tipoUsuario = HttpContext.Session.GetString("TIPOUSUARIO");

            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario != "Adoptante")
            {
                context.Result = RedirectToAction("Login", "Managed");
            }

            base.OnActionExecuting(context);
        }
    }
}
