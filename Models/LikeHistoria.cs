using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("LIKESHISTORIAS")]
    public class LikeHistoria
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

        [Required, MaxLength(20)]
        [Column("TipoReaccion")]
        public string TipoReaccion { get; set; }
        [Column("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
