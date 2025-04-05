using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("HISTORIASEXITO")]
    public class HistoriaExito
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

        [Required, MaxLength(255)]
        [Column("Titulo")]
        public string Titulo { get; set; }

        [Required]
        [Column("Descripcion")]
        public string Descripcion { get; set; }
        [Column("Foto")]
        public string? Foto { get; set; }
        [Column("FechaPublicacion")]
        public DateTime FechaPublicacion { get; set; } = DateTime.Now;

        [Required, MaxLength(50)]
        [Column("Estado")]
        public string Estado { get; set; } = "Pendiente";
        public virtual ICollection<LikeHistoria> Likes { get; set; } = new HashSet<LikeHistoria>();
    }
}
