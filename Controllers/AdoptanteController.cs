using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;
using ZuvoPet_V2.Data;
using ZuvoPet_V2.Filters;
using ZuvoPet_V2.Helpers;
using ZuvoPet_V2.Models;
using ZuvoPet_V2.Repositories;
using Microsoft.AspNetCore.SignalR;
using ZuvoPet_V2.Hubs;

namespace ZuvoPet_V2.Controllers
{
    [AuthorizeZuvoPet_V2("Adoptante")]
    public class AdoptanteController : BaseController
    {
        private readonly ZuvoPet_V2Context context;
        private readonly IRepositoryZuvoPet_V2 repo;
        private readonly IHubContext<ChatHub> hubContext;
        private HelperPathProvider helperPath;
        public AdoptanteController(ZuvoPet_V2Context context, IRepositoryZuvoPet_V2 repo, HelperPathProvider helperPath, IHubContext<ChatHub> hubContext)
        {
            this.context = context;
            this.repo = repo;
            this.helperPath = helperPath;
            this.hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            List<MascotaCard> mascotasDestacadas = await this.repo.ObtenerMascotasDestacadasAsync();
            
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

            var vistaInicioAdoptante = new VistaInicioAdoptante
            {
                MascotasDestacadas = mascotasDestacadas,
                HistoriasExito = historiasConDetalles
            };

            return View(vistaInicioAdoptante);
        }

        public async Task<IActionResult> Refugios(int pagina = 1)
        {
            // Obtener todos los refugios
            List<Refugio> refugios = await this.repo.ObtenerRefugiosAsync();

            // Configuración de la paginación
            int refugiosPorPagina = 9; // Puedes ajustar esta cantidad según tus necesidades

            // Calcula el total de páginas
            int totalRegistros = refugios.Count;
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / refugiosPorPagina);

            // Validación de la página actual
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            // Filtra los refugios para la página actual
            var refugiosPaginados = refugios
                .Skip((pagina - 1) * refugiosPorPagina)
                .Take(refugiosPorPagina)
                .ToList();

            // Asigna las variables de paginación al ViewBag
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Para solicitudes AJAX, devuelve la vista parcial o completa
                return View(refugiosPaginados);
            }

