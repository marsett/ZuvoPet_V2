using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("PERFIL_USUARIO")]
    public class PerfilUsuario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [ForeignKey("Usuario")]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        [Column("FotoPerfil")]
        public string FotoPerfil { get; set; }

        [Column("Descripcion")]
        public string Descripcion { get; set; }
    }
}
