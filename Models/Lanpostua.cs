using System.Collections.Generic;

public class Lanpostua
{
    public virtual int Id { get; set; }
    public virtual string Izena { get; set; }

    public virtual IList<Langilea> Langilea { get; set; } = new List<Langilea>();
}
