using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZuvoPet_V2.Filters;
using ZuvoPet_V2.Helpers;
using ZuvoPet_V2.Models;
using ZuvoPet_V2.Repositories;
using ZuvoPet_V2.Data;
using Microsoft.EntityFrameworkCore;

namespace ZuvoPet_V2.Controllers
{
    [AuthorizeZuvoPet_V2("Refugio")]
    public class RefugioController : Controller
    {
        private readonly ZuvoPet_V2Context context;
        private readonly IRepositoryZuvoPet_V2 repo;
        private HelperPathProvider helperPath;

        public RefugioController(ZuvoPet_V2Context context, IRepositoryZuvoPet_V2 repo, HelperPathProvider helperPath)
        {
            this.repo = repo;
            this.helperPath = helperPath;
            this.context = context;
        }

        // Actualización del Controller - Método Index
        public async Task<IActionResult> Index()
        {
            //var tipoUsuario = HttpContext.Session.GetString("TIPOUSUARIO");
            //if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario != "Refugio")
            //{
            //    return RedirectToAction("Login", "Managed");
            //}

            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            var refugio = await this.repo.GetRefugioByUsuarioIdAsync(idusuario);

            if (refugio == null)
            {
                return RedirectToAction("Create");
            }

            // Obtener todas las mascotas de este refugio
            var mascotas = await this.repo.GetMascotasByRefugioIdAsync(refugio.Id);
            refugio.ListaMascotas = mascotas.ToList();

            // Contar las solicitudes pendientes, rechazadas y aprobadas
            int solicitudesPendientes = await this.repo.GetSolicitudesByEstadoAndRefugioAsync(refugio.Id, "Pendiente");
            int solicitudesRechazadas = await this.repo.GetSolicitudesByEstadoAndRefugioAsync(refugio.Id, "Rechazada");

            ViewBag.NuevasSolicitudes = solicitudesPendientes;
            ViewBag.SolicitudesRechazadas = solicitudesRechazadas;

            // Calcular mascotas adoptadas
            int mascotasAdoptadas = await this.repo.GetSolicitudesByEstadoAndRefugioAsync(refugio.Id, "Aprobada");
            ViewBag.MascotasAdoptadas = mascotasAdoptadas;

            // Preparar datos para el gráfico de especies
            var especies = mascotas.GroupBy(m => m.Especie)
                                  .Select(g => new { Especie = g.Key, Cantidad = g.Count() })
                                  .OrderByDescending(g => g.Cantidad)
                                  .ToList();

            ViewBag.EspeciesLabels = especies.Select(e => e.Especie).ToArray();
            ViewBag.EspeciesData = especies.Select(e => e.Cantidad).ToArray();

            return View(refugio);
        }

        public async Task<IActionResult> Gestion(int pagina = 1)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            // Obtiene todas las mascotas del refugio
            List<MascotaCard> mascotas = await this.repo.ObtenerMascotasRefugioAsync(idusuario);

            // Configuración de la paginación
            int mascotasPorPagina = 9; // Puedes ajustar esta cantidad según tus necesidades

            // Calcula el total de páginas
            int totalRegistros = mascotas.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / mascotasPorPagina);

            // Validación de la página actual
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            // Filtra las mascotas para la página actual
            var mascotasPaginadas = mascotas
                .Skip((pagina - 1) * mascotasPorPagina)
                .Take(mascotasPorPagina)
                .ToList();

