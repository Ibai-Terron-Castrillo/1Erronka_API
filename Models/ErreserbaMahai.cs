public class ErreserbaMahai
{
    public virtual int Id { get; set; }
    public virtual int ErreserbakId { get; set; }
    public virtual int MahaiakId { get; set; }

    public virtual Erreserba Erreserba { get; set; }
    public virtual Mahai Mahai { get; set; }
}