using FluentNHibernate.Mapping;

public class KajaMap : ClassMap<Kaja>
{
    public KajaMap()
    {
        Table("Kaja");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Data).Column("data").Not.Nullable();
        Map(x => x.KajaHasiera).Column("kaja_hasiera").Not.Nullable();
        Map(x => x.KajaBukaera).Column("kaja_bukaera").Not.Nullable();
    }
}