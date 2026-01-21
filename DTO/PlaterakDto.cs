using System.Collections.Generic;

namespace API.DTO
{
    public class PlaterakDto
    {
        public int Id { get; set; }
        public string Izena { get; set; }
        public double Prezioa { get; set; }
        public int Stock { get; set; }
        public int KategoriaId { get; set; }
        public string KategoriaIzena { get; set; }
    }

    public class PlateraPostDto
    {
        public string Izena { get; set; }
        public double Prezioa { get; set; }
        public int Stock { get; set; }
        public KategoriaRefDto Kategoria { get; set; }
    }

    public class PlateraPutDto
    {
        public int Id { get; set; }
        public string Izena { get; set; }
        public double Prezioa { get; set; }
        public int Stock { get; set; }
        public KategoriaDto Kategoria { get; set; }
        public List<OsagaiDto> Osagaiak { get; set; }
    }

    public class KategoriaRefDto
    {
        public int Id { get; set; }
    }

    public class StockEguneratuDto
    {
        public int Kopurua { get; set; }
    }

    public class PlateraDesaktibatuDto
    {
        public bool Desaktibatua { get; set; }
        public string Arrazoia { get; set; }
    }

    public class OsagaiDto
    {
        public int Id { get; set; }
        public int Kopurua { get; set; }
    }
}