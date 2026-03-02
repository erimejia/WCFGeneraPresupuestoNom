using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class clsResult_UsuarioBuscar
    {
        public string NUM { get; set; }
        public short IDROL { get; set; }
        public string NOMBREROL { get; set; }
        public string IDEMPLEADO { get; set; }
        public string NOMBRE { get; set; }
        public string EMAIL { get; set; }
        public string NAMEUPDATE { get; set; }
        public DateTime FECHAUPDATE { get; set; }
        public bool ESTATUS { get; set; }
    }
}
