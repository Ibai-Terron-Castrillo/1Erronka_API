using System;
using System.Collections.Generic;

public class ErreserbaDto
{
    public int Id { get; set; }
    public string Izena { get; set; }
    public string Telefonoa { get; set; }
    public string Txanda { get; set; }
    public int PertsonaKopurua { get; set; }
    public DateTime Data { get; set; }
    public List<MahaiUpdateDto> Mahaiak { get; set; }
}