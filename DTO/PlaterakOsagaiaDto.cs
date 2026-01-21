namespace API.DTO
{
    public class PlaterakOsagaiaDto
    {
        public int Id { get; set; }
        public int OsagaiakId { get; set; }
        public int PlaterakId { get; set; }
        public int Kopurua { get; set; }
        public string OsagaiaIzena { get; set; }
        public double OsagaiaPrezioa { get; set; }
    }

    public class KomandaInfoDto
    {
        public int Id { get; set; }
        public int FakturaId { get; set; }
        public int Kopurua { get; set; }
        public double Totala { get; set; }
        public int Egoera { get; set; }
        public string Oharrak { get; set; }
        public double FakturaTotala { get; set; }
        public int FakturaEgoera { get; set; }
        public string BezeroIzena { get; set; }
        public int BezeroTelefonoa { get; set; }
    }
}