            return View(refugiosPaginados);
        }

        public async Task<IActionResult> DetallesRefugio(int idrefugio)
        {
            try
            {
                Refugio refugio = await this.repo.GetDetallesRefugioAsync(idrefugio);
                List<Mascota> mascotasDelRefugio = await this.repo.GetMascotasPorRefugioAsync(idrefugio);
                if(mascotasDelRefugio.Count() != 0)
                {
                    refugio.ListaMascotas = mascotasDelRefugio;
                }
                
                return View(refugio);
            }
            catch (Exception ex)
            {
                // Registrar el error
                Console.WriteLine($"Error al cargar refugio {idrefugio}: {ex.Message}");
                // Opcional: más detalles
                Console.WriteLine(ex.StackTrace);

                // Redirigir a una página de error o mostrar un mensaje
                ViewBag.ErrorMessage = "No se pudo cargar el refugio. Por favor intente más tarde.";
                return View("Error");
            }
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








        public async Task<IActionResult> Perfil(int pagina = 1)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            VistaPerfilAdoptante perfil = await this.repo.GetPerfilAdoptante(idusuario);

            var favoritos = await this.repo.ObtenerMascotasFavoritas(idusuario);

            // Lógica de paginación
            var totalMascotas = favoritos.Count;
            var mascotaActual = totalMascotas > 0 && pagina <= totalMascotas ? favoritos[pagina - 1] : null;

            var viewModel = new PerfilAdoptanteViewModel
            {
                Perfil = perfil,
                MascotasFavoritas = favoritos,
                PaginaActual = pagina,
                TotalPaginas = totalMascotas
            };

            // Pasamos la mascota actual a la vista si existe
            ViewData["MascotaActual"] = mascotaActual;

            return View(viewModel);
        }

        [AjaxAuthentication]
        public async Task<IActionResult> _MascotaFavoritaPartial(int pagina = 1)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            var favoritos = await this.repo.ObtenerMascotasFavoritas(idusuario);

            // Lógica de paginación
            var totalMascotas = favoritos.Count;

            // Validar que la página solicitada sea válida
            if (pagina < 1) pagina = 1;
            if (pagina > totalMascotas) pagina = totalMascotas;

            // Si no hay mascotas, devolver vista vacía con mensaje
            if (totalMascotas == 0)
            {
                return PartialView(null);
            }

            // Obtener la mascota actual (restamos 1 porque los índices empiezan en 0)
            var mascotaActual = favoritos[pagina - 1];

            // Crear viewmodel para la vista parcial
            var viewModel = new
            {
                Mascota = mascotaActual,
                PaginaActual = pagina,
                TotalPaginas = totalMascotas
            };

            return PartialView(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarDescripcion(VistaPerfilAdoptante modelo)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            var adoptante = await repo.GetPerfilAdoptante(idusuario);

            adoptante.Descripcion = modelo.Descripcion;

            await context.SaveChangesAsync();

            return RedirectToAction("Perfil");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarDetalles(VistaPerfilAdoptante modelo)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            var adoptante = await repo.GetPerfilAdoptante(idusuario);

            adoptante.TipoVivienda = modelo.TipoVivienda;
            adoptante.RecursosDisponibles = modelo.RecursosDisponibles;
            adoptante.TiempoEnCasa = modelo.TiempoEnCasa;
            adoptante.TieneJardin = modelo.TieneJardin;
            adoptante.OtrosAnimales = modelo.OtrosAnimales;

            await context.SaveChangesAsync();

            return RedirectToAction("Perfil");
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarPerfil(VistaPerfilAdoptante modelo)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");


            var usuario = await context.Usuarios.FindAsync(idusuario);
            if (usuario != null)
            {
                usuario.Email = modelo.Email;
            }

            var adoptante = await context.Adoptantes.FirstOrDefaultAsync(a => a.IdUsuario == idusuario);
            if (adoptante != null)
            {
                adoptante.Nombre = modelo.Nombre;
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

            var adoptante = await repo.GetPerfilAdoptante(idusuario);

            // Eliminar la foto de perfil anterior si existe
            if (!string.IsNullOrEmpty(adoptante.FotoPerfil))
            {
                string oldFilePath = this.helperPath.MapPath(adoptante.FotoPerfil, Folders.Images);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            adoptante.FotoPerfil = fileName;
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

        public async Task<IActionResult> Adopta(string ordenEdad, string tamano, string sexo, string especie, int pagina = 1)
        {
            // Llamamos al servicio para obtener todas las mascotas
            List<MascotaCard> mascotas = await this.repo.ObtenerMascotasAsync();

            // Aplicamos los filtros en la lista obtenida
            var query = mascotas.AsQueryable();

            // Filtro por Tamaño
            if (!string.IsNullOrEmpty(tamano))
            {
                query = query.Where(m => m.Tamano == tamano);
            }

            // Filtro por Sexo
            if (!string.IsNullOrEmpty(sexo))
            {
                query = query.Where(m => m.Sexo == sexo);
            }

            // Filtro por Especie
            if (!string.IsNullOrEmpty(especie))
            {
                query = query.Where(m => m.Especie == especie);
            }

            // Ordenar por Edad
            if (ordenEdad == "asc")
            {
                query = query.OrderBy(m => m.Edad); // Orden ascendente (de más joven a más viejo)
            }
            else if (ordenEdad == "desc")
            {
                query = query.OrderByDescending(m => m.Edad); // Orden descendente (de más viejo a más joven)
            }

            // Obtener la lista filtrada y ordenada
            var filteredMascotas = query.ToList();

            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            var favoritos = await this.repo.ObtenerMascotasFavoritas(idusuario);
            var idsFavoritos = favoritos.Select(f => f.Id).ToList();

            // Pasar los IDs de favoritos a ViewBag
            ViewData["IDSFAVORITOS"] = idsFavoritos;

            if (idsFavoritos.Count == 0)
            {
                Console.WriteLine("No hay favoritos para este usuario.");
            }

            // Configuración de la paginación
            int mascotasPorPagina = 9; // Puedes ajustar esta cantidad según tus necesidades

            // Calcula el total de páginas
            int totalRegistros = filteredMascotas.Count; // AQUÍ ESTÁ EL CAMBIO: usamos filteredMascotas.Count
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / mascotasPorPagina);

            // Validación de la página actual
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            // Filtra las mascotas para la página actual
            var mascotasPaginadas = filteredMascotas // AQUÍ ESTÁ EL CAMBIO: usamos filteredMascotas
                .Skip((pagina - 1) * mascotasPorPagina)
                .Take(mascotasPorPagina)
                .ToList();

            // Asigna las variables de paginación al ViewBag
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Para solicitudes AJAX, devuelve la vista parcial o completa
                return View(mascotasPaginadas);
            }

            return View(mascotasPaginadas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MascotaFavorita(int idmascota)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            Console.WriteLine("IDUSUARIO", idusuario);

            var lastActionTime = await this.repo.ObtenerUltimaAccionFavorito(idusuario, idmascota);

            // Si la acción fue reciente (por ejemplo, en los últimos 2 segundos), no realizar nada
            if (lastActionTime != null && (DateTime.Now - lastActionTime.Value).TotalSeconds < 2)
            {
                return Json(new { success = false, message = "Esperar un momento antes de volver a hacer clic." });
            }

            bool esFavorito = await this.repo.EsFavorito(idusuario, idmascota);

            if (esFavorito)
            {
                await this.repo.EliminarFavorito(idusuario, idmascota);
            }
            else
            {
                await this.repo.InsertMascotaFavorita(idusuario, idmascota);
            }

            // For Ajax requests, return a JSON result
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, isFavorite = !esFavorito });
            }

            return RedirectToAction("Adopta");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarFavorito(int idmascota)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            Console.WriteLine("IDUSUARIO", idusuario);

            var lastActionTime = await this.repo.ObtenerUltimaAccionFavorito(idusuario, idmascota);

            if (lastActionTime != null && (DateTime.Now - lastActionTime.Value).TotalSeconds < 2)
            {
                return Json(new { success = false, message = "Esperar un momento antes de volver a hacer clic." });
            }

            bool esFavorito = await this.repo.EsFavorito(idusuario, idmascota);

            if (esFavorito)
            {
                await this.repo.EliminarFavorito(idusuario, idmascota);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, isFavorite = !esFavorito });
            }

            return RedirectToAction("Perfil");
        }

        public async Task<IActionResult> DetallesMascota(int idmascota)
        {
            Mascota mascota = await this.repo.GetDetallesMascotaAsync(idmascota);
            // Comprobar si el usuario ya ha visto esta mascota
            string cookieName = $"Mascota_Vista_{idmascota}";
            if (Request.Cookies[cookieName] == null)
            {
                // El usuario no ha visto esta mascota antes, incrementar contador
                mascota.Vistas++;
                await this.context.SaveChangesAsync();

                // Crear una cookie para registrar que este usuario ya vio esta mascota
                // La cookie expirará después de 30 días
                CookieOptions options = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    IsEssential = true,
                    HttpOnly = true
                };
                Response.Cookies.Append(cookieName, "visto", options);
            }
            return View(mascota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitudAdopcion(int idmascota)
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            // Verificar si ya existe una solicitud para evitar duplicados
            bool existeSolicitud = await this.repo.ExisteSolicitudAdopcionAsync(idusuario, idmascota);
            if (existeSolicitud)
            {
                return Json(new { success = false, message = "Ya existe una solicitud para esta mascota" });
            }
            bool resultado = await this.repo.CrearSolicitudAdopcionAsync(idusuario, idmascota);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = resultado });
            }

            // Si no es AJAX (por si acaso), redirigir a la página de detalles
            return RedirectToAction("DetallesMascota", "Adoptante", new { idmascota });
        }

        [HttpGet]
        public async Task<IActionResult> VerificarSolicitudExistente(int idmascota)
        {
            
                // Verificar que el usuario esté autenticado
                if (!HttpContext.Session.Keys.Contains("USUARIOID"))
                {
                    return Json(new { solicitudExiste = false });
                }

                int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
                bool existeSolicitud = await this.repo.ExisteSolicitudAdopcionAsync(idusuario, idmascota);
                return Json(new { solicitudExiste = existeSolicitud });
            
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

        public async Task<IActionResult> CrearHistoriaExito()
        {
            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");

            // Obtener las mascotas adoptadas sin historia
            var mascotas = await this.repo.GetMascotasAdoptadasSinHistoria(idusuario);

            // Preparar los elementos para el dropdown
            ViewBag.Mascotas = new SelectList(mascotas, "Id", "Nombre");

            // Obtener el ID del adoptante y pasarlo a la vista
            var adoptante = await this.repo.GetAdoptanteByUsuarioId(idusuario);
            ViewBag.IdAdoptante = adoptante.Id;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearHistoriaExito(HistoriaExito historiaexito, IFormFile fichero)
        {
            //if (!ModelState.IsValid)
            //{
            //    int idUsuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            //    var mascotas = await this.repo.GetMascotasAdoptadasSinHistoria(idUsuario);
            //    ViewBag.Mascotas = new SelectList(mascotas, "Id", "Nombre");
            //    var adoptante = await this.repo.GetAdoptanteByUsuarioId(idUsuario);
            //    ViewBag.IdAdoptante = adoptante.Id;
            //    return View(historiaexito);
            //}
            if (fichero != null)
            {
                string fileName = Guid.NewGuid().ToString() + ".png";
                string path = this.helperPath.MapPath(fileName, Folders.Images);
                using (Stream stream = new FileStream(path, FileMode.Create))
                {
                    await fichero.CopyToAsync(stream);
                }
                historiaexito.Foto = fileName;
            }

            int idusuario = (int)HttpContext.Session.GetInt32("USUARIOID");
            await this.repo.CrearHistoriaExito(historiaexito, idusuario);
            return RedirectToAction("HistoriasExito");
        }








        private async Task<int> GetIdUsuarioActual()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var adoptante = await context.Adoptantes.FirstOrDefaultAsync(a => a.IdUsuario == userId);
            return userId;
        }

        // Lista de conversaciones
        [HttpGet]
        public async Task<IActionResult> Mensajes()
        {
            int usuarioId = await GetIdUsuarioActual();
            var conversaciones = await this.repo.GetConversacionesAdoptanteAsync(usuarioId);
            return View(conversaciones);
        }

        // Ver chat específico
        [HttpGet]
        public async Task<IActionResult> Chat(int id)
        {
            int usuarioActualId = await GetIdUsuarioActual();

            // Obtener mensajes
            var mensajes = await this.repo.GetMensajesConversacionAsync(usuarioActualId, id);

            // Marcar como leídos los mensajes recibidos
            foreach (var mensaje in mensajes.Where(m => m.IdEmisor == id && !m.Leido))
            {
                mensaje.Leido = true;
            }
            await context.SaveChangesAsync();

            // Obtener nombre del refugio
            var refugio = await context.Refugios.FirstOrDefaultAsync(r => r.IdUsuario == id);
            string nombreDestinatario = refugio != null ? refugio.NombreRefugio : "UsuarioO";

            var viewModel = new ChatViewModel
            {
                Mensajes = mensajes,
                NombreDestinatario = nombreDestinatario,
                IdDestinatario = id
            };

            return View(viewModel);
        }

        // Enviar mensaje
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensaje(int destinatarioId, string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
            {
                return BadRequest("El mensaje no puede estar vacío");
            }

            int emisorId = await GetIdUsuarioActual();
            var mensaje = await this.repo.AgregarMensajeAsync(emisorId, destinatarioId, contenido);

            // Notificar por SignalR
            await hubContext.Clients.User(destinatarioId.ToString())
                .SendAsync("RecibirMensaje", emisorId, mensaje.Contenido, mensaje.Fecha);

            return Json(new { success = true });
        }

        // Iniciar chat desde detalles de refugio
        [HttpGet]
        public async Task<IActionResult> IniciarChat(int refugioId)
        {
            // Obtener el IdUsuario del refugio
            var refugio = await context.Refugios.FirstOrDefaultAsync(r => r.Id == refugioId);
            if (refugio == null)
            {
                return NotFound();
            }

            return RedirectToAction("Chat", new { id = refugio.IdUsuario });
        }
    }
}
