using System.Collections.Generic;

public class Platerak
{
    public Platerak()
    {
        Osagaiak = new List<Osagaia>();
    }

    public virtual int Id { get; set; }
    public virtual int KategoriakId { get; set; }
    public virtual string Izena { get; set; }
    public virtual double Prezioa { get; set; }
    public virtual int Stock { get; set; }

    public virtual Kategoriak Kategoriak { get; set; }
    public virtual IList<Osagaia> Osagaiak { get; set; }
}