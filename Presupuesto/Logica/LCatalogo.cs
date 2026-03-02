using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.DirectoryServices;
using System.Globalization;
using System.Configuration;

using Newtonsoft.Json;
using Udla.Exceptions;
using Presupuesto.Clases;
using Presupuesto.Entidades;

namespace Presupuesto.Logica
{
    public class LCatalogo
    {
        UDLAPSIFEntities dbSIF = new UDLAPSIFEntities();

        #region clsCostCenter
        public clsResultado buscarCC(int? id_cc, string cc, string descripcion, bool? estatus, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from c in dbSIF.UDLAPSIF_PTONOM_COSTCENTER_BUSCARCC(id_cc, cc, descripcion, estatus)
                            select new clsResult_CostCenterBuscar
                            {
                                ID_CC = c.ID_CC,
                                NUM = c.NUM,
                                CC = c.CC,
                                DESCRIPCION = c.DESCRIPCION,
                                ESTATUS = c.ESTATUS,
                                NAMEUPDATE = c.NAMEUPDATE,
                                FECHAUPDATE = c.FECHAUPDATE
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "buscarCC", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado obtenCC(string rutaLog) //obtiene todos los CostCenter activos
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from cc in dbSIF.UDLAPSIF_PTONOM_COSTCENTER_OBTENCC()
                            select new clsObtenCostCenter
                            {
                                ID_CC = cc.ID_CC,
                                CC = cc.CC,
                                DESCRIPCION = cc.DESCRIPCION
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "obtenCC", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado guardarCC(clsParams_CostCenterGuarda costCenter, string rutaLog)
        {
            clsResultado resultado = new clsResultado();
            resultado.Resultado = "Indefinido";

            try
            {
                var query = from c in dbSIF.UDLAPSIF_PTONOM_COSTCENTER_AGREGA_ACTUALIZA
                                                                       (costCenter.ID_CC,
                                                                        costCenter.CC,
                                                                        costCenter.DESCRIPCION,
                                                                        costCenter.NAMEUPDATE,
                                                                        costCenter.ESTATUS,
                                                                        costCenter.IDUSER)
                            select new clsResult_Error
                            {
                                CLAVEERROR = c.CLAVEERROR, //valor int devuelto. ID_CC o el numero del error 
                                DESCRIBEERROR = c.DESCRIBEERROR //la descripción del error o del update/inserción exitosa
                            };
                resultado.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault()); //Resultado con el codigo de error o confirmacion de insert/update desde el stored procedure
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "guardarCC", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                resultado.Resultado = "Error";
            }
            return resultado;
        }
        #endregion
        
        #region clsLedgerAccount
        public clsResultado buscarLA(int? id_la, string la, string descripcion, bool? estatus, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from l in dbSIF.UDLAPSIF_PTONOM_LEDGERACCOUNT_BUSCARLA(id_la, la, descripcion, estatus)
                            select new clsResult_LedgerAccountBuscar
                            {
                                ID_LA = l.ID_LA,
                                NUM = l.NUM,
                                LA = l.LA,
                                DESCRIPCION = l.DESCRIPCION,
                                ESTATUS = l.ESTATUS,
                                NAMEUPDATE = l.NAMEUPDATE,
                                FECHAUPDATE = l.FECHAUPDATE
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "buscarLA", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado obtenLA(string rutaLog) //obtiene todos los Ledger Account activos
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from la in dbSIF.UDLAPSIF_PTONOM_LEDGERACCOUNT_OBTENLA()
                            select new clsObtenLedgerAccount
                            {
                                ID_LA = la.ID_LA,
                                LA = la.LA,
                                DESCRIPCION = la.DESCRIPCION
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "obtenLA", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado guardarLA(clsParams_LedgerAccountGuarda ledgerAccount, string rutaLog)
        {
            clsResultado resultado = new clsResultado();
            resultado.Resultado = "Indefinido";

            try
            {
                var query = from l in dbSIF.UDLAPSIF_PTONOM_LEDGERACCOUNT_AGREGA_ACTUALIZA
                                                                       (ledgerAccount.ID_LA,
                                                                        ledgerAccount.LA,
                                                                        ledgerAccount.DESCRIPCION,
                                                                        ledgerAccount.NAMEUPDATE,
                                                                        ledgerAccount.ESTATUS,
                                                                        ledgerAccount.IDUSER)
                            select new clsResult_Error
                            {
                                CLAVEERROR = l.CLAVEERROR, //valor int devuelto. ID_LA o el numero del error 
                                DESCRIBEERROR = l.DESCRIBEERROR //la descripción del error o del update/inserción exitosa
                            };
                resultado.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault()); //Resultado con el codigo de error o confirmacion de insert/update desde el stored procedure
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "guardarLA", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                resultado.Resultado = "Error";
            }
            return resultado;
        }
        #endregion

        #region clsSpendCategory
        public clsResultado buscarSC(int? id_sc, string sc, string descripcion, bool? estatus, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from s in dbSIF.UDLAPSIF_PTONOM_SPENDCATEGORY_BUSCARSC(id_sc, sc, descripcion, estatus)
                            select new clsResult_SpendCategoryBuscar
                            {
                                ID_SC = s.ID_SC,
                                NUM = s.NUM,
                                SC = s.SC,
                                DESCRIPCION = s.DESCRIPCION,
                                ESTATUS = s.ESTATUS,
                                NAMEUPDATE = s.NAMEUPDATE,
                                FECHAUPDATE = s.FECHAUPDATE
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "buscarSC", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado obtenSC(string rutaLog)//obtiene todos los Spend Category activos
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from sc in dbSIF.UDLAPSIF_PTONOM_SPENDCATEGORY_OBTENSC()
                            select new clsObtenSpendCategory
                            {
                                ID_SC = sc.ID_SC,
                                SC = sc.SC,
                                DESCRIPCION = sc.DESCRIPCION
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "obtenSC", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado guardarSC(clsParams_SpendCategoryGuarda spendCategory, string rutaLog)
        {
            clsResultado resultado = new clsResultado();
            resultado.Resultado = "Indefinido";

            try
            {
                var query = from s in dbSIF.UDLAPSIF_PTONOM_SPENDCATEGORY_AGREGA_ACTUALIZA
                                                                       (spendCategory.ID_SC,
                                                                        spendCategory.SC,
                                                                        spendCategory.DESCRIPCION,
                                                                        spendCategory.NAMEUPDATE,
                                                                        spendCategory.ESTATUS,
                                                                        spendCategory.IDUSER)
                            select new clsResult_Error
                            {
                                CLAVEERROR = s.CLAVEERROR, //valor int devuelto. ID_LA o el numero del error 
                                DESCRIBEERROR = s.DESCRIBEERROR //la descripción del error o del update/inserción exitosa
                            };
                resultado.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault()); //Resultado con el codigo de error o confirmacion de insert/update desde el stored procedure
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "guardarSC", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                resultado.Resultado = "Error";
            }
            return resultado;
        }
        #endregion

        #region clsCuentas
        public clsResultado buscarC(int? id_cuenta, string cuenta, string descripcion, bool? estatus, string cc, int? id_sc, int? id_la, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from c in dbSIF.UDLAPSIF_PTONOM_CUENTAS_BUSCARC(id_cuenta, cuenta, descripcion, estatus, cc, id_sc, id_la)
                            select new clsResult_CuentaBuscar
                            {
                                ID_CUENTA = c.ID_CUENTA,
                                NUM = c.NUM,
                                CUENTA = c.CUENTA,
                                DESCRIPCION = c.DESCRIPCION,

                                //COST-CENTER
                                ID_CC = c.ID_CC,
                                COSTCENTER = c.COSTCENTER,
                                //SPEND-CATEGORY
                                ID_SC = c.ID_SC,
                                SPENDCATEGORY = c.SPENDCATEGORY,
                                //LEDGER-ACCOUNT
                                ID_LA = c.ID_LA,
                                LEDGERACCOUNT = c.LEDGERACCOUNT,
                                //CURRENCY
                                IDCURRENCY = c.IDCURRENCY,
                                CODECURRENCY = c.CODECURRENCY,
                                //TIPO-CUENTA
                                IDTIPOCUENTA = c.IDTIPOCUENTA,
                                TIPOCUENTA = c.TIPOCUENTA,
                                //PROJECT
                                IDPROJECT = c.IDPROJECT,
                                PROJECT = c.PROJECT,
                                //CUENTAESPECIAL
                                CUENTAESPECIAL = c.CUENTAESPECIAL,
                                //VALES
                                VALES = c.VALES,

                                ESTATUS = c.ESTATUS,
                                NAMEUPDATE = c.NAMEUPDATE,
                                FECHAUPDATE = c.FECHAUPDATE
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "buscarC", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado guardarC(clsParams_CuentaGuarda cuentas, string rutaLog) //inserta o actualiza cuentas contables de workday
        {
            clsResultado resultado = new clsResultado();
            resultado.Resultado = "Indefinido";

            try
            {
                var query = from c in dbSIF.UDLAPSIF_PTONOM_CUENTAS_AGREGA_ACTUALIZA
                                                                       (cuentas.ID_CUENTA,
                                                                        cuentas.CUENTA,
                                                                        cuentas.DESCRIPCION,
                                                                        cuentas.ID_CC,
                                                                        cuentas.ID_SC,
                                                                        cuentas.ID_LA,
                                                                        cuentas.IDCURRENCY,
                                                                        cuentas.IDTIPOCUENTA,
                                                                        cuentas.IDPROJECT,
                                                                        cuentas.CUENTAESPECIAL,
                                                                        cuentas.VALES,
                                                                        cuentas.ESTATUS,
                                                                        cuentas.NAMEUPDATE,
                                                                        cuentas.IDUSER)
                            select new clsResult_Error
                            {
                                CLAVEERROR = c.CLAVEERROR, //valor int devuelto. ID_CUENTA o el numero del error 
                                DESCRIBEERROR = c.DESCRIBEERROR //la descripción del error o del update/inserción exitosa
                            };
                resultado.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault()); //Resultado con el codigo de error o confirmacion de insert/update desde el stored procedure
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "guardarC", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                resultado.Resultado = "Error";
            }
            return resultado;
        }

        public clsResultado obtenTipoCuenta(string rutaLog)//obtiene todos los Tipo de Cuenta activos
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from t in dbSIF.UDLAPSIF_PTONOM_TIPOCUENTA_OBTENTC()
                            select new clsObtenTipoCuenta
                            {
                                IDTIPOCUENTA = t.IDTIPOCUENTA,
                                TIPO = t.TIPO,
                                DESCRIPCION = t.DESCRIPCION,
                                CODE = t.CODE
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "obtenTipoCuenta", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado obtenCurrency(string rutaLog)//obtiene todas las Monedas activas
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from c in dbSIF.UDLAPSIF_PTONOM_CURRENCY_OBTENCURRENCY()
                            select new clsObtenCurrency
                            {
                                IDCURRENCY = c.IDCURRENCY,
                                COUNTRY = c.COUNTRY,
                                CURRENCY = c.CURRENCY,
                                CODE = c.CODE
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "obtenCurrency", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        #endregion

        #region clsProject
        public clsResultado buscarPR(short? id_Project, string project, string descripcion, bool? estatus, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from pr in dbSIF.UDLAPSIF_PTONOM_PROJECT_BUSCARPR(id_Project, project, descripcion, estatus)
                            select new clsResult_ProjectBuscar
                            {
                                IDPROJECT = pr.IDPROJECT,
                                NUM = pr.NUM,
                                PROJECT = pr.PROJECT,
                                DESCRIPCION = pr.DESCRIPCION,
                                ESTATUS = pr.ESTATUS,
                                NAMEUPDATE = pr.NAMEUPDATE,
                                FECHAUPDATE = pr.FECHAUPDATE
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "buscarPR", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado obtenPR(string rutaLog) //obtiene todos los Proyectos activos
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from pr in dbSIF.UDLAPSIF_PTONOM_PROJECT_OBTENPR()
                            select new clsObtenProject
                            {
                                IDPROJECT = pr.IDPROJECT,
                                PROJECT = pr.PROJECT,
                                DESCRIPCION = pr.DESCRIPCION
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "obtenPR", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado guardarPR(clsParams_ProjectGuarda project, string rutaLog)
        {
            clsResultado resultado = new clsResultado();
            resultado.Resultado = "Indefinido";

            try
            {
                var query = from pr in dbSIF.UDLAPSIF_PTONOM_PROJECT_AGREGA_ACTUALIZA
                                                                       (project.IDPROJECT,
                                                                        project.PROJECT,
                                                                        project.DESCRIPCION,
                                                                        project.NAMEUPDATE,
                                                                        project.ESTATUS,
                                                                        project.IDUSER)
                            select new clsResult_Error
                            {
                                CLAVEERROR = pr.CLAVEERROR, //valor short devuelto IDPROJECT o el numero del error 
                                DESCRIBEERROR = pr.DESCRIBEERROR //la descripción del error o del update/inserción exitosa
                            };
                resultado.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault()); //Resultado con el codigo de error o confirmacion de insert/update desde el stored procedure
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "guardarPR", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                resultado.Resultado = "Error";
            }
            return resultado;
        }
        #endregion

        #region clsParameters
        public clsResultado buscarParameter(short idparam, string rutaLog)//busca un parametro por el idparam
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from p in dbSIF.UDLAPSIF_PTONOM_PARAMETER_BUSCARP(idparam)
                            select new clsResult_ParameterBuscar
                            {
                                IDPARAM = p.IDPARAM,
                                DESCRIBEPARAM = p.DESCRIBEPARAM,
                                VALUEPARAM = p.VALUEPARAM,
                                NAMEUPDATE = p.NAMEUPDATE,
                                FECHAUPDATE = p.FECHAUPDATE,

                            };
                datos.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "buscarParameter", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado obtenParameters(string rutaLog)//obtiene todos los Parámetros activos
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from p in dbSIF.UDLAPSIF_PTONOM_PARAMETERS_OBTENP()
                            select new clsObtenParameters
                            {
                                IDPARAM = p.IDPARAM,
                                DESCRIBEPARAM = p.DESCRIBEPARAM,
                                VALUEPARAM = p.VALUEPARAM,
                                NAMEUPDATE = p.NAMEUPDATE,
                                FECHAUPDATE = p.FECHAUPDATE,

                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "obtenParameters", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado guardarParameter(clsParams_ParameterGuarda param, string rutaLog)
        {
            clsResultado resultado = new clsResultado();
            resultado.Resultado = "Indefinido";

            try
            {
                var query = from u in dbSIF.UDLAPSIF_PTONOM_PARAMETERS_AGREGA_ACTUALIZA
                                                                       (param.IDPARAM,
                                                                       param.DESCRIBEPARAM,
                                                                       param.VALUEPARAM,
                                                                       param.ESTATUS,
                                                                       param.NAMEUPDATE,
                                                                       param.IDUSER)
                            select new clsResult_Error
                            {
                                CLAVEERROR = u.CLAVEERROR, //valor int devuelto. IDEMPLEADO o el numero del error cero 0 
                                DESCRIBEERROR = u.DESCRIBEERROR //la descripción del error o del update/inserción exitosa
                            };
                resultado.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault()); //Resultado con el codigo de error o confirmacion de insert/update desde el stored procedure
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "guardarParameter", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                resultado.Resultado = "Error";
            }
            return resultado;
        }

        #endregion

        #region clsUsuario
        public clsResultado buscarUsuario(string idEmpleado, string nombre, short? idRol, bool? estatus, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from u in dbSIF.UDLAPSIF_PTONOM_USUARIOS_BUSCARU(idEmpleado, nombre, idRol, estatus)
                            select new clsResult_UsuarioBuscar
                            {
                                NUM = u.NUM,
                                IDROL = u.IDROL,
                                NOMBREROL = u.NOMBREROL,
                                IDEMPLEADO = u.IDEMPLEADO,
                                NOMBRE = u.NOMBRE,
                                EMAIL = u.EMAIL,
                                NAMEUPDATE = u.NAMEUPDATE,
                                FECHAUPDATE = u.FECHAUPDATE,
                                ESTATUS = u.ESTATUS
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "buscarUsuario", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado obtenRol(string rutaLog)//obtiene todos los Spend Category activos
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            try
            {
                var query = from r in dbSIF.UDLAPSIF_PTONOM_ROLES_OBTENROL()
                            select new clsObtenRol
                            {
                                IDROL = r.IDROL,
                                NOMBREROL = r.NOMBREROL
                            };
                datos.Resultado = JsonConvert.SerializeObject(query.ToList());
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "obtenRol", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            return datos;
        }

        public clsResultado guardarUsuario(clsParams_UsuarioGuarda usuario, string rutaLog)
        {
            clsResultado resultado = new clsResultado();
            resultado.Resultado = "Indefinido";

            try
            {
                var query = from u in dbSIF.UDLAPSIF_PTONOM_USUARIOS_AGREGA_ACTUALIZA
                                                                       (usuario.IDEMPLEADO,
                                                                       usuario.NOMBRE,
                                                                       usuario.EMAIL,
                                                                       usuario.IDROL,
                                                                       usuario.ESTATUS,
                                                                       usuario.NAMEUPDATE,
                                                                       usuario.IDUSER
                                                                       )
                            select new clsResult_Error
                            {
                                CLAVEERROR = u.CLAVEERROR, //valor int devuelto. IDEMPLEADO o el numero del error cero 0 
                                DESCRIBEERROR = u.DESCRIBEERROR //la descripción del error o del update/inserción exitosa
                            };
                resultado.Resultado = JsonConvert.SerializeObject(query.FirstOrDefault()); //Resultado con el codigo de error o confirmacion de insert/update desde el stored procedure
            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "guardarUsuario", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                resultado.Resultado = "Error";
            }
            return resultado;
        }
        #endregion 

        #region ActiveDirectory

        /// <summary>
        /// Metodo para obtener desde el AD, los datos de empleado para darlo de alta como usuario del sistema 
        /// Este metodo realiza busqueda de empleado por ID de epleado
        /// </summary>
        /// <param name="IdUsuario"></param>
        /// <param name="cadenaConexionAD"></param>
        /// <param name="rutaLog"></param>
        /// <returns></returns>
        public clsResultado obtenerUsuarioADPorID(string IdUsuario, string cadenaConexionAD, string rutaLog) //busca a un responsable por su ID de empleado
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            //Declaracion e inicializacion de variables locales.
            clsUsuarioAD usuario = new clsUsuarioAD();
            SearchResult resultado = null;
            if (string.IsNullOrEmpty(IdUsuario))
                return datos;
            IdUsuario = normalizaID(IdUsuario.Trim());

            DirectoryEntry raiz = new DirectoryEntry(cadenaConexionAD);
            DirectorySearcher search = new DirectorySearcher(raiz);

            //al objeto search se le asigna el primer filtro de busqueda correspodiente 
            //a la busque da usuarios y/o personas en el dominio.
            search.Filter = "(&(objectClass=user)(objectCategory=person))";

            search.Filter = "(userPrincipalName=" + IdUsuario + ")";

            search.PropertiesToLoad.Add("samaccountname");
            search.PropertiesToLoad.Add("displayname");
            search.PropertiesToLoad.Add("memberOf");
            search.PropertiesToLoad.Add("mail");
            search.PropertiesToLoad.Add("distinguishedName");
            search.PropertiesToLoad.Add("initials");
            search.PropertiesToLoad.Add("msDS-User-Account-Control-Computed");
            search.PropertiesToLoad.Add("userAccountControl");
            search.PropertiesToLoad.Add("homeDirectory");
            search.PropertiesToLoad.Add("givenName");
            search.PropertiesToLoad.Add("sn");
            search.PropertiesToLoad.Add("mailNickName");
            search.PropertiesToLoad.Add("title");
            search.PropertiesToLoad.Add("department");
            search.PropertiesToLoad.Add("info");
            //Para poder obtener los datos de ubicación y extensión del usuario
            search.PropertiesToLoad.Add("TelephoneNumber");
            search.PropertiesToLoad.Add("physicalDeliveryOfficeName");

            try
            {
                //ejecucion de la busqueda en el dominio.
                resultado = search.FindOne();

                if (resultado == null && IdUsuario.ToLower().Contains("exa"))
                {
                    IdUsuario = IdUsuario.ToLower().Replace("exa", "");
                    search.Filter = "(SAMAccountName=" + IdUsuario + ")";
                    resultado = search.FindOne();
                }

                //  clsUsuarioAD usuario = new clsUsuarioAD();

                //si resultado es diferente de null, osea que si encontro al usuario.
                if (resultado != null)
                {
                    //creacion de la instancia de un objeto UsuarioUdla.
                    usuario = new clsUsuarioAD();
                    //si resultado contiene la propiedad "displayname" se asigna a usuario.NombreCompleto
                    if (resultado.Properties.Contains("displayname"))
                    {
                        try
                        {
                            usuario.NombreCompleto = Convert.ToString(resultado.Properties["displayname"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.NombreCompleto = string.Empty;
                        }
                    }
                    else //si no  usuario.NombreCompleto se inicializa con cadena vacia.
                        usuario.NombreCompleto = string.Empty;
                    //si resultado contiene la propiedad "memberOf" se asignan los grupos
                    //a usuario.Grupos
                    if (resultado.Properties.Contains("memberOf"))
                    {
                        //recorremos cada uno de los grupos consultados.
                        for (int i = 0; i < resultado.Properties["memberOf"].Count; i++)
                        {
                            //obtenemos la lista de grupos
                            string[] grupos = Convert.ToString(resultado.Properties["memberOf"][i], CultureInfo.InvariantCulture).Split(',');
                            //insertamos cada grupo a la lista de grupos
                            for (int ii = 0; ii < grupos.Length; ii++)
                            {
                                //aqui se remueve del nombre del grupo la palabra CN=Grupo
                                if (grupos[ii].StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                                    usuario.grupos.Add(grupos[ii].Substring(3));
                            }
                        }
                    }
                    //si resultado contiene la propiedad "initials" se asignan usuario.Iniciales
                    if (resultado.Properties.Contains("initials"))
                    {
                        try
                        {
                            usuario.Iniciales = Convert.ToString(resultado.Properties["initials"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Iniciales = string.Empty;
                        }
                    }
                    else//si no  usuario.Iniciales se inicializa con cadena vacia.
                        usuario.Iniciales = string.Empty;
                    //si resultado contiene la propiedad "SAMAccountName" se asignan usuario.IdUsuario
                    if (resultado.Properties.Contains("SAMAccountName"))
                    {
                        try
                        {
                            usuario.IdUsuario = Convert.ToString(resultado.Properties["SAMAccountName"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.IdUsuario = string.Empty;
                        }
                    }
                    else//si no  usuario.IdUsuario se inicializa con cadena vacia.
                        usuario.IdUsuario = string.Empty;
                    //si resultado contiene la propiedad "mail" se asignan usuario.Correo
                    if (resultado.Properties.Contains("mail"))
                    {
                        try
                        {
                            usuario.Correo = Convert.ToString(resultado.Properties["mail"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Correo = string.Empty;
                        }
                    }
                    else//si no  usuario.Correo se inicializa con cadena vacia.
                        usuario.Correo = string.Empty;
                    //si resultado contiene la propiedad "distinguishedName" se asignan
                    //la lista de OUs a las que pertenece el usuario.
                    if (resultado.Properties.Contains("distinguishedName"))
                    {
                        //obtenemos la lista de OUs
                        string[] outemp;

                        try
                        {
                            outemp = Convert.ToString(resultado.Properties["distinguishedName"][0], CultureInfo.InvariantCulture).Split(',');
                            //recorremos cada una de las OUs
                            for (int i = 0; i < outemp.Length; i++)
                            {
                                //obtemos la OU
                                string cadena = outemp[i];
                                //removemos la OU la cadena OU=
                                if (cadena.StartsWith("OU=", StringComparison.OrdinalIgnoreCase))
                                {
                                    //la agregamos a lista de OUs
                                    usuario.listaOus.Add(cadena.Substring(3));
                                }
                            }
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            // no hace nada
                        }
                    }
                    //si resultado contiene la propiedad "msDS-User-Account-Control-Computed" se asignan
                    // a usuario.EstaBloqueado
                    if (resultado.Properties.Contains("msDS-User-Account-Control-Computed"))
                    {
                        int num;
                        try
                        {
                            num = Convert.ToInt32(resultado.Properties["msDS-User-Account-Control-Computed"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            // 16685: Tony Rojas
                            // 21-01-2009
                            // Revisar esta regla
                            num = 0;
                        }
                        if ((num & 0x10) != 0)
                            usuario.EstaBloqueado = true;
                        else
                            usuario.EstaBloqueado = false;
                    }
                    else
                        usuario.EstaBloqueado = false;
                    //si resultado contiene la propiedad "userAccountControl" se asignan
                    //a usuario.EstaAprobado
                    if (resultado.Properties.Contains("userAccountControl"))
                    {
                        int num;
                        try
                        {
                            num = Convert.ToInt32(resultado.Properties["userAccountControl"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            num = -1;
                        }
                    }

                    ////si resultado contiene la propiedad "givenName" se asignan
                    ////a usuario.Nombre
                    if (resultado.Properties.Contains("givenName"))
                    {
                        try
                        {
                            usuario.Nombre = Convert.ToString(resultado.Properties["givenName"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Nombre = string.Empty;
                        }
                    }
                    else//en caso contrario se inicializa con cadena vacia
                        usuario.Nombre = string.Empty;
                    //si resultado contiene la propiedad "sn" se asignan
                    //a usuario.Apellidos
                    if (resultado.Properties.Contains("sn"))
                    {
                        try
                        {
                            usuario.Apellidos = Convert.ToString(resultado.Properties["sn"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Apellidos = string.Empty;
                        }
                    }
                    else//en caso contrario se inicializa con cadena vacia
                        usuario.Apellidos = string.Empty;

                    ////si resultado contiene la propiedad "title" se asignan
                    ////a usuario.Titulo
                    if (resultado.Properties.Contains("title"))
                    {
                        try
                        {
                            usuario.Titulo = Convert.ToString(resultado.Properties["title"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Titulo = string.Empty;
                        }
                    }
                    else//en caso contrario se inicializa con cadena vacia
                        usuario.Titulo = string.Empty;
                    //si resultado contiene la propiedad "department" se asignan
                    //a usuario.Departamento
                    if (resultado.Properties.Contains("department"))
                    {
                        try
                        {
                            usuario.Departamento = Convert.ToString(resultado.Properties["department"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Departamento = string.Empty;
                        }
                    }
                    else//en caso contrario se inicializa con cadena vacia
                        usuario.Departamento = string.Empty;

                    if (usuario.listaOus.Contains("Alumnos"))
                        usuario.TipoUsuario = "ALUMNO";
                    else if (usuario.listaOus.Contains("Academicos"))
                        usuario.TipoUsuario = "PROFESOR";
                    else if (usuario.listaOus.Contains("Administrativos"))
                        usuario.TipoUsuario = "EMPLEADO";
                    else if (usuario.listaOus.Contains("Exalumnos"))
                        usuario.TipoUsuario = "EXALUMNO";
                    //if (usuario.IdUsuario.StartsWith("exa"))
                    //    usuario.TipoUsuario = "EXALUMNO";

                    //Para poder obtener los datos de ubicación y extensión del usuario
                    if (resultado.Properties.Contains("TelephoneNumber"))
                    {
                        try
                        {
                            usuario.Work = Convert.ToString(resultado.Properties["TelephoneNumber"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Work = string.Empty;
                        }
                    }
                    else //si no  usuario.NombreCompleto se inicializa con cadena vacia.
                        usuario.Work = string.Empty;

                    if (resultado.Properties.Contains("physicalDeliveryOfficeName"))
                    {
                        try
                        {
                            usuario.Office = Convert.ToString(resultado.Properties["physicalDeliveryOfficeName"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Office = string.Empty;
                        }
                    }
                    else //si no  usuario.NombreCompleto se inicializa con cadena vacia.
                        usuario.Office = string.Empty;
                }

                datos.Resultado = JsonConvert.SerializeObject(usuario);
            }
            catch (Exception ex)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", ex.Message.ToString());
                valores.Add("Source:", ex.Source.ToString());
                valores.Add("StackTrace:", ex.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());

                AdministradorException.Publicar(ex, valores, rutaLog, "obtenerUsuarioADPorID", TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            finally
            {
                //liberamos los recursos
                resultado = null;
                search.Dispose();
                search = null;
            }
            //retornamos el usuario.
            return datos;
        }

        /// <summary>
        /// Metodo para obtener desde el AD, los datos de empleado para darlo de alta como usuario del sistema
        /// Este metodo realiza busqueda de empleado por NOMBRE de epleado
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="cadenaConexionAD"></param>
        /// <param name="rutaLog"></param>
        /// <returns></returns>
        public clsResultado obtenerUsuarioADPorNombre(string nombre, string cadenaConexionAD, string rutaLog) //busca a un responsable por su NOMBRE de empleado
        {
            //Declaracion e inicializacion de variables locales.
            List<clsUsuarioAD> lusuarios = new List<clsUsuarioAD>();
            SearchResultCollection results;

            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";

            DirectoryEntry raiz = new DirectoryEntry(cadenaConexionAD);
            DirectorySearcher search = new DirectorySearcher(raiz);

            //al objeto search se le asigna el primer filtro de busqueda correspodiente 
            //a la busque da usuarios y/o personas en el dominio.
            search.Filter = "(&(objectClass=user)(objectCategory=person))";

            search.Filter = "(CN=*" + nombre + "*)";
            search.PropertiesToLoad.Add("samaccountname");
            search.PropertiesToLoad.Add("displayname");
            search.PropertiesToLoad.Add("memberOf");
            search.PropertiesToLoad.Add("mail");
            search.PropertiesToLoad.Add("distinguishedName");
            search.PropertiesToLoad.Add("initials");
            search.PropertiesToLoad.Add("msDS-User-Account-Control-Computed");
            search.PropertiesToLoad.Add("userAccountControl");
            search.PropertiesToLoad.Add("homeDirectory");
            search.PropertiesToLoad.Add("givenName");
            search.PropertiesToLoad.Add("sn");
            search.PropertiesToLoad.Add("mailNickName");
            search.PropertiesToLoad.Add("title");
            search.PropertiesToLoad.Add("department");
            search.PropertiesToLoad.Add("info");
            //Agregado por Alfredo Vidal Sánchez - 18704
            //Para poder obtener los datos de ubicación y extensión del usuario
            search.PropertiesToLoad.Add("TelephoneNumber");
            search.PropertiesToLoad.Add("physicalDeliveryOfficeName");

            try
            {
                //ejecucion de la busqueda en el dominio.
                results = search.FindAll();

                //si resultado es diferente de null, osea que si encontro al usuario.
                foreach (SearchResult resultado in results)
                {
                    clsUsuarioAD usuario = new clsUsuarioAD();
                    //creacion de la instancia de un objeto UsuarioUdla.
                    usuario = new clsUsuarioAD();
                    //si resultado contiene la propiedad "displayname" se asigna a usuario.NombreCompleto
                    if (resultado.Properties.Contains("displayname"))
                    {
                        try
                        {
                            usuario.NombreCompleto = Convert.ToString(resultado.Properties["displayname"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.NombreCompleto = string.Empty;
                        }
                    }
                    else //si no  usuario.NombreCompleto se inicializa con cadena vacia.
                        usuario.NombreCompleto = string.Empty;
                    //si resultado contiene la propiedad "memberOf" se asignan los grupos
                    //a usuario.Grupos
                    if (resultado.Properties.Contains("memberOf"))
                    {
                        //recorremos cada uno de los grupos consultados.
                        for (int i = 0; i < resultado.Properties["memberOf"].Count; i++)
                        {
                            //obtenemos la lista de grupos
                            string[] grupos = Convert.ToString(resultado.Properties["memberOf"][i], CultureInfo.InvariantCulture).Split(',');
                            //insertamos cada grupo a la lista de grupos
                            for (int ii = 0; ii < grupos.Length; ii++)
                            {
                                //aqui se remueve del nombre del grupo la palabra CN=Grupo
                                if (grupos[ii].StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                                    usuario.grupos.Add(grupos[ii].Substring(3));
                            }
                        }
                    }
                    //si resultado contiene la propiedad "initials" se asignan usuario.Iniciales
                    if (resultado.Properties.Contains("initials"))
                    {
                        try
                        {
                            usuario.Iniciales = Convert.ToString(resultado.Properties["initials"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Iniciales = string.Empty;
                        }
                    }
                    else//si no  usuario.Iniciales se inicializa con cadena vacia.
                        usuario.Iniciales = string.Empty;
                    //si resultado contiene la propiedad "SAMAccountName" se asignan usuario.IdUsuario
                    if (resultado.Properties.Contains("SAMAccountName"))
                    {
                        try
                        {
                            usuario.IdUsuario = Convert.ToString(resultado.Properties["SAMAccountName"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.IdUsuario = string.Empty;
                        }
                    }
                    else//si no  usuario.IdUsuario se inicializa con cadena vacia.
                        usuario.IdUsuario = string.Empty;
                    //si resultado contiene la propiedad "mail" se asignan usuario.Correo
                    if (resultado.Properties.Contains("mail"))
                    {
                        try
                        {
                            usuario.Correo = Convert.ToString(resultado.Properties["mail"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Correo = string.Empty;
                        }
                    }
                    else//si no  usuario.Correo se inicializa con cadena vacia.
                        usuario.Correo = string.Empty;
                    //si resultado contiene la propiedad "distinguishedName" se asignan
                    //la lista de OUs a las que pertenece el usuario.
                    if (resultado.Properties.Contains("distinguishedName"))
                    {
                        //obtenemos la lista de OUs
                        string[] outemp;

                        try
                        {
                            outemp = Convert.ToString(resultado.Properties["distinguishedName"][0], CultureInfo.InvariantCulture).Split(',');
                            //recorremos cada una de las OUs
                            for (int i = 0; i < outemp.Length; i++)
                            {
                                //obtemos la OU
                                string cadena = outemp[i];
                                //removemos la OU la cadena OU=
                                if (cadena.StartsWith("OU=", StringComparison.OrdinalIgnoreCase))
                                {
                                    //la agregamos a lista de OUs
                                    usuario.listaOus.Add(cadena.Substring(3));
                                }
                            }
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            // no hace nada
                        }
                    }
                    //si resultado contiene la propiedad "msDS-User-Account-Control-Computed" se asignan
                    // a usuario.EstaBloqueado
                    if (resultado.Properties.Contains("msDS-User-Account-Control-Computed"))
                    {
                        int num;
                        try
                        {
                            num = Convert.ToInt32(resultado.Properties["msDS-User-Account-Control-Computed"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            // 16685: Tony Rojas
                            // 21-01-2009
                            // Revisar esta regla
                            num = 0;
                        }
                        if ((num & 0x10) != 0)
                            usuario.EstaBloqueado = true;
                        else
                            usuario.EstaBloqueado = false;
                    }
                    else
                        usuario.EstaBloqueado = false;
                    //si resultado contiene la propiedad "userAccountControl" se asignan
                    //a usuario.EstaAprobado
                    if (resultado.Properties.Contains("userAccountControl"))
                    {
                        int num;
                        try
                        {
                            num = Convert.ToInt32(resultado.Properties["userAccountControl"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            num = -1;
                        }

                    }

                    ////si resultado contiene la propiedad "givenName" se asignan
                    ////a usuario.Nombre
                    if (resultado.Properties.Contains("givenName"))
                    {
                        try
                        {
                            usuario.Nombre = Convert.ToString(resultado.Properties["givenName"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Nombre = string.Empty;
                        }
                    }
                    else//en caso contrario se inicializa con cadena vacia
                        usuario.Nombre = string.Empty;
                    //si resultado contiene la propiedad "sn" se asignan
                    //a usuario.Apellidos
                    if (resultado.Properties.Contains("sn"))
                    {
                        try
                        {
                            usuario.Apellidos = Convert.ToString(resultado.Properties["sn"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Apellidos = string.Empty;
                        }
                    }
                    else//en caso contrario se inicializa con cadena vacia
                        usuario.Apellidos = string.Empty;

                    ////si resultado contiene la propiedad "title" se asignan
                    ////a usuario.Titulo
                    if (resultado.Properties.Contains("title"))
                    {
                        try
                        {
                            usuario.Titulo = Convert.ToString(resultado.Properties["title"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Titulo = string.Empty;
                        }
                    }
                    else//en caso contrario se inicializa con cadena vacia
                        usuario.Titulo = string.Empty;
                    //si resultado contiene la propiedad "department" se asignan
                    //a usuario.Departamento
                    if (resultado.Properties.Contains("department"))
                    {
                        try
                        {
                            usuario.Departamento = Convert.ToString(resultado.Properties["department"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Departamento = string.Empty;
                        }
                    }
                    else//en caso contrario se inicializa con cadena vacia
                        usuario.Departamento = string.Empty;

                    if (usuario.listaOus.Contains("Alumnos"))
                        usuario.TipoUsuario = "ALUMNO";
                    else if (usuario.listaOus.Contains("Academicos"))
                        usuario.TipoUsuario = "PROFESOR";
                    else if (usuario.listaOus.Contains("Administrativos"))
                        usuario.TipoUsuario = "EMPLEADO";
                    else if (usuario.listaOus.Contains("Exalumnos"))
                        usuario.TipoUsuario = "EXALUMNO";
                    //if (usuario.IdUsuario.StartsWith("exa"))
                    //    usuario.TipoUsuario = "EXALUMNO";
                    //Para poder obtener los datos de ubicación y extensión del usuario
                    if (resultado.Properties.Contains("TelephoneNumber"))
                    {
                        try
                        {
                            usuario.Work = Convert.ToString(resultado.Properties["TelephoneNumber"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Work = string.Empty;
                        }
                    }
                    else //si no  usuario.NombreCompleto se inicializa con cadena vacia.
                        usuario.Work = string.Empty;

                    if (resultado.Properties.Contains("physicalDeliveryOfficeName"))
                    {
                        try
                        {
                            usuario.Office = Convert.ToString(resultado.Properties["physicalDeliveryOfficeName"][0], CultureInfo.InvariantCulture);
                        }
                        catch (IndexOutOfRangeException IndexException)
                        {
                            usuario.Office = string.Empty;
                        }
                    }
                    else //si no  usuario.NombreCompleto se inicializa con cadena vacia.
                        usuario.Office = string.Empty;
                    lusuarios.Add(usuario);
                    datos.Resultado = JsonConvert.SerializeObject(lusuarios);
                }
            }
            catch (Exception ex)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", ex.Message.ToString());
                valores.Add("Source:", ex.Source.ToString());
                valores.Add("StackTrace:", ex.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());

                AdministradorException.Publicar(ex, valores, rutaLog, "obtenerUsuarioADPorNombre", TipoSalidaExcepcion.Txt);
                datos.Resultado = "Error";
            }
            finally
            {
                //liberamos los recursos

                search.Dispose();
                search = null;
            }
            //retornamos el usuario.
            return datos;
        }

        //algunos metodos para encontrar usuarios requieren de normalizaID() para eliminar el prefijo de ceros al inicio de un ID
        public static string normalizaID(string user)
        {
            if (string.IsNullOrEmpty(user))
                return "";
            if (user.ToLower().StartsWith("udla") && user.ToLower().Contains("\\"))
                user = user.Split('\\')[1];
            quitaCeros:
            if (user.StartsWith("0"))
            {
                user = user.Remove(0, 1);
                goto quitaCeros;
            }
            return user.Trim();
        }
        #endregion
    }
}
