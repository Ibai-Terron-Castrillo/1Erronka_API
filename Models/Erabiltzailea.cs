public class Erabiltzailea
{
    public Erabiltzailea() { }
    public virtual int Id { get; set; }
    public virtual string Izena { get; set; }
    public virtual string Pasahitza { get; set; }

    public virtual Langilea Langilea { get; set; } 
}
