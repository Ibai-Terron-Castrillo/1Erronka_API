using FluentNHibernate.Mapping;

public class OsagaiaHornitzaileaMap : ClassMap<OsagaiaHornitzailea>
{
    public OsagaiaHornitzaileaMap()
    {
        Table("Osagaiak_Hornitzaileak");

        Id(x => x.Id).GeneratedBy.Identity();

        References(x => x.Osagaia)
            .Column("Osagaiak_id")
            .Not.Nullable()
            .Insert()
            .Update();

        References(x => x.Hornitzailea)
            .Column("Hornitzaileak_id")
            .Not.Nullable()
            .Insert()
            .Update();
    }
}