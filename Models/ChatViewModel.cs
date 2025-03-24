namespace ZuvoPet_V2.Models
{
    public class ChatViewModel
    {
        public List<Mensaje> Mensajes { get; set; }
        public string NombreDestinatario { get; set; }
        public int IdDestinatario { get; set; }
        public string NuevoMensaje { get; set; }
    }
}
