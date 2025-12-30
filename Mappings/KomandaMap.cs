using FluentNHibernate.Mapping;

public class KomandaMap : ClassMap<Komanda>
{
    public KomandaMap()
    {
        Table("Komandak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Kopurua).Column("kopurua").Not.Nullable();
        Map(x => x.Totala).Column("totala").Not.Nullable();
        Map(x => x.Oharrak).Column("oharrak").Length(65535);
        Map(x => x.Egoera).Column("egoera").Not.Nullable();

        References(x => x.Platerak)
            .Column("platerak_id")
            .Not.Nullable()
            .Insert()
            .Update();

        References(x => x.Faktura)
            .Column("fakturak_id")
            .Not.Nullable()
            .Insert()
            .Update();
    }
}