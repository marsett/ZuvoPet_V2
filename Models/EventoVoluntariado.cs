using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZuvoPet_V2.Models
{
    [Table("EVENTOSVOLUNTARIADO")]
    public class EventoVoluntariado
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("Refugio")]
        [Column("IdRefugio")]
        public int IdRefugio { get; set; }
        public Refugio Refugio { get; set; }

        [Required, MaxLength(255)]
        [Column("Nombre")]
        public string Nombre { get; set; }

        [Required]
        [Column("Descripcion")]
        public string Descripcion { get; set; }
        [Column("FechaInicio")]
        public DateTime FechaInicio { get; set; }
        [Column("FechaFin")]
        public DateTime FechaFin { get; set; }

        [Required]
        [Column("Capacidad")]
        public int Capacidad { get; set; }

        public virtual ICollection<InscripcionEvento> Inscripciones { get; set; } = new HashSet<InscripcionEvento>();
    }
}
