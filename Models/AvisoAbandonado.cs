using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZuvoPet_V2.Models
{
    [Table("AVISOSABANDONADOS")]
    public class AvisoAbandonado
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }
        [Column("Latitud")]
        public double Latitud { get; set; }

        [Column("Longitud")]
        public double Longitud { get; set; }

        [Required]
        [Column("Descripcion")]
        public string Descripcion { get; set; }
        [Column("Foto")]
        public string? Foto { get; set; }

        [Required, MaxLength(50)]
        [Column("Estado")]
        public string Estado { get; set; } = "Pendiente";
        [Column("FechaReporte")]
        public DateTime FechaReporte { get; set; } = DateTime.Now;
    }
}
