using FluentNHibernate.Mapping;

public class ErabiltzaileaMap : ClassMap<Erabiltzailea>
{
    public ErabiltzaileaMap()
    {
        Table("Erabiltzaileak");

        Id(x => x.Id).GeneratedBy.Identity();

        Map(x => x.Izena).Column("erabiltzailea");
        Map(x => x.Pasahitza).Column("pasahitza");

        References(x => x.Langilea)
            .Column("langileak_id")
            .Not.Nullable();
    }
}
