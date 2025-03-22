using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("VETERINARIOS")]
    public class Veterinario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        [Column("Nombre")]
        public string Nombre { get; set; }

        [Required, MaxLength(255)]
        [Column("Especializacion")]
        public string Especializacion { get; set; }
        [Column("Contacto")]
        public string? Contacto { get; set; }
        [Column("Latitud")]
        public double Latitud { get; set; }

        [Column("Longitud")]
        public double Longitud { get; set; }
    }
}
