using FluentNHibernate.Mapping;

public class PlaterakOsagaiaMap : ClassMap<PlaterakOsagaia>
{
    public PlaterakOsagaiaMap()
    {
        Table("Platerak_Osagaiak");

        CompositeId()
            .KeyProperty(x => x.Id, "id")
            .KeyReference(x => x.Osagaia, "osagaiak_id")
            .KeyReference(x => x.Platerak, "platerak_id");

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