using FluentNHibernate.Mapping;

public class MahaiMap : ClassMap<Mahai>
{
    public MahaiMap()
    {
        Table("Mahaiak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.MahaiZenbakia).Column("mahai_zenbakia").Not.Nullable();

        HasManyToMany(x => x.Erreserbak)
            .Table("Erreserbak_Mahaiak")
            .ParentKeyColumn("mahaiak_id")
            .ChildKeyColumn("erreserbak_id")
            .Inverse()
            .Cascade.SaveUpdate();
    }
}