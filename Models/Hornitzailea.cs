using System.Collections.Generic;

public class Hornitzailea
{
    public Hornitzailea()
    {
        Osagaiak = new List<Osagaia>();
    }

    public virtual int Id { get; set; }
    public virtual string Cif { get; set; }
    public virtual string Helbidea { get; set; }
    public virtual string Izena { get; set; }
    public virtual string Sektorea { get; set; }
    public virtual string Telefonoa { get; set; }
    public virtual string Email { get; set; }

    public virtual IList<Osagaia> Osagaiak { get; set; }
}