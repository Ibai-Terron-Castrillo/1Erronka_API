using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

public static class NHibernateHelper
{
    private static ISessionFactory _sessionFactory;

    public static ISessionFactory SessionFactory =>
        _sessionFactory ?? (_sessionFactory = CreateSessionFactory());

    private static ISessionFactory CreateSessionFactory()
    {
        return Fluently.Configure()
            .Database(MySQLConfiguration.Standard
                .ConnectionString("Server=192.168.115.162;Database=erronka1;Uid=2Taldea;Pwd=2Taldea2;"))
            .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Erabiltzailea>())
            .BuildSessionFactory();
    }

    public static ISession OpenSession() => SessionFactory.OpenSession();
}
