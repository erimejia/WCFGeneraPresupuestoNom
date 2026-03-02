using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class clsResult_CuentaNomBuscar
    {
        public int jrnlidx { get; set; }
        public string Num { get; set; }
        public string idPoliza { get; set; }
        public string CuentaGP { get; set; }
        public string CuentaWD { get; set; }
        public decimal Debito { get; set; }
        public string Refrence { get; set; }
        public DateTime FechaPoliza { get; set; }
        public bool ProcesadoConta { get; set; }
        public string EstausLayout { get; set; }
        public DateTime FechaUpdate { get; set; }
        public bool Anulada { get; set; }
    }
}
