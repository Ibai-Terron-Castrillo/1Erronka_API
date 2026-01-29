using System.Collections.Generic;

public class Faktura
{
    public Faktura()
    {
        Komandak = new List<Komanda>();
        Egoera = false;
        Totala = 0;
    }

    public virtual int Id { get; set; }
    public virtual double Totala { get; set; }
    public virtual bool Egoera { get; set; }
    public virtual string FakturaPdf { get; set; }

    public virtual Erreserba Erreserba { get; set; }
    public virtual IList<Komanda> Komandak { get; set; }
}
