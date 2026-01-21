using FluentNHibernate.Mapping;

public class PlaterakMap : ClassMap<Platerak>
{
    public PlaterakMap()
    {
        Table("Platerak");

        Id(x => x.Id).GeneratedBy.Identity();
        Map(x => x.Izena).Column("izena").Length(45).Not.Nullable();
        Map(x => x.Prezioa).Column("prezioa").Not.Nullable();
        Map(x => x.Stock).Column("stock").Not.Nullable();

        References(x => x.Kategoriak)
            .Column("Kategoriak_id")
            .Not.Nullable()
            .Fetch.Join();

        HasManyToMany(x => x.Osagaiak)
            .Table("Platerak_Osagaiak")
            .ParentKeyColumn("platerak_id")
            .ChildKeyColumn("osagaiak_id")
            .Cascade.AllDeleteOrphan()
            .LazyLoad();
    }
}