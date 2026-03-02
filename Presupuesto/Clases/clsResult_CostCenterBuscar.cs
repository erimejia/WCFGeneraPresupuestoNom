using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class clsResult_CostCenterBuscar
    {
        public int ID_CC { get; set; }
        public string NUM { get; set; }
        public string CC { get; set; }
        public string DESCRIPCION { get; set; }
        public bool ESTATUS { get; set; }
        public string NAMEUPDATE { get; set; }
        public DateTime FECHAUPDATE { get; set; }
    }
}
