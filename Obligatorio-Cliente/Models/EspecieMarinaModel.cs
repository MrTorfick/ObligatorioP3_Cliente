namespace Obligatorio_Cliente.Models
{
    public class EspecieMarinaModel
    {

        public int Id { get; set; }
        public string NombreCientifico { get; set; }
        public string NombreVulgar { get; set; }
        public string Descripcion { get; set; }
        public List<ImagenModel> Imagen { get; set; }
        public double Peso { get; set; }
        public double Longitud { get; set; }
        public List<EcosistemaMarinoModel> EcosistemaMarinos { get; set; }
        public List<AmenazasAsociadasModel> Amenazas { get; set; }
        public int? EstadoConservacionId { get; set; }

    }
}
