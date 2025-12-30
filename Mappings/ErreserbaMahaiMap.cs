using FluentNHibernate.Mapping;

public class ErreserbaMahaiMap : ClassMap<ErreserbaMahai>
{
    public ErreserbaMahaiMap()
    {
        Table("Erreserbak_Mahaiak");

        Id(x => x.Id).GeneratedBy.Identity();

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