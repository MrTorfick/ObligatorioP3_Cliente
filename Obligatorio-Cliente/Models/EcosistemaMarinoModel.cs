namespace Obligatorio_Cliente.Models
{
    public class EcosistemaMarinoModel
    {

        public int Id { get; set; }
        public string Nombre { get; set; }
        public CoordenadasModel Coordenadas { get; set; }
        public double Area { get; set; }
        public string DescripcionCaracteristicas { get; set; }
        public List<ImagenModel> Imagen { get; set; }
        public List<EspecieMarinaModel> EspeciesHabitan { get; set; }
        public List<AmenazasAsociadasModel> Amenazas { get; set; }
        public int? PaisId { get; set; }
        public int? EstadoConservacionId { get; set; }
        public string grados_Latitud { get; set; }
        public string grados_Longitud { get; set; }








    }
}
