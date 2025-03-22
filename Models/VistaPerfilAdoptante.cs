using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("V_AdoptantePerfil")]
    public class VistaPerfilAdoptante
    {
        [Key]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("NombreUsuario")]
        public string NombreUsuario { get; set; }

        [Required]
        [Column("Email")]
        public string Email { get; set; }

        [Column("FotoPerfil")]
        public string FotoPerfil { get; set; }

        [Column("Descripcion")]
        public string Descripcion { get; set; }

        [Required]
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
    }
}
