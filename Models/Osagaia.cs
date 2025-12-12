using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models
{
    internal class Osagaia
    {
        public int Id { get; set; }
        public string Izena { get; set; }
        public double AzkenPrezioa { get; set; }
        public int Stock { get; set; }
        public int GutxienekoStock { get; set; }
        public bool Eskatu { get; set; }

        public virtual Eskaera Eskaera { get; set; }
    }
}
