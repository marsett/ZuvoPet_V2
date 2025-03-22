using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZuvoPet_V2.Models
{
    [Table("NOTIFICACIONES")]
    public class Notificacion
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        [Required, MaxLength(255)]
        [Column("Mensaje")]
        public string Mensaje { get; set; }

        [Required, MaxLength(50)]
        [Column("Tipo")]
        public string Tipo { get; set; }
        [Column("Url")]
        public string? Url { get; set; }
        [Column("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;
        [Column("Leido")]
        public bool Leido { get; set; } = false;
    }
}
