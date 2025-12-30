using FluentNHibernate.Mapping;

public class OsagaiaHornitzaileaMap : ClassMap<OsagaiaHornitzailea>
{
    public OsagaiaHornitzaileaMap()
    {
        Table("Osagaiak_Hornitzaileak");

        CompositeId()
            .KeyProperty(x => x.Id, "id")
            .KeyReference(x => x.Osagaia, "Osagaiak_id")
            .KeyReference(x => x.Hornitzailea, "Hornitzaileak_id");

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