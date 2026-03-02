using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    /// <summary>
    /// clsParams_UsuarioGuarda - son todos los parámetros envíados al stored "USUARIOS_AGREGA_ACTUALIZA" 
    /// para ser Actualizados o Insertados en la tabla CAT_USUARIOS
    /// </summary>
    public class clsParams_UsuarioGuarda
    {
        public string IDEMPLEADO { get; set; }
        public string NOMBRE { get; set; }
        public string EMAIL { get; set; }
        public short IDROL { get; set; }
        public bool ESTATUS { get; set; }
        public string NAMEUPDATE { get; set; }
        public string IDUSER { get; set; }
    }
}
