namespace Obligatorio_Cliente.Models
{
    public class PaisModel
    {
        public int id { get; set; }
        public Name name { get; set; }
        public string cca3 { get; set; }



        public class Name
        {
            public string common { get; set; }
        }

    }
}
