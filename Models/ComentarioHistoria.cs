using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("COMENTARIOSHISTORIAS")]
    public class ComentarioHistoria
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("HistoriaExito")]
        [Column("IdHistoria")]
        public int IdHistoria { get; set; }
        public HistoriaExito HistoriaExito { get; set; }

        [ForeignKey("Usuario")]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        [Column("Comentario")]
        public string Comentario { get; set; }
        [Column("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
