using System;

public class Komanda
{
    public Komanda()
    {
        Egoera = false;
    }

    public virtual int Id { get; set; }
    public virtual int Kopurua { get; set; }
    public virtual double Totala { get; set; }
    public virtual string Oharrak { get; set; }
    public virtual bool Egoera { get; set; }

    public virtual Platerak Platerak { get; set; }
    public virtual Faktura Faktura { get; set; }
}