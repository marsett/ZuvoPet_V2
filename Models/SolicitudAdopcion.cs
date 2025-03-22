using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("SOLICITUDESADOPCION")]
    public class SolicitudAdopcion
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("Adoptante")]
        [Column("IdAdoptante")]
        public int IdAdoptante { get; set; }
        public Adoptante Adoptante { get; set; }

        [ForeignKey("Mascota")]
        [Column("IdMascota")]
        public int IdMascota { get; set; }
        public Mascota Mascota { get; set; }

        [Required, MaxLength(50)]
        [Column("Estado")]
        public string Estado { get; set; } = "Pendiente";
        [Column("ExperienciaPrevia")]
        public bool? ExperienciaPrevia { get; set; }
        [Column("TipoVivienda")]
        public string? TipoVivienda { get; set; }
        [Column("OtrosAnimales")]
        public bool? OtrosAnimales { get; set; }
        [Column("Recursos")]
        public List<string> Recursos { get; set; }
        [Column("TiempoEnCasa")]
        public string? TiempoEnCasa { get; set; }
        [Column("FechaSolicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
        [Column("FechaRespuesta")]
        public DateTime? FechaRespuesta { get; set; }
    }
}
