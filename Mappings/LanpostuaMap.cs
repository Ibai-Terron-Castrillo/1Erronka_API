using FluentNHibernate.Mapping;

public class LanpostuaMap : ClassMap<Lanpostua>
{
    public LanpostuaMap()
    {
        Table("Lanpostuak");
        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Izena);

        HasMany(x => x.Langilea)
            .KeyColumn("lanpostuak_id")
            .Inverse()
            .Cascade.All();
    }
}
