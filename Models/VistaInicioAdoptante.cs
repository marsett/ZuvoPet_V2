namespace ZuvoPet_V2.Models
{
    public class VistaInicioAdoptante
    {
        public List<MascotaCard> MascotasDestacadas { get; set; }
        public List<HistoriaExitoConDetalles> HistoriasExito { get; set; }
        public Usuario Usuario { get; set; }
    }
}
