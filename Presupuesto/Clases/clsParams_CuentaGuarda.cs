using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    /// <summary>
    /// clsParams_CuentaGuarda - son todos los parámetros envíados al stored "CUENTAS_AGREGA_ACTUALIZA" 
    /// para ser Actualizados o Insertados en la tabla CAT_CUENTAS
    /// </summary>
    public class clsParams_CuentaGuarda
    {        
        public int ID_CUENTA { get; set; }
        public string CUENTA { get; set; }
        public string DESCRIPCION { get; set; }
        public int ID_CC { get; set; }
        public int ID_SC { get; set; }
        public int ID_LA { get; set; }
        public short IDCURRENCY { get; set; }
        public short IDTIPOCUENTA { get; set; }
        public short IDPROJECT { get; set; }
        public bool CUENTAESPECIAL { get; set; }
        public bool VALES { get; set; }
        public bool ESTATUS { get; set; }
        public string NAMEUPDATE { get; set; }        
        public string IDUSER { get; set; }
    }
}
