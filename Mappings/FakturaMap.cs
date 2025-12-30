using FluentNHibernate.Mapping;

public class FakturaMap : ClassMap<Faktura>
{
    public FakturaMap()
    {
        Table("Fakturak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Totala).Column("totala").Not.Nullable();
        Map(x => x.Egoera).Column("egoera").Not.Nullable();
        Map(x => x.FakturaPdf).Column("faktura_pdf").Length(70);

        References(x => x.Erreserba)
            .Column("erreserbak_id")
            .Not.Nullable();

        HasMany(x => x.Komandak)
            .KeyColumn("fakturak_id")
            .Inverse()
            .Cascade.AllDeleteOrphan();
    }
}