            // Asigna las variables de paginación al ViewBag
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(mascotasPaginadas);
        }

        public async Task<IActionResult> CrearMascota()
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            Refugio refugio = await this.repo.GetRefugio(idusuario);
            ViewData["LATITUD"] = refugio.Latitud;
            ViewData["LONGITUD"] = refugio.Longitud;
            ViewData["NOMBRE"] = refugio.NombreRefugio;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearMascota(Mascota mascota, IFormFile fichero)
        {
            if (fichero != null)
            {
                string fileName = Guid.NewGuid().ToString() + ".png";
                string path = this.helperPath.MapPath(fileName, Folders.Images);
                using (Stream stream = new FileStream(path, FileMode.Create))
                {
                    await fichero.CopyToAsync(stream);
                }
                mascota.Foto = fileName;
            }

            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            await this.repo.CrearMascotaRefugioAsync(mascota, idusuario);
            return RedirectToAction("Gestion");
        }

        public async Task<IActionResult> EditarMascota(int idmascota)
        {
            // Buscar la mascota por ID
            Mascota mascota = await this.repo.GetMascotaByIdAsync(idmascota);

            if (mascota == null)
            {
                return NotFound(); // Si no existe la mascota, devolver 404
            }

            // Verificar que el usuario logueado es dueño del refugio de esta mascota
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            Refugio refugio = await this.repo.GetRefugio(idusuario);

            if (mascota.IdRefugio != refugio.Id)
            {
                return Forbid(); // Si no es el dueño, denegar acceso
            }

            // Pasar datos adicionales a la vista
            ViewData["LATITUD"] = refugio.Latitud;
            ViewData["LONGITUD"] = refugio.Longitud;
            ViewData["NOMBRE"] = refugio.NombreRefugio;

            return View(mascota);
        }

        // Método POST para procesar el formulario de edición
        [HttpPost]
        public async Task<IActionResult> EditarMascota(Mascota mascota, IFormFile fichero)
        {
            // Verificar si el usuario es dueño del refugio
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            Refugio refugio = await this.repo.GetRefugio(idusuario);

            if (mascota.IdRefugio != refugio.Id)
            {
                return Forbid(); // Si no es el dueño, denegar acceso
            }

            // Procesar la nueva imagen si se proporcionó una
            if (fichero != null)
            {
                string fileName = Guid.NewGuid().ToString() + ".png";
                string path = this.helperPath.MapPath(fileName, Folders.Images);
                using (Stream stream = new FileStream(path, FileMode.Create))
                {
                    await fichero.CopyToAsync(stream);
                }

                // Eliminar la imagen antigua si existe
                if (!string.IsNullOrEmpty(mascota.Foto))
                {
                    string oldImagePath = this.helperPath.MapPath(mascota.Foto, Folders.Images);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                mascota.Foto = fileName;
            }

            // Actualizar la mascota en la base de datos
            await this.repo.UpdateMascotaAsync(mascota);

            return RedirectToAction("Gestion");
        }

        // Método POST para procesar la eliminación
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Refugio/EliminarMascota/{idmascota}")]
        public async Task<IActionResult> EliminarMascota(int idmascota)
        {
            // Buscar la mascota por ID
            Mascota mascota = await this.repo.GetMascotaByIdAsync(idmascota);
            if (mascota == null)
            {
                return NotFound(); // Si no existe la mascota, devolver 404
            }

            // Verificar que el usuario logueado es dueño del refugio de esta mascota
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            Refugio refugio = await this.repo.GetRefugio(idusuario);
            if (mascota.IdRefugio != refugio.Id)
            {
                return Forbid(); // Si no es el dueño, denegar acceso
            }

            // Eliminar la imagen si existe
            if (!string.IsNullOrEmpty(mascota.Foto))
            {
                string imagePath = this.helperPath.MapPath(mascota.Foto, Folders.Images);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Eliminar la mascota y sus relaciones de la base de datos
            bool result = await this.repo.DeleteMascotaAsync(idmascota);

            // Para solicitudes AJAX, devolver un resultado JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = result });
            }

            // Para solicitudes regulares, redirigir a la página de gestión
            return RedirectToAction("Gestion");
        }

        //public async Task<IActionResult> Solicitudes(int pagina = 1)
        //{
        //    int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

        //    List<SolicitudAdopcion> solicitudes =
        //        await this.repo.GetSolicitudesRefugioAsync(idusuario);

        //    return View(solicitudes);
        //}
        public async Task<IActionResult> Solicitudes(int pagina = 1)
        {
            int tamañoPagina = 3; // Número de solicitudes por página
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            List<SolicitudAdopcion> todasSolicitudes =
                await this.repo.GetSolicitudesRefugioAsync(idusuario);

            // Calcular total de páginas
            int totalSolicitudes = todasSolicitudes.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalSolicitudes / tamañoPagina);

            // Ajustar página actual si está fuera de rango
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            // Obtener solicitudes de la página actual
            List<SolicitudAdopcion> solicitudesPaginadas = todasSolicitudes
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToList();

            // Pasar información de paginación a la vista
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(solicitudesPaginadas);
        }

        public async Task<IActionResult> DetallesSolicitud(int idsolicitud)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            SolicitudAdopcion soli =
                await this.repo.GetSolicitudByIdAsync(idusuario, idsolicitud);
            return View(soli);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarSolicitud(int idSolicitud, string nuevoEstado)
        {
            if (HttpContext.Session.GetInt32("USUARIOID") == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            bool resultado = await this.repo.ProcesarSolicitudAdopcionAsync(idSolicitud, nuevoEstado);

            if (resultado)
            {
                // Retornar un resultado JSON para solicitudes AJAX
                return Json(new { success = true, mensaje = $"Solicitud {nuevoEstado.ToLower()} correctamente" });
            }
            else
            {
                return Json(new { success = false, mensaje = "Error al procesar la solicitud" });
            }
        }

        public async Task<IActionResult> DetallesMascota(int idmascota)
        {
            Mascota mascota = await this.repo.GetDetallesMascotaAsync(idmascota);
            return View(mascota);
        }

        public async Task<IActionResult> HistoriasExito()
        {
            List<HistoriaExito> historiasExito = await this.repo.ObtenerHistoriasExitoAsync();


            var historiasConDetalles = new List<HistoriaExitoConDetalles>();

            foreach (var historia in historiasExito)
            {
                // Obtener los comentarios y likes para cada historia
                var comentariosHistoria = await this.repo.ObtenerComentariosHistoriaAsync(historia.Id);
                var likeHistorias = await this.repo.ObtenerLikeHistoriaAsync(historia.Id);



                // Crear un objeto con la historia, comentarios y likes
                var historiaConDetalles = new HistoriaExitoConDetalles
                {
                    HistoriaExito = historia,
                    ComentariosHistoria = comentariosHistoria,
                    LikeHistorias = likeHistorias
                };

                // Añadir el objeto a la lista
                historiasConDetalles.Add(historiaConDetalles);
            }
            return View(historiasConDetalles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReaccionarHistoria(int idHistoria, string tipoReaccion)
        {
            // Verificar que el usuario esté autenticado
            if (!HttpContext.Session.GetInt32("USUARIOID").HasValue)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            // Obtener el ID del usuario de la sesión
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            // Verificar si el usuario ya tiene una reacción para esta historia
            var reaccionExistente = await this.repo.ObtenerLikeUsuarioHistoriaAsync(idHistoria, idusuario);
            bool resultado;
            string accion;

            // Si la reacción existente es del mismo tipo, eliminarla (toggle)
            if (reaccionExistente != null && reaccionExistente.TipoReaccion == tipoReaccion)
            {
                resultado = await this.repo.EliminarLikeHistoriaAsync(idHistoria, idusuario);
                accion = "eliminado";
            }
            // Si la reacción es de otro tipo o no existe, crearla o actualizarla
            else
            {
                if (reaccionExistente == null)
                {
                    resultado = await this.repo.CrearLikeHistoriaAsync(idHistoria, idusuario, tipoReaccion);
                }
                else
                {
                    resultado = await this.repo.ActualizarLikeHistoriaAsync(idHistoria, idusuario, tipoReaccion);
                }
                accion = "agregado";
            }

            if (resultado)
            {
                // Obtener contadores actualizados
                var contadores = await this.repo.ObtenerContadoresReaccionesAsync(idHistoria);
                return Json(new
                {
                    success = true,
                    accion = accion,
                    contadores = contadores
                });
            }

            return Json(new { success = false, message = "Error al procesar la reacción" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObtenerEstadoReaccion(int idHistoria)
        {
            // Verificar que el usuario esté autenticado
            if (!HttpContext.Session.GetInt32("USUARIOID").HasValue)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            // Obtener el ID del usuario de la sesión
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            // Obtener la reacción actual del usuario para esta historia
            var reaccion = await this.repo.ObtenerLikeUsuarioHistoriaAsync(idHistoria, idusuario);

            if (reaccion != null)
            {
                return Json(new
                {
                    success = true,
                    tipoReaccion = reaccion.TipoReaccion
                });
            }

            return Json(new
            {
                success = true,
                tipoReaccion = ""
            });
        }

        public async Task<IActionResult> Notificaciones(int pagina = 1)
        {
            if (HttpContext.Session.GetInt32("USUARIOID") == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            int idUsuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            int tamañoPagina = 10; // Número de notificaciones por página

            // Obtener notificaciones del usuario
            var notificaciones = await this.repo.GetNotificacionesUsuarioAsync(idUsuario, pagina, tamañoPagina);

            // Calcular información de paginación
            int totalNotificaciones = await this.repo.GetTotalNotificacionesUsuarioAsync(idUsuario);
            int totalPaginas = (int)Math.Ceiling((double)totalNotificaciones / tamañoPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalNotificaciones = totalNotificaciones;
            ViewBag.NoLeidas = await this.repo.GetTotalNotificacionesNoLeidasAsync(idUsuario);

            return View(notificaciones);
        }

        public async Task<IActionResult> VerificarNuevasNotificaciones()
        {
            if (HttpContext.Session.GetInt32("USUARIOID") == null)
            {
                return Json(new { hayNuevas = false });
            }

            int idUsuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            // Obtén la última vez que se verificaron notificaciones (desde la sesión)
            DateTime ultimaVerificacion = DateTime.Now.AddDays(-1); // Valor predeterminado: ayer
            if (HttpContext.Session.GetString("ULTIMA_VERIFICACION_NOTIF") != null)
            {
                ultimaVerificacion = DateTime.Parse(HttpContext.Session.GetString("ULTIMA_VERIFICACION_NOTIF"));
            }

            // Verificar si hay notificaciones nuevas desde la última verificación
            bool hayNuevas = await this.repo.HayNotificacionesNuevasDesdeAsync(idUsuario, ultimaVerificacion);

            // Actualizar el timestamp de última verificación
            HttpContext.Session.SetString("ULTIMA_VERIFICACION_NOTIF", DateTime.Now.ToString("o"));

            return Json(new { hayNuevas });
        }

        [HttpPost]
        public async Task<IActionResult> MarcarComoLeida(int idNotificacion)
        {
            if (HttpContext.Session.GetInt32("USUARIOID") == null)
            {
                return Json(new { success = false, mensaje = "Sesión no válida" });
            }

            int idUsuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            bool resultado = await this.repo.MarcarNotificacionComoLeidaAsync(idNotificacion, idUsuario);

            return Json(new { success = resultado });
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodasComoLeidas()
        {
            if (HttpContext.Session.GetInt32("USUARIOID") == null)
            {
                return Json(new { success = false, mensaje = "Sesión no válida" });
            }

            int idUsuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            bool resultado = await this.repo.MarcarTodasNotificacionesComoLeidasAsync(idUsuario);

            return Json(new { success = resultado });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarNotificacion(int idNotificacion)
        {
            if (HttpContext.Session.GetInt32("USUARIOID") == null)
            {
                return Json(new { success = false, mensaje = "Sesión no válida" });
            }

            int idUsuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            bool resultado = await this.repo.EliminarNotificacionAsync(idNotificacion, idUsuario);

            return Json(new { success = resultado });
        }



        public async Task<IActionResult> Perfil()
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            VistaPerfilRefugio perfil = await this.repo.GetPerfilRefugio(idusuario);


            return View(perfil);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarDescripcion(VistaPerfilRefugio modelo)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            var refugio = await repo.GetPerfilRefugio(idusuario);

            refugio.Descripcion = modelo.Descripcion;

            await context.SaveChangesAsync();

            return RedirectToAction("Perfil");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarDetalles(VistaPerfilRefugio modelo)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            var refugio = await repo.GetPerfilRefugio(idusuario);

            refugio.ContactoRefugio = modelo.ContactoRefugio;
            refugio.CantidadAnimales = modelo.CantidadAnimales;
            refugio.CapacidadMaxima = modelo.CapacidadMaxima;
            refugio.Latitud = modelo.Latitud;
            refugio.Longitud = modelo.Longitud;

            await context.SaveChangesAsync();

            return RedirectToAction("Perfil");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarPerfil(VistaPerfilRefugio modelo)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            var usuario = await context.Usuarios.FindAsync(idusuario);
            if (usuario != null)
            {
                usuario.Email = modelo.Email;
            }

            var refugio = await context.Refugios.FirstOrDefaultAsync(r => r.IdUsuario == idusuario);
            if (refugio != null)
            {
                refugio.NombreRefugio = modelo.NombreRefugio;
            }

            await context.SaveChangesAsync();

            return RedirectToAction("Perfil");
        }

        public IActionResult SubirFichero()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubirFichero(IFormFile fichero)
        {
            string fileName = Guid.NewGuid().ToString() + ".png";

            string path = this.helperPath.MapPath(fileName, Folders.Images);
            string pathServer = this.helperPath.MapUrlPathServer(fileName, Folders.Images);

            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                await fichero.CopyToAsync(stream);
            }

            string pathAccessor = this.helperPath.MapUrlPath(fileName, Folders.Images);

            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            var refugio = await repo.GetPerfilRefugio(idusuario);

            // Eliminar la foto de perfil anterior si existe
            if (!string.IsNullOrEmpty(refugio.FotoPerfil))
            {
                string oldFilePath = this.helperPath.MapPath(refugio.FotoPerfil, Folders.Images);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            refugio.FotoPerfil = fileName;
            await context.SaveChangesAsync();

            // Obtener la nueva foto para actualizar la sesión y los claims
            string fotoPerfil = await this.repo.GetFotoPerfilAsync(idusuario);
            HttpContext.Session.SetString("AVATAR", fotoPerfil);

            var user = HttpContext.User;
            var identity = user.Identity as ClaimsIdentity;

            if (identity != null)
            {
                // Eliminar el claim existente
                var existingClaim = identity.FindFirst("FotoPerfil");
                if (existingClaim != null)
                {
                    identity.RemoveClaim(existingClaim);
                }

                // Agregar el nuevo claim con la nueva foto
                Claim claimFoto = new Claim("FotoPerfil", fotoPerfil);
                identity.AddClaim(claimFoto);

                // REFRESCAR LA AUTENTICACIÓN DEL USUARIO
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity)
                );
            }

            return RedirectToAction("Perfil");
        }
    }
}
