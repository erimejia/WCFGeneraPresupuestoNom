using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    /// <summary>
    /// clsParams_ParameterGuarda - son todos los parámetros envíados al stored "PARAMETERS_AGREGA_ACTUALIZA" 
    /// para ser Actualizados o Insertados en la tabla CAT_PARAMETERS. 
    /// VALUEPARAM, contiene todos los valores de cada párametros de tipo string. Pero en el Front son convertidos a su tipo correspondiente.
    /// </summary>
    public class clsParams_ParameterGuarda
    {
        public short IDPARAM { get; set; }
        public string DESCRIBEPARAM { get; set; }
        public string VALUEPARAM { get; set; }
        public bool ESTATUS { get; set; }
        public string NAMEUPDATE { get; set; }
        public string IDUSER { get; set; }        
    }
}
