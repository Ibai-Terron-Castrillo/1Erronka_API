using System.Collections.Generic;

public class Osagaia
{
    public Osagaia()
    {
        Hornitzaileak = new List<Hornitzailea>();
        Platerak = new List<Platerak>();
        Eskatu = false;
    }

    public virtual int Id { get; set; }
    public virtual string Izena { get; set; }
    public virtual double AzkenPrezioa { get; set; }
    public virtual int Stock { get; set; }
    public virtual int GutxienekoStock { get; set; }
    public virtual bool Eskatu { get; set; }

    public virtual IList<Hornitzailea> Hornitzaileak { get; set; }
    public virtual IList<Platerak> Platerak { get; set; }
}