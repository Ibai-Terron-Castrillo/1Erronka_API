using FluentNHibernate.Mapping;

public class OsagaiaMap : ClassMap<Osagaia>
{
    public OsagaiaMap()
    {
        Table("Osagaiak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Izena).Column("izena").Length(80).Not.Nullable();
        Map(x => x.AzkenPrezioa).Column("azken_prezioa").Not.Nullable();
        Map(x => x.Stock).Column("stock").Not.Nullable();
        Map(x => x.GutxienekoStock).Column("gutxieneko_stock");
        Map(x => x.Eskatu).Column("eskatu");

        HasManyToMany(x => x.Hornitzaileak)
            .Table("Osagaiak_Hornitzaileak")
            .ParentKeyColumn("Osagaiak_id")
            .ChildKeyColumn("Hornitzaileak_id")
            .Cascade.SaveUpdate();

        HasManyToMany(x => x.Platerak)
            .Table("Platerak_Osagaiak")
            .ParentKeyColumn("osagaiak_id")
            .ChildKeyColumn("platerak_id")
            .Cascade.SaveUpdate();
    }
}