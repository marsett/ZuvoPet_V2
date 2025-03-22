using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("USUARIOS_ROLES")]
    public class UsuarioRol
    {
        [ForeignKey("Usuario")]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        [ForeignKey("Rol")]
        [Column("IdRol")]
        public int IdRol { get; set; }
        public Rol Rol { get; set; }
    }
}
