using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ZuvoPet_V2.Filters
{
    public class AjaxAuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Verificar si el usuario está autenticado utilizando la sesión
                string tipoUsuario = filterContext.HttpContext.Session.GetString("TIPOUSUARIO");
                if (string.IsNullOrEmpty(tipoUsuario))
                {
                    filterContext.Result = new JsonResult(new
                    {
                        sessionExpired = true,
                        redirectUrl = "/Managed/Login" // Ajusta esta ruta según tu configuración
                    });
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
