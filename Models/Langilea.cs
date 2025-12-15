public class Langilea
{
    public Langilea() { }
    public virtual int Id { get; set; }
    public virtual string Izena { get; set; }
    public virtual string Abizena1 { get; set; }
    public virtual string Abizena2 { get; set; }
    public virtual string Telefonoa { get; set; }

    public virtual Lanpostua Lanpostua { get; set; }
}
