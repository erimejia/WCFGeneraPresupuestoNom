using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Udla.Exceptions;
using Presupuesto.Clases;
using Presupuesto.Entidades;

namespace Presupuesto.Logica
{
    public class LAcceso
    {
        UDLAPSIFEntities dbSIF = new UDLAPSIFEntities();

        //clsPermisosAccesos
        public clsResultado validaAcceso(string idUsuario, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from r in dbSIF.UDLAPSIF_PTONOM_VALIDAACCESO(idUsuario)
                            select new clsResult_ValidaAcceso
                            {
                                IDROL = r.IDROL,
                                NOMBREROL = r.NOMBREROL,
                                IDEMPLEADO = r.IDEMPLEADO,
                                NOMBRE = r.NOMBRE
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "validaAcceso", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }



    }
}
