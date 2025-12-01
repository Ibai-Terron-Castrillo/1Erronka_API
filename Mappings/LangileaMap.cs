using FluentNHibernate.Mapping;

public class LangileaMap : ClassMap<Langilea>
{
    public LangileaMap()
    {
        Table("Langileak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Izena).Column("izena");
        Map(x => x.Abizena1).Column("abizena1");
        Map(x => x.Abizena2).Column("abizena2");
        Map(x => x.Telefonoa).Column("telefonoa");

        References(x => x.Lanpostua)
            .Column("lanpostuak_id")
            .Not.Nullable();
    }
}
