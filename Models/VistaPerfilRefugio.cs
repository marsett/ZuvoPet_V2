using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZuvoPet_V2.Models
{
    [Table("V_RefugioPerfil")]
    public class VistaPerfilRefugio
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
        [Column("NombreRefugio")]
        public string NombreRefugio { get; set; }

        [Column("ContactoRefugio")]
        public string ContactoRefugio { get; set; }

        [Column("CantidadAnimales")]
        public int CantidadAnimales { get; set; }

        [Column("CapacidadMaxima")]
        public int CapacidadMaxima { get; set; }

        [Column("Latitud")]
        public double Latitud { get; set; }

        [Column("Longitud")]
        public double Longitud { get; set; }
    }
}
