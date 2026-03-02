using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    /// <summary>
    /// clsParams_CostCenterGuarda - son todos los parámetros envíados al stored "COSTCENTER_AGREGA_ACTUALIZA" 
    /// para ser Actualizados o Insertados en la tabla CAT_COSTCENTER
    /// </summary>
    public class clsParams_CostCenterGuarda
    {
        public int ID_CC { get; set; }
        public string CC { get; set; }
        public string DESCRIPCION { get; set; }
        public string NAMEUPDATE { get; set; }
        public bool ESTATUS { get; set; }
        public string IDUSER { get; set; }
    }
}
