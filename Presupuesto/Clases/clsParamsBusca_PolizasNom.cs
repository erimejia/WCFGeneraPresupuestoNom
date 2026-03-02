using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class clsParamsBusca_PolizasNom
    {
        public string fechaIni { get; set; }//CreateDate
        public string fechaFin { get; set; }//CreateDate
        public string bachNumb { get; set; }//idPoliza
        public string refrence { get; set; }//REFRENCE
        public char? statusLayout { get; set; } //CtrlPPTO
        public bool? procesaConta { get; set; } //Active
    }
}
