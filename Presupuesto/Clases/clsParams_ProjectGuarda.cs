using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    /// <summary>
    /// clsParams_ProjectGuarda - son todos los parámetros envíados al stored "PROJECT_AGREGA_ACTUALIZA"
    /// para ser Actualizados o Insertados en la tabla CAT_PROJECT
    /// </summary>
    public class clsParams_ProjectGuarda
    {
        public short IDPROJECT { get; set; }
        public string PROJECT { get; set; }
        public string DESCRIPCION { get; set; }
        public string NAMEUPDATE { get; set; }
        public bool ESTATUS { get; set; }
        public string IDUSER { get; set; }
    }
}
