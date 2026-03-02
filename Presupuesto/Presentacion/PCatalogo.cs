using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Presupuesto.Clases;
using Presupuesto.Logica;

namespace Presupuesto.Presentacion
{
    public class PCatalogo
    {
        LCatalogo c = new LCatalogo();

        #region clsCostCenter
        public clsResultado buscarCC(int? id_cc, string cc, string descripcion, bool? estatus, string rutaLog)
        {
            return c.buscarCC(id_cc, cc, descripcion, estatus, rutaLog);
        }

        public clsResultado obtenCC(string rutaLog)//obtiene todos los CostCenter activos
        {
            return c.obtenCC(rutaLog);
        }

        public clsResultado guardarCC(clsParams_CostCenterGuarda costCenter, string rutaLog)
        {
            return c.guardarCC(costCenter, rutaLog);
        }
        #endregion

        #region clsLedgerAccount
        public clsResultado buscarLA(int? id_la, string la, string descripcion, bool? estatus, string rutaLog)
        {
            return c.buscarLA(id_la, la, descripcion, estatus, rutaLog);
        }

        public clsResultado obtenLA(string rutaLog)//obtiene todos los Ledger Account activos
        {
            return c.obtenLA(rutaLog);
        }

        public clsResultado guardarLA(clsParams_LedgerAccountGuarda ledgerAccount, string rutaLog)
        {
            return c.guardarLA(ledgerAccount, rutaLog);
        }
        #endregion

        #region clsSpendCategory
        public clsResultado buscarSC(int? id_sc, string sc, string descripcion, bool? estatus, string rutaLog)
        {
            return c.buscarSC(id_sc, sc, descripcion, estatus, rutaLog);
        }

        public clsResultado obtenSC(string rutaLog)//obtiene todos los Spend Category activos
        {
            return c.obtenSC(rutaLog);
        }

        public clsResultado guardarSC(clsParams_SpendCategoryGuarda spendCategory, string rutaLog)
        {
            return c.guardarSC(spendCategory, rutaLog);
        }
        #endregion

        #region clsCuentas
        public clsResultado buscarC(int? id_cuenta, string cuenta, string descripcion, bool? estatus, string cc, int? id_sc, int? id_la, string rutaLog)
        {
            return c.buscarC(id_cuenta, cuenta, descripcion, estatus, cc, id_sc, id_la, rutaLog);
        }
        public clsResultado guardarC(clsParams_CuentaGuarda cuentas, string rutaLog)//inserta o actualiza cuentas contables de workday
        {
            return c.guardarC(cuentas, rutaLog);
        }
        public clsResultado obtenTipoCuenta(string rutaLog)//obtiene todos los Tipo de Cuenta activos
        {
            return c.obtenTipoCuenta(rutaLog);
        }
        public clsResultado obtenCurrency(string rutaLog)//obtiene todas las Monedas activas
        {
            return c.obtenCurrency(rutaLog);
        }
        #endregion

        #region clsProject
        public clsResultado buscarPR(short? id_Project, string project, string descripcion, bool? estatus, string rutaLog)
        {
            return c.buscarPR(id_Project, project, descripcion, estatus, rutaLog);
        }

        public clsResultado obtenPR(string rutaLog)//obtiene todos los Proyectos activos
        {
            return c.obtenPR(rutaLog);
        }

        public clsResultado guardarPR(clsParams_ProjectGuarda project, string rutaLog)
        {
            return c.guardarPR(project, rutaLog);
        }
        #endregion

        #region clsParameters
        public clsResultado buscarParameter(short idparam, string rutaLog)
        {
            return c.buscarParameter(idparam, rutaLog);
        }
        public clsResultado obtenParameters(string rutaLog)
        {
            return c.obtenParameters(rutaLog);
        }
        public clsResultado guardarParameter(clsParams_ParameterGuarda param, string rutaLog)
        {
            return c.guardarParameter(param, rutaLog);
        }

        #endregion

        #region clsUsuario
        public clsResultado buscarUsuario(string idEmpleado, string nombre, short? idRol, bool? estatus, string rutaLog)
        {
            return c.buscarUsuario(idEmpleado, nombre, idRol, estatus, rutaLog);
        }
        public clsResultado obtenRol(string rutaLog)
        {
            return c.obtenRol(rutaLog);
        }
        public clsResultado guardarUsuario(clsParams_UsuarioGuarda usuario, string rutaLog)
        {
            return c.guardarUsuario(usuario, rutaLog);
        }
        #endregion

        #region ActiveDirectory
        //metodos de busqueda de usuarios para darlos de alta en el sistema. Desde el popup de creacion de usuario de sistema
        public clsResultado obtenerUsuarioADPorID(string IdUsuario, string cadenaConexionAD, string rutaLog) //busca a un responsable por su ID de empleado
        {
            return c.obtenerUsuarioADPorID(IdUsuario, cadenaConexionAD, rutaLog);
        }
        public clsResultado obtenerUsuarioADPorNombre(string nombre, string cadenaConexionAD, string rutaLog) //busca a un responsable por su NOMBRE de empleado
        {
            return c.obtenerUsuarioADPorNombre(nombre, cadenaConexionAD, rutaLog);
        }
        #endregion
        
    }
}
