using FluentNHibernate.Mapping;

public class ErreserbaMap : ClassMap<Erreserba>
{
    public ErreserbaMap()
    {
        Table("Erreserbak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Izena).Column("izena").Length(60);
        Map(x => x.Telefonoa).Column("telefonoa");
        Map(x => x.Txanda).Column("txanda").Length(20);
        Map(x => x.PertsonaKopurua).Column("pertsona_kopurua");
        Map(x => x.Data).Column("data");

        HasOne(x => x.Faktura)
            .PropertyRef(r => r.Erreserba)
            .Cascade.All();

        HasManyToMany(x => x.Mahaiak)
            .Table("Erreserbak_Mahaiak")
            .ParentKeyColumn("erreserbak_id")
            .ChildKeyColumn("mahaiak_id")
            .Cascade.SaveUpdate();
    }
}