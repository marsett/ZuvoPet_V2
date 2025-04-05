using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("USUARIOS")]
    public class Usuario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("NombreUsuario")]
        public string NombreUsuario { get; set; }

        [Required, MaxLength(255), EmailAddress]
        [Column("Email")]
        public string Email { get; set; }

        [Required, MaxLength(255)]
        [Column("ContrasenaLimpia")]
        public string ContrasenaLimpia { get; set; }

        [Required]
        [Column("ContrasenaEncriptada")]
        public byte[] ContrasenaEncriptada { get; set; }

        [Required, MaxLength(255)]
        [Column("Salt")]
        public string Salt { get; set; }

        [Required, MaxLength(20)]
        [Column("TipoUsuario")]
        public string TipoUsuario { get; set; } // "Adoptante" o "Refugio"
        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public PerfilUsuario PerfilUsuario { get; set; }
    }
}
