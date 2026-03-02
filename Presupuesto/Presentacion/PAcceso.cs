using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Presupuesto.Clases;
using Presupuesto.Logica;

namespace Presupuesto.Presentacion
{
    public class PAcceso
    {
        LAcceso a = new LAcceso();
        //clsPermisosAccesos
        public clsResultado validaAcceso(string idUsuario, string rutaLog)
        {
            return a.validaAcceso(idUsuario, rutaLog);
        }
    }
}
