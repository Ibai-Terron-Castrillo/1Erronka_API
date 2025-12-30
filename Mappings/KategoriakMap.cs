using FluentNHibernate.Mapping;

public class KategoriakMap : ClassMap<Kategoriak>
{
    public KategoriakMap()
    {
        Table("Kategoriak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Izena).Column("izena").Length(60);

        HasMany(x => x.Platerak)
            .KeyColumn("Kategoriak_id")
            .Inverse()
            .Cascade.AllDeleteOrphan();
    }
}