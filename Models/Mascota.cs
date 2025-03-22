using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("MASCOTAS")]
    public class Mascota
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Column("Nombre")]
        public string Nombre { get; set; }

        [Required, MaxLength(50)]
        [Column("Especie")]
        public string Especie { get; set; }
        [Column("FechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [Required, MaxLength(50)]
        [Column("Tamano")]
        public string Tamano { get; set; }

        [Required, MaxLength(20)]
        [Column("Sexo")]
        public string Sexo { get; set; }
        [Column("Castrado")]
        public bool Castrado { get; set; } = false;
        [Column("Vacunado")]
        public bool Vacunado { get; set; } = false;
        [Column("Desparasitado")]
        public bool Desparasitado { get; set; } = false;
        [Column("Sano")]
        public bool Sano { get; set; } = true;
        [Column("Microchip")]
        public bool Microchip { get; set; } = false;
        [Column("CompatibleConNinos")]
        public bool CompatibleConNinos { get; set; } = false;
        [Column("CompatibleConAdultos")]
        public bool CompatibleConAdultos { get; set; } = false;
        [Column("CompatibleConOtrosAnimales")]
        public bool CompatibleConOtrosAnimales { get; set; } = false;
        [Column("Personalidad")]
        public string? Personalidad { get; set; }
        [Column("Descripcion")]
        public string? Descripcion { get; set; }
        [Required, MaxLength(50)]
        [Column("Estado")]
        public string Estado { get; set; } = "Disponible";
        [Column("Latitud")]
        public double Latitud { get; set; }

        [Column("Longitud")]
        public double Longitud { get; set; }

        [ForeignKey("Refugio")]
        [Column("IdRefugio")]
        public int? IdRefugio { get; set; }
        public Refugio? Refugio { get; set; }
        [Column("Foto")]
        public string? Foto { get; set; }
        [Column("Vistas")]
        public int Vistas { get; set; } = 0;
        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
