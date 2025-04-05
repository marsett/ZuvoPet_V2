namespace ZuvoPet_V2.Models
{
    public class PerfilAdoptanteViewModel
    {
        public VistaPerfilAdoptante Perfil { get; set; }
        public List<MascotaCard> MascotasFavoritas { get; set; }
        public List<MascotaAdoptada> MascotasAdoptadas { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }
}
