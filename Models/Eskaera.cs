using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Eskaera
{
    public Eskaera()
    {
        Osagaiak = new List<Osagaia>();
        Egoera = false;
    }

    public virtual int Id { get; set; }
    public virtual int EskaeraZenbakia { get; set; }
    public virtual double Totala { get; set; }
    public virtual bool Egoera { get; set; }
    public virtual string EskaeraPdf { get; set; }

    [JsonIgnore]
    public virtual IList<Osagaia> Osagaiak { get; set; }
}