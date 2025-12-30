using FluentNHibernate.Mapping;

public class HornitzaileaMap : ClassMap<Hornitzailea>
{
    public HornitzaileaMap()
    {
        Table("Hornitzaileak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Cif).Column("cif").Length(10).Not.Nullable();
        Map(x => x.Helbidea).Column("helbidea").Length(100).Not.Nullable();
        Map(x => x.Izena).Column("izena").Length(60).Not.Nullable();
        Map(x => x.Sektorea).Column("sektorea").Length(60).Not.Nullable();
        Map(x => x.Telefonoa).Column("telefonoa").Length(9).Not.Nullable();
        Map(x => x.Email).Column("email").Length(60).Not.Nullable();

        HasManyToMany(x => x.Osagaiak)
            .Table("Osagaiak_Hornitzaileak")
            .ParentKeyColumn("Hornitzaileak_id")
            .ChildKeyColumn("Osagaiak_id")
            .Inverse()
            .Cascade.SaveUpdate();
    }
}