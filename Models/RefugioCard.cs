namespace ZuvoPet_V2.Models
{
    public class RefugioCard
    {
        public int Id { get; set; }
        public string NombreRefugio { get; set; }
        public string ContactoRefugio { get; set; }
        public int CantidadAnimales { get; set; }
        public int CapacidadMaxima { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public string FotoPerfil { get; set; }
        public PerfilUsuario PerfilUsuario { get; set; }
    }
}
