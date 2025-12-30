public class PlaterakOsagaia
{
    public virtual int Id { get; set; }
    public virtual int OsagaiakId { get; set; }
    public virtual int PlaterakId { get; set; }
    public virtual int Kopurua { get; set; }

    public virtual Osagaia Osagaia { get; set; }
    public virtual Platerak Platerak { get; set; }
}