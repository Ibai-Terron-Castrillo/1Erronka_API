using System;

public class Kaja
{
    public Kaja()
    {
        Data = DateTime.Today;
    }

    public virtual int Id { get; set; }
    public virtual DateTime Data { get; set; }
    public virtual double KajaHasiera { get; set; }
    public virtual double KajaBukaera { get; set; }
}