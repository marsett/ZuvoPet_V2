using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZuvoPet_V2.Models
{
    [Table("MENSAJES")]
    public class Mensaje
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("Emisor")]
        [Column("IdEmisor")]
        public int IdEmisor { get; set; }
        public Usuario Emisor { get; set; }

        [ForeignKey("Receptor")]
        [Column("IdReceptor")]
        public int IdReceptor { get; set; }
        public Usuario Receptor { get; set; }

        [Required]
        [Column("Contenido")]
        public string Contenido { get; set; }
        [Column("Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;
        [Column("Leido")]
        public bool Leido { get; set; } = false;
    }
}
