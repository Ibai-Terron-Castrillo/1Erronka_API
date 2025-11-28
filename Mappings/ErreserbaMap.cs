using FluentNHibernate.Mapping;

public class ErreserbaMap : ClassMap<Erreserbak>
{
    public ErreserbaMap()
    {
        Table("Erreserbak");
        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Izena);
        Map(x => x.Telefonoa);
        Map(x => x.Txanda);
        Map(x => x.PertsonaKopurua);
        Map(x => x.Data);

        HasMany(x => x.Fakturak)
            .KeyColumn("erreserbak_id")
            .Inverse()
            .Cascade.All();
    }
}
