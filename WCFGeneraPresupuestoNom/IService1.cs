using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WCFGeneraPresupuestoNom
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        string Catalogo(string tipoObjeto, string metodo, string parametros);

        [OperationContract]
        string Acceso(string tipoObjeto, string metodo, string parametros);

        [OperationContract]
        string Operacion(string tipoObjeto, string metodo, string parametros);

        [OperationContract]
        string Reportes(string tipoObjeto, string metodo, string parametros);
    }
}
