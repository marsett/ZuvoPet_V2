using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using ZuvoPet_V2.Data;
using ZuvoPet_V2.Filters;
using ZuvoPet_V2.Helpers;
using ZuvoPet_V2.Models;
using ZuvoPet_V2.Repositories;

namespace ZuvoPet_V2.Controllers
{
    public class ManagedController : Controller
    {
        private readonly ZuvoPet_V2Context context;
        private readonly IRepositoryZuvoPet_V2 repo;
        public ManagedController(ZuvoPet_V2Context context, IRepositoryZuvoPet_V2 repo)
        {
            this.context = context;
            this.repo = repo;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Usuario registro)
        {
            // Verificar si el usuario o email ya existe
            if (repo.UserExistsAsync(registro.NombreUsuario, registro.Email).Result)
            {
                ModelState.AddModelError("", "El nombre de usuario o el correo ya están en uso.");
                return View(registro);
            }
            // Guardar datos del usuario en sesión
            HttpContext.Session.SetString("RegistroUsuario", JsonSerializer.Serialize(registro));

            // Redirigir según tipo de usuario
            if (registro.TipoUsuario == "Adoptante")
            {
                return RedirectToAction("FormularioAdoptante");
            }
            else if (registro.TipoUsuario == "Refugio")
            {
                return RedirectToAction("FormularioRefugio");
            }

            return RedirectToAction("Login");
        }

        public IActionResult FormularioAdoptante()
        {
            // Verificar si hay datos de registro en sesión
            if (!HttpContext.Session.Keys.Contains("RegistroUsuario"))
            {
                return RedirectToAction("Register");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FormularioAdoptante(Adoptante adoptante)
        {
            if (!HttpContext.Session.Keys.Contains("RegistroUsuario"))
            {
                return RedirectToAction("Register");
            }

            // Recuperar datos del usuario
            var registroJson = HttpContext.Session.GetString("RegistroUsuario");
            var registro = JsonSerializer.Deserialize<Usuario>(registroJson);

            // Iniciar transacción
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Paso 1: Crear usuario
                    int? userId = await this.repo.RegisterUserAsync(
                        registro.NombreUsuario,
                        registro.Email,
                        registro.ContrasenaLimpia,
                        registro.TipoUsuario);

                    if (userId == null)
                    {
                        throw new Exception("No se pudo crear el usuario");
                    }

                    // Paso 2: Crear perfil
                    string nombreAvatar = HelperAvatarDinamico.CrearYGuardarAvatar(registro.NombreUsuario);
                    await this.repo.RegisterPerfilUserAsync(userId.Value, nombreAvatar);

                    // Paso 3: Crear adoptante
                    await this.repo.RegisterAdoptanteAsync(
                        userId.Value,
                        adoptante.Nombre,
                        adoptante.TipoVivienda,
                        adoptante.TieneJardin,
                        adoptante.OtrosAnimales,
                        adoptante.RecursosDisponibles,
                        adoptante.TiempoEnCasa);

                    // Confirmar la transacción
                    await transaction.CommitAsync();

                    // Limpiar datos de sesión
                    HttpContext.Session.Remove("RegistroUsuario");

                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    // Revertir la transacción
                    await transaction.RollbackAsync();

                    // Agregar error
                    ModelState.AddModelError("", "Error al completar el registro: " + ex.Message);
                    return View(adoptante);
                }
            }

        }

        public IActionResult FormularioRefugio()
        {
            // Verificar si hay datos de registro en sesión
            if (!HttpContext.Session.Keys.Contains("RegistroUsuario"))
            {
                return RedirectToAction("Register");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FormularioRefugio(Refugio refugio)
        {
            // Verificar si hay datos de registro en sesión
            if (!HttpContext.Session.Keys.Contains("RegistroUsuario"))
            {
                return RedirectToAction("Register");
            }

            // Recuperar datos del usuario
            var registroJson = HttpContext.Session.GetString("RegistroUsuario");
            var registro = JsonSerializer.Deserialize<Usuario>(registroJson);

            // Procesar coordenadas
            if (!string.IsNullOrEmpty(refugio.LatitudStr) && !string.IsNullOrEmpty(refugio.LongitudStr))
            {
                if (double.TryParse(refugio.LatitudStr,
                                System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out double lat))
                {
                    refugio.Latitud = lat;
                }
                if (double.TryParse(refugio.LongitudStr,
                                System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out double lng))
                {
                    refugio.Longitud = lng;
                }
            }

            // Iniciar transacción
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Paso 1: Crear usuario
                    int? userId = await this.repo.RegisterUserAsync(
                        registro.NombreUsuario,
                        registro.Email,
                        registro.ContrasenaLimpia,
                        registro.TipoUsuario);

                    if (userId == null)
                    {
                        throw new Exception("No se pudo crear el usuario");
                    }

                    // Paso 2: Crear perfil
                    string nombreAvatar = HelperAvatarDinamico.CrearYGuardarAvatar(registro.NombreUsuario);
                    await this.repo.RegisterPerfilUserAsync(userId.Value, nombreAvatar);

                    // Paso 3: Crear refugio
                    await this.repo.RegisterRefugioAsync(
                        userId.Value,
                        refugio.NombreRefugio,
                        refugio.ContactoRefugio,
                        refugio.CantidadAnimales,
                        refugio.CapacidadMaxima,
                        refugio.Latitud,
                        refugio.Longitud);

                    // Confirmar la transacción
                    await transaction.CommitAsync();

                    // Limpiar datos de sesión
                    HttpContext.Session.Remove("RegistroUsuario");

                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    // Revertir la transacción
                    await transaction.RollbackAsync();

                    // Agregar error
                    ModelState.AddModelError("", "Error al completar el registro: " + ex.Message);
                    return View(refugio);
                }
            }
        }
        
