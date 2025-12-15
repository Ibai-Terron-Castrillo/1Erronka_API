using System.Collections.Generic;
using System.Text.Json.Serialization;


public class Lanpostua
{
    public Lanpostua() { }
    public virtual int Id { get; set; }
    public virtual string Izena { get; set; }

    [JsonIgnore]
    public virtual IList<Langilea> Langilea { get; set; }
}
