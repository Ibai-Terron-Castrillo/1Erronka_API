using FluentNHibernate.Mapping;

public class EskaeraOsagaiaMap : ClassMap<EskaeraOsagaia>
{
    public EskaeraOsagaiaMap()
    {
        Table("Eskaerak_Osagaiak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Kopurua).Column("kopurua").Not.Nullable();
        Map(x => x.Prezioa).Column("prezioa").Not.Nullable();
        Map(x => x.Totala).Column("totala").Not.Nullable();

        References(x => x.Eskaera)
            .Column("eskaerak_id")
            .Not.Nullable()
            .Insert()
            .Update();

        References(x => x.Osagaia)
            .Column("osagaiak_id")
            .Not.Nullable()
            .Insert()
            .Update();
    }
}