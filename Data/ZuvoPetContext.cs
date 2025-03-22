using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ZuvoPet_V2.Models;

namespace ZuvoPet_V2.Data
{
    public class ZuvoPet_V2Context: DbContext
    {
        public ZuvoPet_V2Context(DbContextOptions<ZuvoPet_V2Context> options): base(options){}

        // 📌 Usuarios y Roles
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PerfilUsuario> PerfilUsuario { get; set; }
        public DbSet<VistaPerfilAdoptante> VistaPerfilAdoptante { get; set; }
        public DbSet<VistaPerfilRefugio> VistaPerfilRefugio { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UsuarioRol> UsuariosRoles { get; set; }

        // 📌 Adoptantes y Refugios
        public DbSet<Adoptante> Adoptantes { get; set; }
        public DbSet<Refugio> Refugios { get; set; }

        // 📌 Mascotas y Favoritos
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }

        public DbSet<MascotaCard> MascotasFavoritas { get; set; }

        // 📌 Solicitudes y Historias de Éxito
        public DbSet<SolicitudAdopcion> SolicitudesAdopcion { get; set; }
        public DbSet<HistoriaExito> HistoriasExito { get; set; }
        public DbSet<ComentarioHistoria> ComentariosHistorias { get; set; }
        public DbSet<LikeHistoria> LikesHistorias { get; set; }

        // 📌 Eventos de Voluntariado
        public DbSet<EventoVoluntariado> EventosVoluntariado { get; set; }
        public DbSet<InscripcionEvento> InscripcionesEventos { get; set; }

        // 📌 Veterinarios y Avisos de Animales Abandonados
        public DbSet<Veterinario> Veterinarios { get; set; }
        public DbSet<AvisoAbandonado> AvisosAbandonados { get; set; }

        // 📌 Mensajes y Notificaciones
        public DbSet<Mensaje> Mensajes { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura la clave primaria compuesta
            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.IdUsuario, ur.IdRol });  // Definir la clave primaria compuesta

            //modelBuilder.Entity<SolicitudAdopcion>()
            //.Property(e => e.Recursos)
            //.HasConversion(
            //    v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
            //    v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>()
            //);

        }
    }
}
