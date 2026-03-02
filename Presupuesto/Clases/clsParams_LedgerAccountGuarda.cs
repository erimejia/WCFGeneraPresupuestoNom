using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    /// <summary>
    /// clsParams_LedgerAccountGuarda - son todos los parámetros envíados al stored "LEDGERACCOUNT_AGREGA_ACTUALIZA" 
    /// para ser Actualizados o Insertados en la tabla CAT_LEDGERACCOUNT
    /// </summary>
    public class clsParams_LedgerAccountGuarda
    {
        public int ID_LA { get; set; }
        public string LA { get; set; }
        public string DESCRIPCION { get; set; }
        public string NAMEUPDATE { get; set; }
        public bool ESTATUS { get; set; }
        public string IDUSER { get; set; }
    }
}
