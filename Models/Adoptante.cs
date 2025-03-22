using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZuvoPet_V2.Models
{
    [Table("ADOPTANTES")]
    public class Adoptante
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        [Required, MaxLength(255)]
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("TipoVivienda")]
        public string TipoVivienda { get; set; }
        [Column("TieneJardin")]
        public bool TieneJardin { get; set; }
        [Column("OtrosAnimales")]
        public bool OtrosAnimales { get; set; }
        [Column("RecursosDisponibles")]
        public List<string> RecursosDisponibles { get; set; }
        [Column("TiempoEnCasa")]
        public string TiempoEnCasa { get; set; }

        public virtual ICollection<Favorito> Favoritos { get; set; } = new HashSet<Favorito>();
    }
}
