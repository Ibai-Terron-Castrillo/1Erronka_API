using FluentNHibernate.Mapping;

public class EskaeraMap : ClassMap<Eskaera>
{
    public EskaeraMap()
    {
        Table("Eskaerak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.EskaeraZenbakia).Column("eskaera_zenbakia").Not.Nullable();
        Map(x => x.Totala).Column("totala").Not.Nullable();
        Map(x => x.Egoera).Column("egoera").Not.Nullable();
        Map(x => x.EskaeraPdf).Column("eskaera_pdf").Length(100).Not.Nullable();

        HasManyToMany(x => x.Osagaiak)
            .Table("Eskaerak_Osagaiak")
            .ParentKeyColumn("eskaerak_id")
            .ChildKeyColumn("osagaiak_id")
            .Cascade.SaveUpdate()
            .LazyLoad();
    }
}