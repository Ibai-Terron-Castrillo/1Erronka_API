using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models
{
    internal class Eskaera
    {
        public Eskaera() { }
        public virtual int Id { get; set; }
        public virtual int Totala { get; set; }
        public virtual bool Egoera { get; set; }
        public virtual string EskaeraPDF { get; set; }

        public virtual Osagaia Osagaia { get; set; }
    }
}
