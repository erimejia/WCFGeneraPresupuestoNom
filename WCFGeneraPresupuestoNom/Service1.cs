using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Presupuesto.Clases;
using Presupuesto.Presentacion;

namespace WCFGeneraPresupuestoNom
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        string rutaLog = ConfigurationManager.AppSettings["rutaLog"];        
        string dominioAD = ConfigurationManager.AppSettings["DominioUserPrincipal"];
        string conexionAD = ConfigurationManager.AppSettings["cadenaConexionAD"];
        string ConexionNOMINAS = ConfigurationManager.ConnectionStrings["ConsultaNOMINAS"].ConnectionString;
        string ConexionUDLAPSIF = ConfigurationManager.ConnectionStrings["ConsultaUDLAPSIF"].ConnectionString; 

        //CATALOGOS
        public string Catalogo(string tipoObjeto, string metodo, string parametros)
        {
            try
            {
                JObject param = JObject.Parse(parametros);
                PCatalogo catalogos = new PCatalogo();
                switch (tipoObjeto)
                {
                    case "clsCostCenter":
                        switch (metodo)
                        {
                            case "buscarCostCenter":
                                int? ID_CC = param["ID_CC"].ToObject<int?>();
                                string CC = param["CostCenter"].ToString();
                                string Descripcion = param["Descripcion"].ToString();
                                bool? Estatus = param["Estatus"].ToObject<bool?>();//bool es null para permitir busqueda de TODOS
                                return JsonConvert.SerializeObject(catalogos.buscarCC(ID_CC, CC, Descripcion, Estatus, rutaLog));
                            case "obtenCostCenter":
                                return JsonConvert.SerializeObject(catalogos.obtenCC(rutaLog));
                            case "guardarCostCenter":
                                clsParams_CostCenterGuarda CostCenter = param.ToObject<clsParams_CostCenterGuarda>();
                                return JsonConvert.SerializeObject(catalogos.guardarCC(CostCenter, rutaLog));
                            default:
                                return "Método desconocido";
                        }

                    case "clsLedgerAccount":
                        switch (metodo)
                        {
                            case "buscarLedgerAccount":
                                int? ID_LA = param["ID_LA"].ToObject<int?>();
                                string LA = param["LedgerAccount"].ToString();
                                string Descripcion = param["Descripcion"].ToString();
                                bool? Estatus = param["Estatus"].ToObject<bool?>();//bool es null para permitir busqueda de TODOS
                                return JsonConvert.SerializeObject(catalogos.buscarLA(ID_LA, LA, Descripcion, Estatus, rutaLog));
                            case "obtenLedgerAccount":
                                return JsonConvert.SerializeObject(catalogos.obtenLA(rutaLog));
                            case "guardarLedgerAccount":
                                clsParams_LedgerAccountGuarda LedgerAccount = param.ToObject<clsParams_LedgerAccountGuarda>();
                                return JsonConvert.SerializeObject(catalogos.guardarLA(LedgerAccount, rutaLog));
                            default:
                                return "Método desconocido";
                        }

                    case "clsSpendCategory":
                        switch (metodo)
                        {
                            case "buscarSpendCategory":
                                int? ID_SC = param["ID_SC"].ToObject<int?>();
                                string SC = param["SpendCategory"].ToString();
                                string Descripcion = param["Descripcion"].ToString();
                                bool? Estatus = param["Estatus"].ToObject<bool?>();//bool es null para permitir busqueda de TODOS
                                return JsonConvert.SerializeObject(catalogos.buscarSC(ID_SC, SC, Descripcion, Estatus, rutaLog));
                            case "obtenSpendCategory":
                                return JsonConvert.SerializeObject(catalogos.obtenSC(rutaLog));
                            case "guardarSpendCategory":
                                clsParams_SpendCategoryGuarda SpendCategory = param.ToObject<clsParams_SpendCategoryGuarda>();
                                return JsonConvert.SerializeObject(catalogos.guardarSC(SpendCategory, rutaLog));
                            default:
                                return "Método desconocido";
                        }

                    case "clsCuentas":
                        switch (metodo)
                        {
                            case "buscarCuentas":
                                int? ID_Cuenta = param["ID_Cuenta"].ToObject<int?>();
                                string Cuenta = param["Cuenta"].ToString();
                                string Descripcion = param["Descripcion"].ToString();
                                bool? Estatus = param["Estatus"].ToObject<bool?>();//bool es null para permitir busqueda de TODOS
                                string CC = param["CostCenter"].ToString();
                                int? ID_SC = param["ID_SC"].ToObject<int?>();
                                int? ID_LA = param["ID_LA"].ToObject<int?>();
                                return JsonConvert.SerializeObject(catalogos.buscarC(ID_Cuenta, Cuenta, Descripcion, Estatus, CC, ID_SC, ID_LA, rutaLog));
                            case "guardarCuentas":
                                clsParams_CuentaGuarda Cuentas = param.ToObject<clsParams_CuentaGuarda>();
                                return JsonConvert.SerializeObject(catalogos.guardarC(Cuentas, rutaLog));
                            case "obtenTipoCuenta":
                                return JsonConvert.SerializeObject(catalogos.obtenTipoCuenta(rutaLog));
                            case "obtenCurrency":
                                return JsonConvert.SerializeObject(catalogos.obtenCurrency(rutaLog));
                            default:
                                return "Método desconocido";
                        }

                    case "clsProject":
                        switch (metodo)
                        {
                            case "buscarProject":
                                short? ID_Project = param["ID_Project"].ToObject<short?>();
                                string Project = param["Project"].ToString();
                                string Descripcion = param["Descripcion"].ToString();
                                bool? Estatus = param["Estatus"].ToObject<bool?>();//bool es null para permitir busqueda de TODOS
                                return JsonConvert.SerializeObject(catalogos.buscarPR(ID_Project, Project, Descripcion, Estatus, rutaLog));
                            case "obtenProject":
                                return JsonConvert.SerializeObject(catalogos.obtenPR(rutaLog));
                            case "guardarProject":
                                clsParams_ProjectGuarda project = param.ToObject<clsParams_ProjectGuarda>();
                                return JsonConvert.SerializeObject(catalogos.guardarPR(project, rutaLog));
                            default:
                                return "Método desconocido";
                        }

                    case "clsParameters":
                        switch (metodo)
                        {
                            case "buscarParameter":
                                short IdParam = param["IDParam"].ToObject<short>();
                                return JsonConvert.SerializeObject(catalogos.buscarParameter(IdParam, rutaLog));
                            case "obtenParameters":
                                return JsonConvert.SerializeObject(catalogos.obtenParameters(rutaLog));
                            case "guardarParameter":
                                clsParams_ParameterGuarda usuario = param.ToObject<clsParams_ParameterGuarda>();
                                return JsonConvert.SerializeObject(catalogos.guardarParameter(usuario, rutaLog));
                            default:
                                return "Método desconocido";
                        }

                    case "clsUsuario":
                        switch (metodo)
                        {
                            case "buscarUsuario":
                                string IdEmpleado = param["IdEmpleado"].ToString();
                                string Nombre = param["Nombre"].ToString();
                                short? IdRol = param["IdRol"].ToObject<short?>();//short puede ser null para permitir busqueda de TODOS
                                bool? Estatus = param["Estatus"].ToObject<bool?>();//bool puede ser null para permitir busqueda de TODOS
                                return JsonConvert.SerializeObject(catalogos.buscarUsuario(IdEmpleado, Nombre, IdRol, Estatus, rutaLog));
                            case "obtenRol":
                                return JsonConvert.SerializeObject(catalogos.obtenRol(rutaLog));
                            case "guardarUsuario":
                                clsParams_UsuarioGuarda usuario = param.ToObject<clsParams_UsuarioGuarda>();
                                return JsonConvert.SerializeObject(catalogos.guardarUsuario(usuario, rutaLog));
                            case "obtenerUsuarioADPorID": //busca a un usuario por su ID de empleado
                                string ID = param["IDUsuario"].ToString();
                                return JsonConvert.SerializeObject(catalogos.obtenerUsuarioADPorID(ID.Trim() + dominioAD, conexionAD, rutaLog));
                            case "obtenerUsuarioADPorNombre": //busca a un usuario por su NOMBRE de empleado
                                string nombre = param["Nombre"].ToString();
                                return JsonConvert.SerializeObject(catalogos.obtenerUsuarioADPorNombre(nombre, conexionAD, rutaLog));
                            default:
                                return "Método desconocido";
                        }                     

                    //case "clsCurrency":
                    //    switch (metodo)
                    //    {
                    //        case "obtenCurrency":
                    //            return JsonConvert.SerializeObject(catalogos.obtenCurrency(rutaLog));
                    //        default:
                    //            return "Método desconocido";
                    //    }

                    //case "clsTipoCuenta":
                    //    switch (metodo)
                    //    {
                    //        case "obtenTipoCuenta":
                    //            return JsonConvert.SerializeObject(catalogos.obtenTipoCuenta(rutaLog));
                    //        default:
                    //            return "Método desconocido";
                    //    }

                    //case "clsRol":
                    //    switch (metodo)
                    //    {
                    //        case "obtenRol":
                    //            return JsonConvert.SerializeObject(catalogos.obtenRol(rutaLog));
                    //        default:
                    //            return "Método desconocido";
                    //    }

                    default:
                        return "Tipo de objeto desconocido";
                }
            }
            catch (JsonReaderException)
            {
                return "Llamada sin parámetros";
            }
            catch (NullReferenceException)
            {
                return "Parámetro desconocido";
            }
        }


        //OPERCION
        public string Operacion(string tipoObjeto, string metodo, string parametros)
        {
            try
            {
                JObject param = JObject.Parse(parametros);
                POperacion operacion = new POperacion();
                switch (tipoObjeto)
                {
                    case "clsConsultaPolizas":
                        switch (metodo)
                        {
                            case "buscaPolizaNom":
                                string fechaIni = param["FechaIni"].ToString();
                                string fechaFin = param["FechaFin"].ToString();
                                string bachNumb = param["BachNumb"].ToString();
                                string refrence = param["Refrence"].ToString();
                                char? statusLayout = param["StatusLayout"].ToObject<char?>();
                                bool? procesaConta = param["ProcesaConta"].ToObject<bool?>();
                                return JsonConvert.SerializeObject(operacion.buscaPolizaNom(fechaIni, fechaFin, bachNumb, refrence, statusLayout, procesaConta, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));
                            case "anulaPolizaNom":
                                string anulaFechaIni = param["FechaIni"].ToString();
                                string anulaFechaFin = param["FechaFin"].ToString();
                                string anulaBachNumb = param["BachNumb"].ToString();
                                return JsonConvert.SerializeObject(operacion.anulaPolizaNom(anulaFechaIni, anulaFechaFin, anulaBachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));

                            default:
                                return "Método desconocido";
                        }

                    case "clsConsultaCuentas":
                        switch (metodo)
                        {
                            case "buscaCuentaNom":
                                string fechaIni = param["FechaIni"].ToString();
                                string fechaFin = param["FechaFin"].ToString();
                                string bachNumb = param["BachNumb"].ToString();
                                return JsonConvert.SerializeObject(operacion.buscaCuentaNom(fechaIni, fechaFin, bachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));
                            case "anulaCuentaNom":
                                int jrnlidx = param["Jrnlidx"].ToObject<int>();
                                return JsonConvert.SerializeObject(operacion.anulaCuentaNom(jrnlidx, ConexionNOMINAS, rutaLog));

                            default:
                                return "Método desconocido";
                        }

                    case "clsLayoutNomina":
                        switch (metodo)
                        {
                            //        public clsResultado obtenLayoutMultiplesNominas(string fechaIni, string fechaFin, List<clsParamBachnumbRefrece> ParamsBachnumbRefrence, string estatusLayout, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)

                            case "obtenLayoutMultiplesNominas":
                                string fechaIni_LayoutMultiplesNom = param["FechaIni"].ToString();
                                string fechaFin_LayoutMultiplesNom = param["FechaFin"].ToString();
                                List<clsParamBachnumbRefrece> ParamsBachnumbRefrence = param["ParamsBachnumbRefrence"].ToObject<List<clsParamBachnumbRefrece>>();
                                string estatus_LayoutLayoutMultiplesNom = param["EstatusLayout"].ToString();
                                return JsonConvert.SerializeObject(operacion.obtenLayoutMultiplesNominas(fechaIni_LayoutMultiplesNom, fechaFin_LayoutMultiplesNom, ParamsBachnumbRefrence, estatus_LayoutLayoutMultiplesNom, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));
                            case "obtenLayout":
                                string fechaIni = param["FechaIni"].ToString();
                                string fechaFin = param["FechaFin"].ToString();
                                string bachNumb = param["BachNumb"].ToString();
                                string paramDescription = param["Refrence"].ToString();
                                string estatusLayout = param["EstatusLayout"].ToString();
                                return JsonConvert.SerializeObject(operacion.obtenLayout(fechaIni, fechaFin, bachNumb, paramDescription, estatusLayout, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));
                            case "cambiaStatusPolizaProcesada":
                                string procesadaFechaIni = param["FechaIni"].ToString();
                                string procesadaFechaFin = param["FechaFin"].ToString();
                                string procesadaBachNumb = param["BachNumb"].ToString();
                                return JsonConvert.SerializeObject(operacion.cambiaStatusPolizaProcesada(procesadaFechaIni, procesadaFechaFin, procesadaBachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));
                            case "cambiaStatusMultiplesPolizasProcesada":
                                string procesadaMPFechaIni = param["FechaIni"].ToString();
                                string procesadaMPFechaFin = param["FechaFin"].ToString();
                                List<String> listProcesadaMPBachNumb = param["ListBachNumbs"].ToObject<List<String>>();
                                return JsonConvert.SerializeObject(operacion.cambiaStatusMultiplesPolizasProcesada(procesadaMPFechaIni, procesadaMPFechaFin, listProcesadaMPBachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));
                            case "reviertePolizaProcesada":
                                string revierteProcesadaFechaIni = param["FechaIni"].ToString();
                                string revierteProcesadaFechaFin = param["FechaFin"].ToString();
                                string revierteProcesadaBachNumb = param["BachNumb"].ToString();
                                return JsonConvert.SerializeObject(operacion.reviertePolizaProcesada(revierteProcesadaFechaIni, revierteProcesadaFechaFin, revierteProcesadaBachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));
                            case "updateDebitAmt":
                                int jrnlidx = param["Jrnlidx"].ToObject<int>();
                                decimal debitAmt = param["DebitAmt"].ToObject<decimal>();
                                return JsonConvert.SerializeObject(operacion.updateDebitAmt(jrnlidx, debitAmt, ConexionNOMINAS, rutaLog));

                            default:
                                return "Método desconocido";
                        }

                    case "clsPolizaImportada":
                        switch (metodo)
                        {
                            //public clsResultado obtenLayoutMultiplesNominas(string fechaIni, string fechaFin, List<clsParamBachnumbRefrece> ParamsBachnumbRefrence, string estatusLayout, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)

                            case "guardaNuevaPoliza":
                                string bachNumb = param["BachNumb"].ToString();
                                string reference = param["Reference"].ToString();
                                string createDate = param["CreateDate"].ToString();
                                List<clsActnumberDebitamt> actnumberDebitamt = param["ActnumberDebitamt"].ToObject<List<clsActnumberDebitamt>>();
                                return JsonConvert.SerializeObject(operacion.guardaNuevaPoliza(bachNumb, reference, createDate, actnumberDebitamt, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));
                            //Cases deprecado
                            //case "obtenLayoutAEDprocesadas":
                            //    string LayoutAEDfechaIni = param["FechaIni"].ToString();
                            //    string LayoutAEDfechaFin = param["FechaFin"].ToString();
                            //    string LayoutAEDbachNumb = param["BachNumb"].ToString();
                            //    string LayoutAEDparamDescription = param["Refrence"].ToString();
                            //    return JsonConvert.SerializeObject(operacion.obtenLayoutAEDprocesadas(LayoutAEDfechaIni, LayoutAEDfechaFin, LayoutAEDbachNumb, LayoutAEDparamDescription, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog));

                            default:
                                return "Método desconocido";
                        }

                    default:
                        return "Tipo de objeto desconocido";
                }
            }
            catch (JsonReaderException)
            {
                return "Llamada sin parámetros";
            }
            catch (NullReferenceException)
            {
                return "Parámetro desconocido";
            }
        }

        //ACCESO
        public string Acceso(string tipoObjeto, string metodo, string parametros)
        {
            try
            {
                JObject param = JObject.Parse(parametros);
                PAcceso acceso = new PAcceso();
                switch (tipoObjeto)
                {
                    case "clsAccesos":
                        switch (metodo)
                        {
                            case "validaAcceso":
                                string IDUsuario_Validar = param["IDUsuario"].ToString();
                                return JsonConvert.SerializeObject(acceso.validaAcceso(IDUsuario_Validar, rutaLog));
                            default:
                                return "Método desconocido";
                        }
                    default:
                        return "Tipo de objeto desconocido";
                }
            }
            catch (JsonReaderException)
            {
                return "Llamada sin parámetros";
            }
            catch (NullReferenceException)
            {
                return "Parámetro desconocido";
            }
        }

        //REPORTES
        public string Reportes(string tipoObjeto, string metodo, string parametros)
        {
            try
            {
                JObject param = JObject.Parse(parametros);
                PReportes reportes = new PReportes();
                switch (tipoObjeto)
                {
                //    case "clsReporteAlumnos":
                //        switch (metodo)
                //        {
                //            case "obtenerEstudiantesPorPeriodoMateria":
                //                string ClaveMatertiaSinDetalle = param["ClaveMatertia"].ToString();
                //                string ClaveCarreraSinDetalle = param["ClaveCarrera"].ToString();
                //                string AnioSinDetalle = param["Anio"].ToString();
                //                string AnioHastaSinDetalle = param["AnioHasta"].ToString();
                //                return JsonConvert.SerializeObject(reportes.obtenerEstudiantesPorPeriodoMateria(ClaveMatertiaSinDetalle, ClaveCarreraSinDetalle, AnioSinDetalle, AnioHastaSinDetalle, rutaLog));

                //            default:
                //                return "Método desconocido";
                //        }
                    default:
                        return "Tipo de objeto desconocido";

                }
            }
            catch (JsonReaderException)
            {
                return "Llamada sin parámetros";
            }
            catch (NullReferenceException)
            {
                return "Parámetro desconocido";
            }
        }

    }
}
