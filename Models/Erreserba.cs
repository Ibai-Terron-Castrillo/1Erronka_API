public class Erreserba
{
    public virtual int Id { get; set; }
    public virtual string Izena { get; set; }
    public virtual int? Telefonoa { get; set; }
    public virtual string Txanda { get; set; }  // enum: Gosaria, Bazkaria, Afaria
    public virtual int? PertsonaKopurua { get; set; }
    public virtual DateTime? Data { get; set; }

    public virtual IList<Fakturak> Fakturak { get; set; } = new List<Fakturak>(); 
}
