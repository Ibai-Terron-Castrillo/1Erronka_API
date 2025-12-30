using System.Collections.Generic;

public class Kategoriak
{
    public Kategoriak()
    {
        Platerak = new List<Platerak>();
    }

    public virtual int Id { get; set; }
    public virtual string Izena { get; set; }

    public virtual IList<Platerak> Platerak { get; set; }
}