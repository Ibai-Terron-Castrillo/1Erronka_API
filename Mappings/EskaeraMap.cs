using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Models;

namespace API.Mappings
{
    internal class EskaeraMap : ClassMap<Eskaera>
    {
        public EskaeraMap() {

            Table("Eskaerak");

            Id(x => x.Id).GeneratedBy.Identity();

            Map(x => x.Totala).Column("totala");
            Map(x => x.Egoera).Column("egoera");
            Map(x => x.EskaeraPDF).Column("eskaera_pdf");

            References(x => x.Osagaia)
                .Column("osagaiak_id")
                .Not.Nullable();

        }
    }
}
