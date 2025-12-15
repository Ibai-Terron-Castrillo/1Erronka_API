using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models
{
    internal class Osagaia
    {
        public Osagaia() { }
        public virtual int Id { get; set; }
        public virtual string Izena { get; set; }
        public virtual double AzkenPrezioa { get; set; }
        public virtual int Stock { get; set; }
        public virtual int GutxienekoStock { get; set; }
        public virtual bool Eskatu { get; set; }

        public virtual Eskaera Eskaera { get; set; }
    }
}
