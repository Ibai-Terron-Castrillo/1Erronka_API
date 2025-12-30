using FluentNHibernate.Mapping;

public class PlaterakOsagaiaMap : ClassMap<PlaterakOsagaia>
{
    public PlaterakOsagaiaMap()
    {
        Table("Platerak_Osagaiak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Kopurua).Column("kopurua").Not.Nullable();

        References(x => x.Osagaia)
            .Column("osagaiak_id")
            .Not.Nullable()
            .Insert()
            .Update();

        References(x => x.Platerak)
            .Column("platerak_id")
            .Not.Nullable()
            .Insert()
            .Update();
    }
}