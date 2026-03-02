using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    /// <summary>
    /// clsParams_SpendCategoryGuarda - son todos los parámetros envíados al stored "SPENDCATEGORY_AGREGA_ACTUALIZA" 
    /// para ser Actualizados o Insertados en la tabla CAT_SPENDCATEGORY
    /// </summary>
    public class clsParams_SpendCategoryGuarda
    {
        public int ID_SC { get; set; }
        public string SC { get; set; }
        public string DESCRIPCION { get; set; }
        public string NAMEUPDATE { get; set; }
        public bool ESTATUS { get; set; }
        public string IDUSER { get; set; }
    }
}
