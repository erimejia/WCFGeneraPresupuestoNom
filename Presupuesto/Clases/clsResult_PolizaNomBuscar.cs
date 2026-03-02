using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class clsResult_PolizaNomBuscar
    {
        public string Num { get; set; }
        public bool ProcesadoConta { get; set; }
        public string EstausLayout { get; set; }
        public string idPoliza{ get; set; }
        public bool errorPoliza { get; set; }
        public string Refrence { get; set; }
        public DateTime FechaPoliza { get; set; }
        public decimal TotalDebito { get; set; }
        public int TotalInstrucciones { get; set; }
    }
}
