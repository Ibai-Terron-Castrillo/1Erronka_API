using System;
using System.Collections.Generic;

public class Erreserba
{
    public Erreserba()
    {
        Mahaiak = new List<Mahai>();
        Data = DateTime.Today;
    }

    public virtual int Id { get; set; }
    public virtual string Izena { get; set; }
    public virtual int Telefonoa { get; set; }
    public virtual string Txanda { get; set; } // "Gosaria", "Bazkaria", "Afaria"
    public virtual int PertsonaKopurua { get; set; }
    public virtual DateTime Data { get; set; }

    public virtual Faktura Faktura { get; set; }
    public virtual IList<Mahai> Mahaiak { get; set; }
}