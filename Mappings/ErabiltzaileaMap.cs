using FluentNHibernate.Mapping;

public class ErabiltzaileaMap : ClassMap<Erabiltzailea>
{
    public ErabiltzaileaMap()
    {
        Table("Erabiltzaileak");
        Id(x => x.Id).GeneratedBy.Identity();
        Map(x => x.LangileId).Column("langileak_id");
        Map(x => x.Izena).Column("erabiltzailea");
        Map(x => x.Pasahitza).Column("pasahitza");
    }
}
