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
        //public string grados_Latitud { get; set; }
        //public string grados_Longitud { get; set; }


        public string GradosMinutosSegundos(string valor, string tipo)
        {

            if (string.IsNullOrEmpty(valor) && !valor.Contains('.'))
                throw new Exception("Las coordenadas, deben tener al menos un punto." +
                    "Ejemplo: -56.1881600");

            string[] grados = valor.Split('.');


            double parteEnteraGrados = int.Parse(grados[0]);
            if (tipo == "Longitud")
            {
                if (parteEnteraGrados < -180 || parteEnteraGrados > 180)
                {
                    throw new Exception("La longitud debe estar entre -180° y 180°");
                }
            }
            else
            {

                if (parteEnteraGrados < -90 || parteEnteraGrados > 90)
                {
                    throw new Exception("La latitud debe estar entre -90° y 90°");
                }
            }
            int parteDecimal = int.Parse(grados[1]);
            int minutos = (parteDecimal * 60);
            string StringMinutos = minutos.ToString();
            int parteEnteraMinutos = int.Parse(StringMinutos.Substring(0, 2));
            int parteDecimalMinutos = int.Parse(StringMinutos.Substring(2, StringMinutos.Length - 2));
            double segundos = (parteDecimalMinutos * 60);
            string StringSegundos = segundos.ToString();
            segundos = double.Parse(StringSegundos.Substring(0, 4));
            segundos = segundos / 100;
            parteEnteraGrados = Math.Abs(parteEnteraGrados);
            return $"{parteEnteraGrados}° {parteEnteraMinutos}' {segundos}''";





        }
    }
