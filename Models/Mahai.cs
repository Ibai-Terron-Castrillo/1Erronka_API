using System.Collections.Generic;

public class Mahai
{
    public Mahai()
    {
        Erreserbak = new List<Erreserba>();
    }

    public virtual int Id { get; set; }
    public virtual int MahaiZenbakia { get; set; }
    public virtual int PertsonaMax { get; set; }

    public virtual IList<Erreserba> Erreserbak { get; set; }
}
