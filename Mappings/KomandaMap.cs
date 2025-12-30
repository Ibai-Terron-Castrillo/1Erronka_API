using FluentNHibernate.Mapping;

public class KomandaMap : ClassMap<Komanda>
{
    public KomandaMap()
    {
        Table("Komandak");

        CompositeId()
            .KeyProperty(x => x.Id, "id")
            .KeyReference(x => x.Platerak, "platerak_id")
            .KeyReference(x => x.Fakturak, "fakturak_id");

        Map(x => x.Kopurua).Column("kopurua").Not.Nullable();
        Map(x => x.Totala).Column("totala").Not.Nullable();
        Map(x => x.Oharrak).Column("oharrak").Length(65535);
        Map(x => x.Egoera).Column("egoera").Not.Nullable();

    }
}