        public IActionResult Login()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Clear session if we're at login page but there's a session
            // This helps prevent redirect loops
            if (!User.Identity.IsAuthenticated && HttpContext.Session.GetString("TIPOUSUARIO") != null)
            {
                HttpContext.Session.Clear();
            }

            if (User.Identity.IsAuthenticated)
            {
                string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (userRole == "Adoptante")
                {
                    return RedirectToAction("Index", "Adoptante");
                }
                else if (userRole == "Refugio")
                {
                    return RedirectToAction("Index", "Refugio");
                }
            }

            var tipoUsuario = HttpContext.Session.GetString("TIPOUSUARIO");
            // El resto de tu código de Login
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string nombreusuario, string contrasena)
        {
            Usuario usuario = await this.repo.LogInUserAsync(nombreusuario, contrasena);
            if (usuario == null)
            {
                ViewData["MENSAJE"] = "Credenciales incorrectas";
                return View();
            }

            string fotoPerfil = await this.repo.GetFotoPerfilAsync(usuario.Id);

            // Crear la identidad de Claims
            ClaimsIdentity identity = new ClaimsIdentity(
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name, ClaimTypes.Role);

            // Almacenar el ID
            Claim claimId = new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString());
            identity.AddClaim(claimId);

            // Agregar el tipo de usuario como Role
            Claim claimRole = new Claim(ClaimTypes.Role, usuario.TipoUsuario);
            identity.AddClaim(claimRole);

            // Agregar la foto de perfil como Claim
            Claim claimFoto = new Claim("FotoPerfil", fotoPerfil);
            identity.AddClaim(claimFoto);

            // Si es un tipo específico de usuario, agregar claims adicionales
            if (usuario.TipoUsuario == "Adoptante")
            {
                // Aquí puedes agregar Claims específicos para adoptantes
                // Por ejemplo, si tienes información adicional del adoptante
                Claim claimTipoAdoptante = new Claim("TipoAdoptante", "Regular");
                identity.AddClaim(claimTipoAdoptante);
            }
            else if (usuario.TipoUsuario == "Refugio")
            {
                // Claims específicos para refugios
                Claim claimTipoRefugio = new Claim("TipoRefugio", "Verificado");
                identity.AddClaim(claimTipoRefugio);
            }

            // Crear el principal y realizar el login
            ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userPrincipal);

            // Mantener la sesión como lo tenías antes
            HttpContext.Session.SetInt32("USUARIOID", usuario.Id);
            HttpContext.Session.SetString("USUARIO", nombreusuario);
            HttpContext.Session.SetString("TIPOUSUARIO", usuario.TipoUsuario);
            HttpContext.Session.SetString("AVATAR", fotoPerfil);
            ViewData["TIPOUSUARIO"] = usuario.TipoUsuario;

            // Redirigir según el tipo de usuario
            if (usuario.TipoUsuario == "Adoptante")
            {
                return RedirectToAction("Index", "Adoptante");
            }
            else if (usuario.TipoUsuario == "Refugio")
            {
                return RedirectToAction("Index", "Refugio");
            }
            else
            {
                return View(usuario);
            }
        }

        public IActionResult Denied()
        {
            return View();
        }
        public async Task<IActionResult> Landing()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            // Si ya está autenticado, redirigir
            if (User.Identity.IsAuthenticated)
            {
                string userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                if (userRole == "Adoptante")
                {
                    return RedirectToAction("Index", "Adoptante");
                }
                else if (userRole == "Refugio")
                {
                    return RedirectToAction("Index", "Refugio");
                }
            }
            List<HistoriaExito> historiasExito = await this.repo.ObtenerHistoriasExitoAsync();
            List<HistoriaExito> historiasLimitadas = historiasExito.OrderBy(historias => historias.FechaPublicacion).Take(3).ToList();

            return View(historiasLimitadas);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync
                (CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("USUARIO");
            HttpContext.Session.Remove("TIPOUSUARIO");

            return RedirectToAction("Landing", "Managed");
        }
    }
}
