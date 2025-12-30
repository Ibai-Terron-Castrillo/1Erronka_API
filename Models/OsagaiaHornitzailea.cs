public class OsagaiaHornitzailea
{
	public virtual int Id { get; set; }
	public virtual int OsagaiakId { get; set; }
	public virtual int HornitzaileakId { get; set; }

	public virtual Osagaia Osagaia { get; set; }
	public virtual Hornitzailea Hornitzailea { get; set; }
}