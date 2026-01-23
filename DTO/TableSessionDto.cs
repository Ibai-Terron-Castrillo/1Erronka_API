using System;

public class TableSessionDto
{
    public int MahaiId { get; set; }
    public int ErreserbaMahaiId { get; set; }
    public int ErreserbaId { get; set; }
    public int FakturaId { get; set; }
    public bool FakturaEgoera { get; set; }
    public double FakturaTotala { get; set; }
    public bool RequiresDecision { get; set; }
    public string Txanda { get; set; }
    public DateTime Data { get; set; }
}