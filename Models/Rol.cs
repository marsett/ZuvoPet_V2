using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZuvoPet_V2.Models
{
    [Table("ROLES")]
    public class Rol
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; }
    }
}
