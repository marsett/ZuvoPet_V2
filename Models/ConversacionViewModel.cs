namespace ZuvoPet_V2.Models
{
    public class ConversacionViewModel
    {
        public int UsuarioId { get; set; }
        public string Foto { get; set; }
        public string NombreUsuario { get; set; }
        public string UltimoMensaje { get; set; }
        public DateTime FechaUltimoMensaje { get; set; }
        public int NoLeidos { get; set; }
    }
}
