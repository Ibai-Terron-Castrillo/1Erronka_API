using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models
{
    internal class Eskaera
    {
        public int Id { get; set; }
        public int Totala { get; set; }
        public bool Egoera { get; set; }
        public string EskaeraPDF { get; set; }

        public virtual Osagaia Osagaia { get; set; }
    }
}
