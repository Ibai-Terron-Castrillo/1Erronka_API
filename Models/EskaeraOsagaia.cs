public class EskaeraOsagaia
{
    public virtual int Id { get; set; }
    public virtual int EskaerakId { get; set; }
    public virtual int OsagaiakId { get; set; }
    public virtual int Kopurua { get; set; }
    public virtual double Prezioa { get; set; }
    public virtual double Totala { get; set; }

    public virtual Eskaera Eskaera { get; set; }
    public virtual Osagaia Osagaia { get; set; }
}