using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Models;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Mapping;

namespace API.Mappings
{
    internal class OsagaiaMap : ClassMap<Osagaia>
    {
        public OsagaiaMap()
        {
            Table("Osagaiak");

            Id(x => x.Id).GeneratedBy.Identity();


            Map(x => x.Izena).Column("izena");
            Map(x => x.AzkenPrezioa).Column("azken_prezioa");
            Map(x => x.Stock).Column("stock");
            Map(x => x.GutxienekoStock).Column("gutxieneko_stock");
            Map(x => x.Eskatu).Column("eskatu");

            References(x => x.Eskaera)
                .Column("eskaerak_id")
                .Not.Nullable();
        }
    }
}
