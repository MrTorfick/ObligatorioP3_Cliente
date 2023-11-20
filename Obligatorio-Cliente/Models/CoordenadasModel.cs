namespace Obligatorio_Cliente.Models
{
    public class CoordenadasModel
    {

        public string Longitud { get; set; }
        public string Latitud { get; set; }


        public CoordenadasModel(string longitud, string latitud)
        {
            Longitud = longitud;
            Latitud = latitud;
        }

    }
}
