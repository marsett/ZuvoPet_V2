using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("INSCRIPCIONESEVENTOS")]
    public class InscripcionEvento
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        [ForeignKey("EventoVoluntariado")]
        [Column("IdEvento")]
        public int IdEvento { get; set; }
        public EventoVoluntariado EventoVoluntariado { get; set; }

        [Required, MaxLength(50)]
        [Column("Estado")]
        public string Estado { get; set; } = "Pendiente";
    }
}
