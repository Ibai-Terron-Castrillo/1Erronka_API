public class KomandaSortuDto
{
    public int FakturakId { get; set; }
    public int PlaterakId { get; set; }
    public int Kopurua { get; set; }
    public double Totala { get; set; }
}

public class KomandaDto
{
    public int Id { get; set; }
    public int FakturakId { get; set; }
    public int PlaterakId { get; set; }
    public int Kopurua { get; set; }
    public double Totala { get; set; }
    public string Oharrak { get; set; }
    public bool Egoera { get; set; }
    public PlaterakRefDto Platerak { get; set; }
    public FakturaRefDto Faktura { get; set; }
}

public class PlaterakRefDto
{
    public int Id { get; set; }
}

public class FakturaRefDto
{
    public int Id { get; set; }
}

public class OharrakUpdateDto
{
    public string Oharrak { get; set; }
}
