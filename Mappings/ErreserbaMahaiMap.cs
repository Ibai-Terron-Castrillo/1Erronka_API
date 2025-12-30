using FluentNHibernate.Mapping;

public class ErreserbaMahaiMap : ClassMap<ErreserbaMahai>
{
    public ErreserbaMahaiMap()
    {
        Table("Erreserbak_Mahaiak");

        CompositeId()
            .KeyProperty(x => x.Id, "id")
            .KeyReference(x => x.Erreserba, "erreserbak_id")
            .KeyReference(x => x.Mahai, "mahaiak_id");

        References(x => x.Erreserba)
            .Column("erreserbak_id")
            .Not.Nullable()
            .Insert()
            .Update();

        References(x => x.Mahai)
            .Column("mahaiak_id")
            .Not.Nullable()
            .Insert()
            .Update();
    }
}