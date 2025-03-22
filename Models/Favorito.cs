using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("FAVORITOS")]
    public class Favorito
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
        [Column("FechaAgregado")]
        public DateTime FechaAgregado { get; set; } = DateTime.Now;
    }
}
