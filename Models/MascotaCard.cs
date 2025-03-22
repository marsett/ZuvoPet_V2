namespace ZuvoPet_V2.Models
{
    public class MascotaCard
    {
        public int Id { get; set; }
        public string Nombre {  get; set; }
        public string Especie { get; set; }
        public int Edad { get; set; }
        public string Tamano { get; set; }
        public string Sexo { get; set; }
        public string? Foto { get; set; }
    }
}
