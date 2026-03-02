using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
//using System.Data.SqlTypes;
using Newtonsoft.Json;
using Udla.Exceptions;
using Presupuesto.Clases;
using Presupuesto.Entidades;

using System.Data;
using System.Data.SqlClient;

namespace Presupuesto.Logica
{
    public class LOperacion
    {
        UDLAPSIFEntities dbSIF = new UDLAPSIFEntities();
        string tblSrcPoliciesNOM = "xNOMExcelRH"; //Tabla de nominas importadas desde el Excel del reporte que RH edita y envia a Presupuestos
        //string tblSrcPoliciesNOM = "xNOM2001GPInt"; //Tabla con todas las polizas de Nominas sin editar, directo desde Fortia


        //clsConsultaPolizas
        //buscaPoliza - Este metodo devuleve una lista por grupo de polizas, donde cada registro indica los totales que hay por poliza, 
        //inincluyendo por poliza sus totales de cuentas duplicadas, cuentas de vales, cuentas de origen, cuentas de destino y cuentas de proyecto.
        //Este metodo es para el panel de busqueda de polizas.
        public clsResultado buscaPolizaNom(string fechaIni, string fechaFin, string bachNumb, string refrence, char? statusLayout, bool? procesaConta, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            List<char> ListDigCuentaGP = new List<char>();
            List<clsResult_PolizaNomBuscar> ListPolizas = new List<clsResult_PolizaNomBuscar>();

            string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

            try
            {
                var listErrorBatchnumbs = Get_ErrorBatchnumbs(fechaIni, fechaFin, bachNumb, statusLayout, procesaConta, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);


            //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
                using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
                {
                    using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
                    {
                        Conexion.Open();
                        SqlDataReader dr = sql_command.ExecuteReader();

                        while (dr.Read())
                        {
                            char digCnts; //= new char();
                            digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString());
                            ListDigCuentaGP.Add(digCnts);
                        }
                        sql_command.Connection.Close();
                        Conexion.Close();
                    }
                }

                //SEGUNDO, con la lista de los prefijos(el primer numero) del 'segmento cuenta' de las cuentas-GP, genera la lista de Polizas de Nominas. 
                //En los grupos de polizas generados se incluyen todas las 'cuentas' duplicadas, de vales, de origen, de destino y de proyecto.
                if (ListDigCuentaGP.Count != 0)
                {
                    int rowCount = 1;//inicia contador para enumerar cada fila de todo el SqlDataReader(ListPolizas) generado con el query
                    string query = "SELECT "
                                    //+ "     CAST(ROW_NUMBER() OVER(ORDER BY CreateDate ASC) AS varchar (max)) AS NUM," //aqui enumera filas pero por grupos de DigCuentaGP y no de forma continua
                                    //+ "     @rowCount AS NUM,"//otra opción pero solo enumera por cada itreracion del foreach y no por cada while del SqlDataReader
                                    + "     Active as PROCESADOCONTABILIDAD,"
                                    + "    'ESTATUSLAYOUT'= CASE WHEN CtrlPPTO='9' THEN 'Procesada Manual'" //9
                                    + "                           WHEN CtrlPPTO IS NULL THEN 'Pendiente'" //0
                                    + "                           WHEN CtrlPPTO='1' THEN 'Procesada'" //1
                                    + "                           WHEN CtrlPPTO='2' THEN 'Anulada'" //2
                                    + "                      END,"
                                    + "    BACHNUMB as IDPOLIZA,"
                                    + "    REFRENCE,"
                                    + "    CreateDate as FECHAPOLIZA,"
                                    + "    sum(DEBITAMT) as TOTALDEBITO,"
                                    + "    count(BACHNUMB) as TOTALINSTRUCCIONES"

                                    + " FROM " + tblSrcPoliciesNOM + ""

                                    + " WHERE"

                                    //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
                                    + " ACTNUMBER like @ACTNUMBER"
                                    //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
                                    + " AND DEBITAMT >= 0.00" //el operador >= permite obtener cuentas de debito con montos = 0.00 para los casos de Cuentas Especiales de RH
                                                              //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
                                    + " AND CRDTAMT = 0.00"

                                    //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
                                    + " AND (CreateDate BETWEEN"
                                    + "    CASE"
                                    + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
                                    + "        ELSE '" + fechaIni + "'"
                                    + "    END"
                                    + "    AND"
                                    + "    CASE"
                                    + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
                                    + "    ELSE '" + fechaFin + "'"
                                    + "    END)"

                                    //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
                                    + " AND BACHNUMB LIKE CASE"
                                    + "                     WHEN '" + bachNumb + "'='' THEN BACHNUMB"
                                    + "                     WHEN '" + bachNumb + "' IS NULL THEN BACHNUMB"
                                    + "                     ELSE '" + "%" + bachNumb + "%" + "'"
                                    + "                   END"

                                    //PARAMETRO3 "REFRENCE"
                                    + " AND REFRENCE LIKE CASE"
                                    + "                     WHEN '" + refrence + "'='' THEN REFRENCE"
                                    + "                     WHEN '" + refrence + "' IS NULL THEN REFRENCE"
                                    + "                     ELSE '" + "%" + refrence + "%" + "'"
                                    + "                   END"

                                    //PARAMETRO4 "ESTATUS LAYOUT"
                                    + " AND (('" + statusLayout + "'='0')"
                                    + "            AND (CtrlPPTO IS NULL)"
                                    + "        OR"
                                    + "        ('" + statusLayout + "' = '1' OR '" + statusLayout + "' = '2' OR '" + statusLayout + "' = '9')"
                                    + "            AND CtrlPPTO = CASE"
                                    + "                                WHEN '" + statusLayout + "' = '1' THEN '1'"
                                    + "                                WHEN '" + statusLayout + "' = '2' THEN '2'"
                                    + "                                WHEN '" + statusLayout + "' = '9' THEN '9'"
                                    + "                            END"
                                    + "        OR"
                                    + "        ('" + statusLayout + "' IS NULL OR '" + statusLayout + "'='')"
                                    + "            AND(CtrlPPTO IS NULL"
                                    + "                OR CtrlPPTO = '1'"
                                    + "                OR CtrlPPTO = '2'"
                                    + "                OR CtrlPPTO = '9')"
                                    + "      )"

                                    //PARAMETRO5 "PROCESADO POR CONTA". 1=No contabilizada / 0=Si contabilizada
                                    + " AND Active = CASE"
                                    + "                  WHEN @ACTIVE IS NULL THEN Active"//el valor nulo "null" solo lo puede recibir por parametro el @ACTIVE y no de manera directa
                                    + "                  ELSE @ACTIVE"
                                    + "              END"

                                    + " Group by Active, CtrlPPTO, BACHNUMB, REFRENCE, CreateDate"
                                    + " Order by CreateDate,BACHNUMB";

                    using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                    {
                        using (SqlCommand sql_command = new SqlCommand(query, Conexion))
                        {
                            foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
                            {
                                Conexion.Open();
                                //rowCount++; este contador solo enumera las veces que entra en el foreach, pero no las veces que entra en el while del Reader
                                char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
                                string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
                                sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query
                                sql_command.Parameters.AddWithValue("@ACTIVE", procesaConta == null ? (object)DBNull.Value : procesaConta);
                                //sql_command.Parameters.AddWithValue("@rowCount", rowCount);
                                sql_command.CommandType = CommandType.Text;
                                SqlDataReader dr = sql_command.ExecuteReader();
                                while (dr.Read())
                                {
                                    clsResult_PolizaNomBuscar Poliza = new clsResult_PolizaNomBuscar();
                                    //Poliza.Num = dr["NUM"].ToString();
                                    Poliza.Num = (rowCount++).ToString();//inserta un contador por cada nueva fila generada
                                    Poliza.ProcesadoConta = Boolean.Parse(dr["PROCESADOCONTABILIDAD"].ToString());
                                    Poliza.EstausLayout = dr["ESTATUSLAYOUT"].ToString();
                                    Poliza.idPoliza = dr["IDPOLIZA"].ToString();
                                    Poliza.errorPoliza = !listErrorBatchnumbs.Contains(Poliza.idPoliza);
                                    Poliza.Refrence = dr["REFRENCE"].ToString();
                                    Poliza.FechaPoliza = DateTime.Parse(dr["FECHAPOLIZA"].ToString());
                                    Poliza.TotalDebito = Decimal.Parse(dr["TOTALDEBITO"].ToString());
                                    Poliza.TotalInstrucciones = Int32.Parse(dr["TOTALINSTRUCCIONES"].ToString());
                                    ListPolizas.Add(Poliza);
                                }
                                sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                sql_command.Parameters.RemoveAt("@ACTIVE");
                                //sql_command.Parameters.RemoveAt("@rowCount");
                                sql_command.Connection.Close();
                                Conexion.Close();
                            }

                            datos.Resultado = JsonConvert.SerializeObject(ListPolizas);
                        }
                    }
                }
                else //ocurre cuando la lista 'ListDigCuentaGP' esta vacia
                {
                    ListPolizas.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(ListPolizas);
                    return datos;
                }
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "buscarPolizaNom", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";
            }

            return datos;
        }
        //AnulaPoliza - Se anulan todas las cuentas que pertenecen a una poliza sin exeptuar cuentas de ningun tipo o estatus
        public clsResultado anulaPolizaNom(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            List<char> ListDigCuentaGP = new List<char>();
            int filasActualizadas = 0; //inicializa en cero la cantidad de registros anulados

            string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

            try
            {
                //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
                using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
                {
                    using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
                    {
                        Conexion.Open();
                        SqlDataReader dr = sql_command.ExecuteReader();

                        while (dr.Read())
                        {
                            char digCnts; //= new char();
                            digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString()); // en caso de que el catalogo este vacio se produce el error: "String must be exactly one character long"
                            ListDigCuentaGP.Add(digCnts); //nunca enviara una lista vacia. En caso de estarvacia antes se genera caera en el Catch, enviando un NULL al front
                        }
                        sql_command.Connection.Close();
                        Conexion.Close();
                    }
                }

                //SEGUNDO, con la lista de los prefijos(el primer numero) del 'segmento cuenta' de las cuentas-GP, se genera la lista de todas las Cuentas-GP sin filtrar ninguna para su anulacion
                if (ListDigCuentaGP.Count != 0) //Este IF esta sobrando ya que nunca recibira una lista sin registros, antes generaria un error
                {
                    char anula = '2';//el valor 2 Anula una cuenta
                    string upd_estatus = "UPDATE "
                                    + "     " + tblSrcPoliciesNOM + ""

                                    + " SET CtrlPPTO = '" + anula + "'," //se anula cambiando el Estatus de cada cuenta a 2(Anulada)
                                    + "     ModifyDate = CAST(GETDATE() AS DATE)" //establece en ModifyDate la fecha en la que se ejecuta el update

                                    + " WHERE"

                                    //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
                                    + " ACTNUMBER like @ACTNUMBER"
                                    //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
                                    + " AND DEBITAMT >= 0.00" //el operador >= permite obtener cuentas de debito con montos = 0.00 para los casos de Cuentas Especiales de RH
                                    //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
                                    + " AND CRDTAMT = 0.00"

                                    //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
                                    + " AND (CreateDate BETWEEN"
                                    + "    CASE"
                                    + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
                                    + "        ELSE '" + fechaIni + "'"
                                    + "    END"
                                    + "    AND"
                                    + "    CASE"
                                    + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
                                    + "    ELSE '" + fechaFin + "'"
                                    + "    END)"

                                    //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
                                    + " AND BACHNUMB = '" + bachNumb + "'";
                                    //+ " AND BACHNUMB LIKE CASE" no se requiere un CASE para recibir un bachnumb Nulo, ni Incompleto, ni un Like
                                    //+ "                     WHEN '" + bachNumb + "'='' THEN BACHNUMB"
                                    //+ "                     WHEN '" + bachNumb + "' IS NULL THEN BACHNUMB"
                                    //+ "                     ELSE '" + "%" + bachNumb + "%" + "'"
                                    //+ "                   END"

                                    //PARAMETRO3 "REFRENCE" //este filtro no es requerido

                                    //PARAMETRO4 "ESTATUS LAYOUT" //este filtro no es requerido

                                    //PARAMETRO5 "PROCESADO POR CONTA". 1=No contabilizada / 0=Si contabilizada //este filtro no es requerido                                  
                                    
                                    //+ " Group by Active, CtrlPPTO, BACHNUMB, REFRENCE, CreateDate" //Agrupar no es requerido
                                    //+ " Order by CreateDate,BACHNUMB"; //Ordenar no es requerido

                    using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                    {
                        using (SqlCommand sql_command = new SqlCommand(upd_estatus, Conexion))
                        {
                            foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
                            {
                                Conexion.Open();
                                char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
                                string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
                                sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query

                                int Actualizadas = sql_command.ExecuteNonQuery();
                                filasActualizadas += Actualizadas; //total de cuentas-GP anuladas

                                sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                sql_command.Connection.Close();
                                Conexion.Close();
                            }

                            datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                        }
                    }
                }
                else //ocurre cuando la lista 'ListDigCuentaGP' esta vacia
                {
                    filasActualizadas=0;
                    datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                    return datos;
                }
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "anulaPolizaNom", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";//Falla en el Update
            }

            return datos;
        }

        //clsConsultaCuentas
        //buscaCuenta - Obtiene el detalle de una Poliza. Es decir la lista completa de todas las cuentas que pertenecen a una poliza
        //Este metodo devuelve la lista de cuentas contables de GP con su correspondiente cuenta WD
        public clsResultado buscaCuentaNom(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            List<char> ListDigCuentaGP = new List<char>();
            List<clsResult_CuentaNomBuscar> ListCuentasPrevia = new List<clsResult_CuentaNomBuscar>();
            List<clsResult_CuentaNomBuscar> ListCuentas = new List<clsResult_CuentaNomBuscar>();

            string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

            try
            {
                //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
                using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
                {
                    using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
                    {
                        Conexion.Open();
                        SqlDataReader dr = sql_command.ExecuteReader();

                        while (dr.Read())
                        {
                            char digCnts; //= new char();
                            digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString());
                            ListDigCuentaGP.Add(digCnts);
                        }
                        sql_command.Connection.Close();
                        Conexion.Close();
                    }
                }

                //SEGUNDO, con la lista de los prefijos(el primer numero) del 'segmento cuenta' de las cuentas-GP, se genera la lista de Cuentas-GP corresponndeintes a la poliza recibida por parametro
                //En este paso no se agrupa el resultado, ni se depuran las cuentas ya que son el detallado de la poliza.
                //Unicamente se discriminan las cuentas que de lado del 'catalogo de cuentas, costcenter, ledgerAcoount y spendCategory' estan deshabilitadas
                if (ListDigCuentaGP.Count != 0)
                {
                    //int rowCount = 1;//inicia contador para enumerar cada fila de todo el SqlDataReader(ListPolizas) generado con el query
                    string query = "SELECT "
                                    + "    JRNLIDX,"
                                    + "    BACHNUMB as IDPOLIZA,"
                                    + "    CONCAT(SUBSTRING(ACTNUMBER,1,2),'-',SUBSTRING(ACTNUMBER,3,2),'-',SUBSTRING(ACTNUMBER,5,6),'-',SUBSTRING(ACTNUMBER,11,3),'-',SUBSTRING(ACTNUMBER,14,8)) as CUENTAGP,"
                                    + "    DEBITAMT as DEBITO,"
                                    + "    REFRENCE,"
                                    + "    CreateDate as FECHAPOLIZA,"
                                    + "    Active as PROCESADOCONTABILIDAD,"
                                    + "    'ESTATUSLAYOUT'= CASE WHEN CtrlPPTO='9' THEN 'Procesada Manual'" //9
                                    + "                           WHEN CtrlPPTO IS NULL THEN 'Pendiente'" //0
                                    + "                           WHEN CtrlPPTO='1' THEN 'Procesada'" //1
                                    + "                           WHEN CtrlPPTO='2' THEN 'Anulada'" //2
                                    + "                      END,"
                                    + "    ModifyDate as FECHAUPDATE"

                                    + " FROM " + tblSrcPoliciesNOM + ""

                                    + " WHERE"

                                    //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
                                    + " ACTNUMBER like @ACTNUMBER"
                                    //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
                                    + " AND DEBITAMT >= 0.00" //el operador >= permite obtener cuentas de debito con montos = 0.00 para los casos de Cuentas Especiales de RH
                                    //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
                                    + " AND CRDTAMT = 0.00"
                                    //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
                                    + " AND (CreateDate BETWEEN"
                                    + "    CASE"
                                    + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
                                    + "        ELSE '" + fechaIni + "'"
                                    + "    END"
                                    + "    AND"
                                    + "    CASE"
                                    + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
                                    + "    ELSE '" + fechaFin + "'"
                                    + "    END)"
                                    //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
                                    + " AND BACHNUMB = '" + bachNumb + "'"
                                    //Ordenamiento de los registros obternidos
                                    + " Order by CreateDate,BACHNUMB";
                    
                    using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                    {
                        using (SqlCommand sql_command = new SqlCommand(query, Conexion))
                        {
                            foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
                            {
                                Conexion.Open();
                                char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
                                string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
                                sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query
                                sql_command.CommandType = CommandType.Text;
                                SqlDataReader dr = sql_command.ExecuteReader();
                                while (dr.Read())
                                {
                                    clsResult_CuentaNomBuscar Cuenta = new clsResult_CuentaNomBuscar();
                                    Cuenta.jrnlidx = Int32.Parse(dr["JRNLIDX"].ToString());
                                    //Cuenta.Num = (rowCount++).ToString();//inserta un contador por cada nueva fila generada
                                    Cuenta.idPoliza = dr["IDPOLIZA"].ToString();
                                    Cuenta.CuentaGP = dr["CUENTAGP"].ToString();
                                    //Cuenta.CuentaWD = null;
                                    Cuenta.CuentaWD = (from c in dbSIF.PTONOM_CAT_CUENTAS
                                                       join cc in dbSIF.PTONOM_CAT_COSTCENTER on c.ID_CC equals cc.ID_CC
                                                       join la in dbSIF.PTONOM_CAT_LEDGERACCOUNT on c.ID_LA equals la.ID_LA
                                                       join sc in dbSIF.PTONOM_CAT_SPENDCATEGORY on c.ID_SC equals sc.ID_SC
                                                       where c.CUENTA == Cuenta.CuentaGP
                                                             && c.ESTATUS == true //se discriminan la cuenta que de lado del 'catalogo de cuentas' esta deshabilitada
                                                             && cc.ESTATUS == true //se discriminan la cuenta que de lado del 'catalogo CostCenter' esta deshabilitada
                                                             && la.ESTATUS == true //se discriminan la cuenta que de lado del 'catalogo LedgerAccount' esta deshabilitada 
                                                             && sc.ESTATUS == true //se discriminan la cuenta que de lado del 'catalogo SpéndCategory' esta deshabilitada 
                                                       select cc.CC + "-" + la.LA + "-" + sc.SC).FirstOrDefault();//obtiene cuentaGP desde la BD Fortia y su equivalente cuentaWD desde BD UDLAPSIF, siempre que su Estatus=true, de lo contrario devuelve nula la cuentaWD
                                    Cuenta.Debito = Decimal.Parse(dr["DEBITO"].ToString());
                                    Cuenta.Refrence = dr["REFRENCE"].ToString();
                                    Cuenta.FechaPoliza = DateTime.Parse(dr["FECHAPOLIZA"].ToString());
                                    Cuenta.ProcesadoConta = Boolean.Parse(dr["PROCESADOCONTABILIDAD"].ToString());
                                    Cuenta.EstausLayout = dr["ESTATUSLAYOUT"].ToString(); //Cuenta.CuentaWD == null ? "Error" : dr["ESTATUSLAYOUT"].ToString(); //Estatus = Error Este estatus es temporal y por eso no se envia un int, sino un string.
                                    Cuenta.FechaUpdate = DateTime.Parse(dr["FECHAUPDATE"].ToString());
                                    Cuenta.Anulada = Cuenta.EstausLayout == "Anulada" ? true : false;
                                    ListCuentasPrevia.Add(Cuenta);
                                }
                                sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                sql_command.Connection.Close();
                                Conexion.Close();
                            }
                            //Se ordena la lista de cuentas resultante en base al campo CuentaWD, para siempre mostrar las cuentas con error al inicio de la lista
                            var listaOrdenadaPorCuentaWD = ListCuentasPrevia.OrderBy(x => x.CuentaWD);//orden ascendente
                            //se enumera la lista
                            ListCuentas = listaOrdenadaPorCuentaWD.Select((element, indice) => new clsResult_CuentaNomBuscar
                            {
                                jrnlidx = element.jrnlidx,
                                Num = (indice + 1).ToString(),//enumera la lista
                                idPoliza = element.idPoliza,
                                CuentaGP = element.CuentaGP,
                                CuentaWD = element.CuentaWD,
                                Debito = element.Debito,
                                Refrence = element.Refrence,
                                FechaPoliza = element.FechaPoliza,
                                ProcesadoConta = element.ProcesadoConta,
                                EstausLayout = element.EstausLayout,
                                FechaUpdate = element.FechaUpdate,
                                Anulada = element.Anulada
                            }).ToList();

                            datos.Resultado = JsonConvert.SerializeObject(ListCuentas);//Lista resultante con las cuenta-GP y su correspondiente Cuenta-WD
                        }
                    }
                }
                else //ocurre cuando la lista 'ListDigCuentaGP' esta vacia
                {
                    ListCuentas.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(ListCuentas);
                    return datos;
                }
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "buscaCuentaNom", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";
            }
            return datos;
        }
        //clsConsultaCuentas
        //AnulaCuenta - Se anula por cuenta individual
        public clsResultado anulaCuentaNom(int jrnlidx, string ConexionNOMINAS, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            int filaActualizada = 0;//inicializa en cero la cantidad de registros anulados
            try
            {
                char anula = '2';//el valor 2 Anula una cuenta
                string upd_estatus = "UPDATE "
                                + "     " + tblSrcPoliciesNOM + ""

                                + " SET CtrlPPTO = '" + anula + "'," //se anula cambiando el Estatus de la cuenta a 2(Anulada)
                                + "     ModifyDate = CAST(GETDATE() AS DATE)" //establece en ModifyDate la fecha en la que se ejecuta el update

                                + " WHERE"
                                + " JRNLIDX = '" + jrnlidx + "'";

                using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                {
                    using (SqlCommand sql_command = new SqlCommand(upd_estatus, Conexion))
                    {
                            Conexion.Open();
                            filaActualizada = sql_command.ExecuteNonQuery();//total de registros anulados
                            sql_command.Connection.Close();
                            Conexion.Close();
                            datos.Resultado = JsonConvert.SerializeObject(filaActualizada);
                    }
                }
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "anulaPolizaNom", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";//Falla en el Update
            }

            return datos;
        }


        //clsLayoutNomina
        //obtenLayoutMultiplesNominas - Genera 
        //              A
        //              S
        //              S
        public clsResultado obtenLayoutMultiplesNominas(string fechaIni, string fechaFin, List<clsParamBachnumbRefrece> ParamsBachnumbRefrence, string estatusLayout, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            clsResult_LayoutNominaObten listLayout = new clsResult_LayoutNominaObten();
            clsResult_LayoutNominaObten listLayoutDeNominas = new clsResult_LayoutNominaObten();
            List<listParamsLayoutAmendmentEntryData> listLayoutDEBITS = new List<listParamsLayoutAmendmentEntryData>();
            List<listParamsLayoutAmendmentEntryData> listLayoutCREDITS = new List<listParamsLayoutAmendmentEntryData>();
            List<listParamsLayoutImportBudgetAmendment> listLayoutIBA = new List<listParamsLayoutImportBudgetAmendment>();
            int totalCuentasDestino = 0;
            int totalCuentasOrigen = 0;
            decimal totalDebitAmount = 0.00M;
            decimal totalCreditAmount = 0.00M;

            try
            {
                foreach (var param in ParamsBachnumbRefrence)
                {
                    datos = obtenLayout(fechaIni, fechaFin, param.bachnumb, param.refrence, estatusLayout, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
                    listLayout = JsonConvert.DeserializeObject<clsResult_LayoutNominaObten>(datos.Resultado);

                    listLayoutDEBITS.AddRange(listLayout.listLayoutAED.Where(x => x.BudgetDebitAmount > 0.00M));
                    listLayoutCREDITS.AddRange(listLayout.listLayoutAED.Where(x => x.BudgetCreditAmount > 0.00M));
                    listLayoutIBA = listLayout.listLayoutIBA;
                    //totalCuentasDestino += listLayout.totalCuentasDestino;
                    //totalCuentasOrigen += listLayout.totalCuentasOrigen;
                    totalDebitAmount += listLayout.totalDebitAmount;
                    totalCreditAmount += listLayout.totalCreditAmount;
                }

                var listDebitosAgrupados = from row in listLayoutDEBITS
                                           group new { row } by new
                                            {
                                                row.CostCenter,
                                                row.SpendCategory,
                                                row.LedgerAccount
                                            } into grupo
                                            orderby grupo.Key.CostCenter, grupo.Key.LedgerAccount, grupo.Key.SpendCategory
                                           select new listParamsLayoutAmendmentEntryData
                                            {
                                                Bachnumb = grupo.Select(field => field.row.Bachnumb).FirstOrDefault(),
                                                HeaderKey = grupo.Select(field => field.row.HeaderKey).FirstOrDefault(),
                                                LineKey = null,
                                                FiscalTimeInterval = grupo.Select(field => field.row.FiscalTimeInterval).FirstOrDefault(),
                                                CostCenter = grupo.Select(field => field.row.CostCenter).FirstOrDefault(),
                                                SpendCategory = grupo.Select(field => field.row.SpendCategory).FirstOrDefault(),
                                                RevenueCategory = grupo.Select(field => field.row.RevenueCategory).FirstOrDefault(),
                                                Project = grupo.Select(field => field.row.Project).FirstOrDefault(),
                                                LedgerAccount = grupo.Select(field => field.row.LedgerAccount).FirstOrDefault(),
                                                AccountSet_a = grupo.Select(field => field.row.AccountSet_a).FirstOrDefault(),
                                                LedgerAccountSummary = grupo.Select(field => field.row.LedgerAccountSummary).FirstOrDefault(),
                                                AccountSet_b = grupo.Select(field => field.row.AccountSet_b).FirstOrDefault(),
                                                BudgetCurrency = grupo.Select(field => field.row.BudgetCurrency).FirstOrDefault(),
                                                BookCode = grupo.Select(field => field.row.BookCode).FirstOrDefault(),
                                                BudgetDebitAmount = grupo.Sum(d => d.row.BudgetDebitAmount),
                                                BudgetCreditAmount = 0.00M,
                                                QuantityChange = grupo.Select(field => field.row.QuantityChange).FirstOrDefault(),
                                                Unit = grupo.Select(field => field.row.Unit).FirstOrDefault(),
                                                Memo = grupo.Select(field => field.row.Memo).FirstOrDefault()
                                            };
                listLayoutDEBITS = listDebitosAgrupados.ToList();
                totalCuentasDestino = listLayoutDEBITS.Count;


                var listCreditosAgrupados = from row in listLayoutCREDITS
                                            group new { row } by new
                                            {
                                                row.CostCenter,
                                                row.SpendCategory,
                                                row.LedgerAccount
                                            } into grupo
                                            orderby grupo.Key.LedgerAccount
                                            select new listParamsLayoutAmendmentEntryData
                                            {
                                                Bachnumb = grupo.Select(field => field.row.Bachnumb).FirstOrDefault(),
                                                HeaderKey = grupo.Select(field => field.row.HeaderKey).FirstOrDefault(),
                                                LineKey = null,
                                                FiscalTimeInterval = grupo.Select(field => field.row.FiscalTimeInterval).FirstOrDefault(),
                                                CostCenter = grupo.Select(field => field.row.CostCenter).FirstOrDefault(),
                                                SpendCategory = grupo.Select(field => field.row.SpendCategory).FirstOrDefault(),
                                                RevenueCategory = grupo.Select(field => field.row.RevenueCategory).FirstOrDefault(),
                                                Project = grupo.Select(field => field.row.Project).FirstOrDefault(),
                                                LedgerAccount = grupo.Select(field => field.row.LedgerAccount).FirstOrDefault(),
                                                AccountSet_a = grupo.Select(field => field.row.AccountSet_a).FirstOrDefault(),
                                                LedgerAccountSummary = grupo.Select(field => field.row.LedgerAccountSummary).FirstOrDefault(),
                                                AccountSet_b = grupo.Select(field => field.row.AccountSet_b).FirstOrDefault(),
                                                BudgetCurrency = grupo.Select(field => field.row.BudgetCurrency).FirstOrDefault(),
                                                BookCode = grupo.Select(field => field.row.BookCode).FirstOrDefault(),
                                                BudgetDebitAmount = 0.00M,
                                                BudgetCreditAmount = grupo.Sum(d => d.row.BudgetCreditAmount),
                                                QuantityChange = grupo.Select(field => field.row.QuantityChange).FirstOrDefault(),
                                                Unit = grupo.Select(field => field.row.Unit).FirstOrDefault(),
                                                Memo = grupo.Select(field => field.row.Memo).FirstOrDefault()
                                            };
                listLayoutCREDITS = listCreditosAgrupados.ToList();
                totalCuentasOrigen = listLayoutCREDITS.Count;

                var list_DebitsYCredits = listLayoutDEBITS
                                        .Concat(listLayoutCREDITS)
                                        .Select((elemento, indice) => new listParamsLayoutAmendmentEntryData
                                        {
                                            Bachnumb = elemento.Bachnumb,
                                            HeaderKey = elemento.HeaderKey,
                                            LineKey = indice + 1,
                                            FiscalTimeInterval = elemento.FiscalTimeInterval,
                                            CostCenter = elemento.CostCenter,
                                            SpendCategory = elemento.SpendCategory,
                                            RevenueCategory = elemento.RevenueCategory,
                                            Project = elemento.Project,
                                            LedgerAccount = elemento.LedgerAccount,
                                            AccountSet_a = elemento.AccountSet_a,
                                            LedgerAccountSummary = elemento.LedgerAccountSummary,
                                            AccountSet_b = elemento.AccountSet_b,
                                            BudgetCurrency = elemento.BudgetCurrency,
                                            BookCode = elemento.BookCode,
                                            BudgetDebitAmount = elemento.BudgetDebitAmount,
                                            BudgetCreditAmount = elemento.BudgetCreditAmount,
                                            QuantityChange = elemento.QuantityChange,
                                            Unit = elemento.Unit,
                                            Memo = elemento.Memo
                                        });

                listLayoutDeNominas.listLayoutIBA = listLayoutIBA;
                listLayoutDeNominas.listLayoutAED = list_DebitsYCredits.ToList();
                listLayoutDeNominas.totalCuentasDestino = totalCuentasDestino;
                listLayoutDeNominas.totalCuentasOrigen = totalCuentasOrigen;
                listLayoutDeNominas.totalDebitAmount = totalDebitAmount;
                listLayoutDeNominas.totalCreditAmount = totalCreditAmount;

                datos.Resultado = JsonConvert.SerializeObject(listLayoutDeNominas);

            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "obtenLayoutMultiplesNominas", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";
            }

            return datos;
        }


        //clsLayoutNomina
        //obtenLayout - Genera el archivo en Excel del LAYOUT de una Poliza para subir a Workday la provisión de cada cuenta de tipo Debito(destino) desde su correspondiente cuenta de Credito(origen)
        //              A travez de su parametro "estatusLayout" se indica a este metodo los tipos de cuentas que obtendra por poliza
        //              Si estatusLayout=Pendiente - Se usa para obtener las cuentas de una poliza que aun no ha sido procesada. Obteniendo todas las cuentas con estatus "Pendiente" y "no Procesadas por Contabilidad" (CtrlPPTO IS NULL & Active = 1)
        //              Si estatusLayout=Procesada - Se usa para obtener las cuentas de una poliza que aun si ha sido procesada. Obteniendo todas las cuentas con estatus "Procesada" y "sin filtrar por Procesadas por Contabilidad" (CtrlPPTO = 1)
        public clsResultado obtenLayout(string fechaIni, string fechaFin, string bachNumb, string paramDescription, string estatusLayout, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog) //string catParamsDescription,
        {
            int TotalCuentasDestino = 0;
            int TotalCuentasOrigen = 0;
            decimal TotalDebitAmount = 0.0M;
            decimal TotalCreditAmount = 0.0M;
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            clsResultado LayoutImportBudgetAmendment = new clsResultado();
            LayoutImportBudgetAmendment.Resultado = "Indefinido";
            clsResultado LayoutAmendmentEntryData = new clsResultado();
            LayoutAmendmentEntryData.Resultado = "Indefinido";

            List<char> ListDigCuentaGP = new List<char>();
            //ListDigCuentaGP = null;
            List<clsResult_CuentaNomBuscar> ListCuentas = new List<clsResult_CuentaNomBuscar>();
            //ListCuentas = null;
            List<listParamsLayoutAmendmentEntryData> listaCuentasDestinoParaLayout = new List<listParamsLayoutAmendmentEntryData>();
            listaCuentasDestinoParaLayout = null;
            List<listParamsLayoutAmendmentEntryData> listaCuentasOrigenParaLayout = new List<listParamsLayoutAmendmentEntryData>();
            listaCuentasOrigenParaLayout = null;
            List<cuentaWDorigenCREDIT> listCreditosPreviaSinAgrupar = new List<cuentaWDorigenCREDIT>();
            listCreditosPreviaSinAgrupar = null;
            List<cuentaWDorigenCREDIT> listCreditosAgrupada = new List<cuentaWDorigenCREDIT>();
            listCreditosAgrupada = null;
            List<listParamsLayoutAmendmentEntryData> ListLayoutAED = new List<listParamsLayoutAmendmentEntryData>();
            ListLayoutAED = null;
            List<listParamsLayoutImportBudgetAmendment> ListLayoutIBA = new List<listParamsLayoutImportBudgetAmendment>();
            //ListLayoutIBA = null;
            clsResult_LayoutNominaObten listLayoutDeNomina = new clsResult_LayoutNominaObten();
            //listLayoutDeNomina = null;

            try
            {
                //Inicia preparando la lista del CATALOGO DE PARAMETROS PARA ARMAR EL LAYOUT AmendmentEntryData 
                var ListCatalogoParamsAmendmentEntryData = Get_AmendmentEntryData_Params(rutaLog);

                //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
                string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

                using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
                {
                    using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
                    {
                        Conexion.Open();
                        SqlDataReader dr = sql_command.ExecuteReader();

                        while (dr.Read())
                        {
                            char digCnts; //= new char();
                            digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString());
                            ListDigCuentaGP.Add(digCnts);
                        }
                        sql_command.Connection.Close();
                        Conexion.Close();
                    }
                }

                //SEGUNDO, con la lista de prefijos del 'segmento cuenta' de cuentas-GP, se genera una lista de Cuentas-GP
                //De la BD Fortia genera una lista de cuentasGP con guiones intermedios candidatas a ser provisionadas. La lista no contiene cuentaWD
                //Cada registro contiene 3 columnas: Num | cuentaGP | debito
                //La lista generada solo es de cuentasGP PENDIENTES (Estatus = NULL)
                //La lista generada solo es de cuentasGP NO PROCESADAS CONTABILIDAD (Active = 1)
                //La lista generada va FILTRADA por rango de FECHAS INICIAL y FINAL
                //La lista generada contiene CuentasGP unicamente FILTRADAS por el BACHNUMB recibido por param
                if (ListDigCuentaGP.Count != 0)
                {
                    int rowCount = 1;//inicia contador para enumerar cada fila de todo el SqlDataReader(ListPolizas) generado con el query
                    string query = "SELECT "
                                    + "    CONCAT(SUBSTRING(ACTNUMBER,1,2),'-',SUBSTRING(ACTNUMBER,3,2),'-',SUBSTRING(ACTNUMBER,5,6),'-',SUBSTRING(ACTNUMBER,11,3),'-',SUBSTRING(ACTNUMBER,14,8)) as cuentaGP,"
                                    + "    DEBITAMT as debito"

                                    + " FROM " + tblSrcPoliciesNOM + ""

                                    + " WHERE"

                                    //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
                                    + " ACTNUMBER like @ACTNUMBER"
                                    //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
                                    + " AND DEBITAMT >= 0.00" //el operador >= permite obtener cuentas de debito con montos = 0.00 para los casos de Cuentas Especiales de RH
                                    //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
                                    + " AND CRDTAMT = 0.00";

                                    //Filtra cuentas-GP PENDIENTES & NO PROCESADAS POR CONTABILIDAD (CtrlPPTO IS NULL & Active=1) | o cuentas-GP solo PROCESADAS (CtrlPPTO = 1)
                            query += estatusLayout == "Pendiente" ? " AND CtrlPPTO IS NULL      AND Active = 1" : " AND CtrlPPTO = 1";

                                    //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
                            query += " AND (CreateDate BETWEEN"
                                    + "    CASE"
                                    + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
                                    + "        ELSE '" + fechaIni + "'"
                                    + "    END"
                                    + "    AND"
                                    + "    CASE"
                                    + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
                                    + "    ELSE '" + fechaFin + "'"
                                    + "    END)"
                                    //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
                                    + " AND BACHNUMB = '" + bachNumb + "'"
                                    //Ordenamiento de los registros obternidos
                                    + " Order by cuentaGP";

                    using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                    {
                        using (SqlCommand sql_command = new SqlCommand(query, Conexion))
                        {
                            foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
                            {
                                Conexion.Open();
                                //rowCount++; este contador solo enumera las veces que entra en el foreach, pero no las veces que entra en el while del Reader
                                char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
                                string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
                                sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query
                                sql_command.CommandType = CommandType.Text;
                                SqlDataReader dr = sql_command.ExecuteReader();
                                while (dr.Read())
                                {
                                    clsResult_CuentaNomBuscar Cuenta = new clsResult_CuentaNomBuscar();
                                    Cuenta.Num = (rowCount++).ToString();//inserta un contador por cada nueva fila generada
                                    Cuenta.CuentaGP = dr["cuentaGP"].ToString();
                                    Cuenta.Debito = Decimal.Parse(dr["debito"].ToString());
                                    ListCuentas.Add(Cuenta);
                                }
                                sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                sql_command.Connection.Close();
                                Conexion.Close();
                            }
                            //la siguiente linea no se requiere. Solo es para TEST
                            datos.Resultado = JsonConvert.SerializeObject(ListCuentas);//Lista resultante con las cuenta-GP y su correspondiente Cuenta-WD
                        }
                    }
                }
                else //ocurre cuando la lista 'ListDigCuentaGP' esta vacia
                {
                    ListCuentas.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(ListCuentas); //Esta linea no se requiere. Solo es para TEST
                    return datos;
                }

                /****Inicia el proceso para generar las cuentas de debito o cuentas destino, es decir las cuentas de la nomina que seran provisionadas****/

                //TERCERO, se obtiene la lista depurada de todas las cuentasWD que seran provisionadas 
                //Cada registro se coloca en la columna correspondiente al layout AmendmentEntryData
                //1 Se cruza la lista 'ListCuentas' de cuentasGP, con el 'Catalogo de Cuentas' para obtener su correspondiente CUENTA de debito o destino de WORKDAY
                //2 Se depura la lista resultante, eliminando cuentasWD DUPLICADAS para sumarizar sus montos en una sola cuenta por cada cuenta duplicada
                //3 Se quitan todas las cuentas de VALES de despensa
                //4 Se quitan todas las cuentas de CREDITO u origen
                //5 Se identifica y separa en la misma lista las cuentasWD con posible PROYECTO asignado
                if ((ListCuentas != null || ListCuentas.Any()) && (ListCatalogoParamsAmendmentEntryData != null || ListCatalogoParamsAmendmentEntryData.Any()))
                {
                    var ListDebitosPrevia = from f in ListCuentas
                                join c in dbSIF.PTONOM_CAT_CUENTAS on f.CuentaGP equals c.CUENTA//usando como referencia las CuentaGP se relaciona una cuentaGP con su cuentaWD
                                join cc in dbSIF.PTONOM_CAT_COSTCENTER on c.ID_CC equals cc.ID_CC
                                join la in dbSIF.PTONOM_CAT_LEDGERACCOUNT on c.ID_LA equals la.ID_LA
                                join sc in dbSIF.PTONOM_CAT_SPENDCATEGORY on c.ID_SC equals sc.ID_SC
                                //join pr in dbSIF.PTONOM_CAT_PROJECT on c.IDPROJECT equals pr.IDPROJECT
                                where c.VALES == false //discrimina todas las cuentas que son de tipo Vales de Despensa, donde VALES=1(si es cuenta de vales) VALES=0(no es cuenta de vales)
                                      && c.CUENTAESPECIAL == false //Al obtener solo las CuentaEspecial apagadas(false) discrimina todas las cuentas marcadas como CuentaEspecial. CuentaEspecial =1(si es CuentaEspecial) CuentaEspecial =0(no es CuentaEspecial)
                                      && c.IDTIPOCUENTA == 1 //discrimina todas las cuentasWD de tipo CREDITO(origen), para dejar una lista unica de cuentas de debito(destino)
                                      && c.ESTATUS == true //cuando la cuentaWD relacionada esta Activa en catalogo de Cuentas
                                      && cc.ESTATUS == true //cuando el CostCenter relacionado esta Activo en catalogo CostCenter
                                      && la.ESTATUS == true //cuando el LedgerAccount relacionado esta Activo en catalogo LedgerAccount
                                      && sc.ESTATUS == true //cuando el SpendCategory relacionado esta Activo en catalogo SpendCategory
                                group new { cc, la, sc, c, f } by new //pr, 
                                {
                                    cc.CC,
                                    la.LA,
                                    sc.SC,
                                    //c,
                                    c.IDPROJECT
                                } into grupo
                                orderby grupo.Key.CC
                                select new cuentaWDdestinoSinValesYsinCredits
                                {
                                    CostCenter = grupo.Key.CC,
                                    LedgerAccount = grupo.Key.LA,
                                    SpendCategory = grupo.Key.SC,
                                    Debito = grupo.Sum(d => d.f.Debito),
                                    BudgetCurrency = grupo.Where(catCu => catCu.c.PTONOM_CAT_CURRENCY.IDCURRENCY == catCu.c.IDCURRENCY)
                                                          .Select(catCy => catCy.c.PTONOM_CAT_CURRENCY.CODE).FirstOrDefault(),//obtiene el code de la moneda, es decir el nombre abreviado de la moneda
                                    //BudgetCurrency = grupo.Select(cy => cy.c.IDCURRENCY).FirstOrDefault().ToString(),
                                    idProject = grupo.Select(pr => pr.c.IDPROJECT).DefaultIfEmpty(null).FirstOrDefault(),
                                };
                    //se itera la lista ListDebitosPrevia para poder asignar cada parte de una cuentaWD a sus correspondientes campos del layout AmendmentEntryData
                    //tambien se cruza y agrupa por idProject para poder mostrar en la misma lista las cuentas 'sin proyecto' y con las posibles cuentas 'con proyecto'
                    var query = from f in ListDebitosPrevia
                                join pr in dbSIF.PTONOM_CAT_PROJECT on f.idProject equals pr.IDPROJECT
                                into grupoProjects
                                from pr in grupoProjects.DefaultIfEmpty() //este es un Left-Join para obtener todos los registros donde PTONOM_CAT_PROJECT.idProject IS NULL
                                orderby f.CostCenter
                                select new listParamsLayoutAmendmentEntryData //cuentaWDdestinoSinValesYsinCredits
                                {
                                    HeaderKey = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 1).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=1 -> Header key
                                    LineKey = null,// SIN VALOR DESDE EL CATALOGO (Este valor siempre inicia en 1 y aumenta de uno en uno)
                                    FiscalTimeInterval = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 18).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=18 -> Fiscal time interval
                                    CostCenter = f.CostCenter,
                                    SpendCategory = f.SpendCategory,
                                    RevenueCategory = null,// SIN VALOR
                                    Project = f.idProject != null ? pr.PROJECT : null,//Cuentas de Debito(destino) sí puede tener Proyectos
                                    LedgerAccount = f.LedgerAccount,
                                    AccountSet_a = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 24).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=24 -> Account set
                                    LedgerAccountSummary = null,// SIN VALOR
                                    AccountSet_b = null, // SIN VALOR (segunda Columna con titulo AccountSet)
                                    BudgetCurrency = f.BudgetCurrency,
                                    BookCode = null, // SIN VALOR
                                    BudgetDebitAmount = f.Debito,
                                    BudgetCreditAmount = 0.00M,
                                    QuantityChange = null,// SIN VALOR
                                    Unit = null,// SIN VALOR
                                    Memo = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 33).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=33 -> Memo (este valor se captura desde pantalla antes de enviar el Layout al excel y a la tabla de guardado de Layouts)
                                };
                    listaCuentasDestinoParaLayout = query.ToList();
                    //hasta aqui se obtienen todas las cunetasWD que seran provisionadas "exluyendo" las cuentas de CREDITO(origen) del CostCenter "CC_134" de RH
                    TotalCuentasDestino = listaCuentasDestinoParaLayout.Count;//se calcula la cantidad de Cuentas de Debito(Destino) que el Layout de la Nomina tendra
                    TotalDebitAmount = listaCuentasDestinoParaLayout.Sum(x => x.BudgetDebitAmount);//se calcula el total del monto de Debito(Destino) a ser pagado
                    datos.Resultado = JsonConvert.SerializeObject(listaCuentasDestinoParaLayout);//Esta linea no se requiere. Solo es para TEST
                }
                else //ocurre cuando la lista 'listaCuentasDestinoParaLayout' esta vacia debido a que no se encontro ningunna cuenta
                {
                    listaCuentasDestinoParaLayout.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(listaCuentasDestinoParaLayout);//Esta linea no se requiere. Solo es para TEST
                    return datos;
                }

                /**** Inicia el proceso para generar las cuentas de credito o cuentas de origen de pago de la nomina ****/
                //El siguiente metodo obtienen unicamente las cuentas de tipo Origen o Credito sin montos. Hasta el momento solo son 3 cuentas origen
                //Con estas 3 cuentas se agrupara toda la lista de cuentas destino en 3 grupos es decir 3 cuentas origen

                var ListCuentasOrigen = GetListLedgAcctCredit(rutaLog);

                //CUARTO, genera lista unicamente de cuentas de debito. Se agrega un campo con el prefijo del Ledger Account al que pertenece cada cuenta. 
                //La lista resultante tiene solo cuentas Destino, pero pasa a ser llamada 'Lista de cuentas de Credito (Origen)'
                //1 Se reutiliza la Lista 'ListCuentas' generada en el segundo paso, para generar una lista de cuentas de DEBITO
                //2 Se cruza la lista 'ListCuentas' de cuentasGP, con el 'Catalogo de Cuentas' para obtener sus scorrespondiente cuentas de debito
                //3 Se depura la lista resultante, eliminando cuentasWD duplicadas para sumarizar sus montos en una sola cuenta por cada cuenta duplicada
                //4 Se quitan todas las cuentas de VALES de despensa
                //5 Se quitan todas las cuentas de CREDITO u origen
                //6 Se toma el mismo Ledger Account de cada registro, se le hace substring, y el prefijo resultante de 3 digitor se coloca para cada cuenta de la lista
                //Cada registro resultante contiene 6 columnas: Prefijo-LedgerAccount | CostCenter| LedgerAccount | SpendCategory | Credit | BudgetCurrency
                if (ListCuentas != null || ListCuentas.Any())
                {
                    var query = from f in ListCuentas
                                join c in dbSIF.PTONOM_CAT_CUENTAS on f.CuentaGP equals c.CUENTA
                                join cc in dbSIF.PTONOM_CAT_COSTCENTER on c.ID_CC equals cc.ID_CC
                                join la in dbSIF.PTONOM_CAT_LEDGERACCOUNT on c.ID_LA equals la.ID_LA
                                join sc in dbSIF.PTONOM_CAT_SPENDCATEGORY on c.ID_SC equals sc.ID_SC
                                where c.VALES == false //discrimina todas las cuentas que son de tipo Vales de Despensa, donde VALES=1(si es cuenta de vales) VALES=0(no es cuenta de vales)
                                      && c.CUENTAESPECIAL == false //Al obtener solo las CuentaEspecial apagadas(false) discrimina todas las cuentas marcadas como CuentaEspecial. CuentaEspecial =1(si es CuentaEspecial) CuentaEspecial =0(no es CuentaEspecial)
                                      && c.IDTIPOCUENTA == 1 //Para calcular el monto de las cuentas Origen se requiere quitar las 3 cuentas de tipo CREDITO(origen) | IDTIPOCUENTA=1(obtiene solo cnts Debito) IDTIPOCUENTA=2(obtiene solo cnts Credito)
                                      && c.ESTATUS == true //cuando la cuentaWD relacionada esta Activa en catalogo de Cuentas
                                      && cc.ESTATUS == true //cuando el CostCenter relacionado esta Activo en catalogo CostCenter
                                      && la.ESTATUS == true //cuando el LedgerAccount relacionado esta Activo en catalogo LedgerAccount
                                      && sc.ESTATUS == true //cuando el SpendCategory relacionado esta Activo en catalogo SpendCategory
                                group new { cc, la, sc, c, f } by new // c, pr, 
                                {
                                    cc.CC,
                                    la.LA,
                                    sc.SC,
                                    c.IDPROJECT //esta key del group es inecesaria ya que no la estoy usando en el Select de abajo, por lo tanto hay que revisar si puedo eliminarla
                                } into grupo
                                orderby grupo.Key.CC
                                select new cuentaWDorigenCREDIT
                                {
                                    prefixLedgAcct = grupo.Key.LA.Substring(0, 3),
                                    CostCenter = grupo.Key.CC,
                                    LedgerAccount = grupo.Key.LA,
                                    SpendCategory = grupo.Key.SC,
                                    Credit = grupo.Sum(credit => credit.f.Debito),//la suma de todos los DEBITOS pasan a ser nombrados de CREDITOS asignandose a la propied 'Credit'
                                    BudgetCurrency = grupo.Where(catCu => catCu.c.PTONOM_CAT_CURRENCY.IDCURRENCY == catCu.c.IDCURRENCY).Select(catCy => catCy.c.PTONOM_CAT_CURRENCY.CODE).FirstOrDefault(),
                                };
                    listCreditosPreviaSinAgrupar = query.ToList();
                    //Hasta aqui ya se tiene el monto total usado para provisionar todas las cunetas destino, falta agrupar este total en 3 grupos o 3 cuentas origen.
                    //Anteriormente aqui se usaba -> TotalDebitAmount = listCreditosPreviaSinAgrupar.Sum(x => x.Credit) <- para calcula el total del monto de Debito(Destino)
                    datos.Resultado = JsonConvert.SerializeObject(listCreditosPreviaSinAgrupar);//esta linea no se requiere. Solo es para TEST
                }
                else //ocurre cuando la lista 'ListCuentas' esta vacia debido a que el bachnumb no tiene cuentas o sus cuentas no cumplen con los filtros
                {
                    listCreditosPreviaSinAgrupar.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(listCreditosPreviaSinAgrupar);//Esta linea no se requiere. Solo es para TEST
                    return datos;
                }

                //QUINTO toda la 'listCreditosPreviaSinAgrupar' se agrupa por el prefijo de LedgerAccount de cada Cuenta origen 
                //previamente cada cuenta Origen fue capturada e identificada en el catalogo de Cuentas. Actualmente hay 3 cuentas de tipo Credito (Origen)
                //1 Cruzar las cuentas de la lista 'listCreditosPreviaSinAgrupar'(que contiene todas las cuentas de debitos) vs. la 'ListCuentasOrigen'(actualmente son 3 cnts)
                //2 filtra a travez del prefijo prefixLedgAcct que tienen en comun las 2 listas cruzadas
                //3 se unifican todas las cuentas de la lista listCreditosPreviaSinAgrupar a travez del prefijo de sus Ledger Account prefixLedgAcct y se sumarizan sus montos y todos pasan a ser de 'Credito'
                //4 se asigna cada una de las 3 cuentaWD de Credito u Origen (seccionada por sus 3 componentes) a su correspondiente grupo de los 3 perfiles de LedgerAccount
                if ( (ListCuentasOrigen != null || ListCuentasOrigen.Any()) && (listCreditosPreviaSinAgrupar != null || listCreditosPreviaSinAgrupar.Any()) )
                {
                    var query = from lco in ListCuentasOrigen
                                join lcp in listCreditosPreviaSinAgrupar on lco.prefixLedgAcct equals lcp.prefixLedgAcct
                                where lco.prefixLedgAcct == lcp.prefixLedgAcct
                                group new { lco, lcp } by new
                                {
                                    lco.prefixLedgAcct
                                } into grupo
                                select new cuentaWDorigenCREDIT
                                {
                                    prefixLedgAcct = grupo.Key.prefixLedgAcct,
                                    CostCenter = grupo.Where(co => co.lco.prefixLedgAcct == grupo.Key.prefixLedgAcct).Select(x => x.lco.CostCenter).FirstOrDefault(),
                                    LedgerAccount = grupo.Where(co => co.lco.prefixLedgAcct == grupo.Key.prefixLedgAcct).Select(x => x.lco.LedgerAccount).FirstOrDefault(),
                                    SpendCategory = grupo.Where(co => co.lco.prefixLedgAcct == grupo.Key.prefixLedgAcct).Select(x => x.lco.SpendCategory).FirstOrDefault(),
                                    Credit = grupo.Sum(c => c.lcp.Credit),
                                    BudgetCurrency = grupo.Select(cy => cy.lcp.BudgetCurrency).FirstOrDefault()//no se requiere ToString()
                                };                   

                    listCreditosAgrupada = query.ToList();
                    TotalCuentasOrigen = listCreditosAgrupada.Count;//se calcula la cantidad de Cuentas de Credito(Origen) que el Layout de la Nomina tendra 
                    TotalCreditAmount = listCreditosAgrupada.Sum(x => x.Credit);//se calcula el total del monto de Credito(Origen) con el que se provisionaran todas las cuentas destino
                    datos.Resultado = JsonConvert.SerializeObject(listCreditosAgrupada);//Esta linea no se requiere. Solo es para TEST
                }
                else
                {
                    listCreditosAgrupada.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(listCreditosAgrupada);//Esta linea no se requiere. Solo es para TEST
                    return datos;
                }


                //SEXTO, se agrega las 3 Cuentas de Credito(Origen) a la estructura del layout AmendmentEntryData
                //se itera la lista agrupada 'listCreditosAgrupada' para poder asignar cada parte de una cuentaWD a sus correspondientes campos del layout AmendmentEntryData
                //tambien se asignan sus parametros a sus correspondientes campos haciendo subqueries hacia el catalogo de Parametros 
                if ((listCreditosAgrupada != null || listCreditosAgrupada.Any()) && (ListCatalogoParamsAmendmentEntryData != null || ListCatalogoParamsAmendmentEntryData.Any()))
                {
                    var query = from lc in listCreditosAgrupada
                                select new listParamsLayoutAmendmentEntryData
                                {
                                    HeaderKey = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 1).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=1 -> Header key
                                    LineKey = null,// SIN VALOR DESDE EL CATALOGO (Este valor siempre inicia en 1 y aumenta de uno en uno)
                                    FiscalTimeInterval = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 18).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=18 -> Fiscal time interval
                                    CostCenter = lc.CostCenter,
                                    SpendCategory = lc.SpendCategory,
                                    RevenueCategory = null,// SIN VALOR
                                    Project = null,//para las cuentas de Credito (origen) no hay Proyectos
                                    LedgerAccount = lc.LedgerAccount,
                                    AccountSet_a = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 24).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=24 -> Account set
                                    LedgerAccountSummary = null,// SIN VALOR
                                    AccountSet_b = null, // SIN VALOR (segunda Columna con titulo AccountSet)
                                    BudgetCurrency = lc.BudgetCurrency,
                                    BookCode = null, // SIN VALOR
                                    BudgetDebitAmount = 0.00M,
                                    BudgetCreditAmount = lc.Credit,
                                    QuantityChange = null,// SIN VALOR
                                    Unit = null,// SIN VALOR
                                    Memo = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 33).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=33 -> Memo (este valor se captura desde pantalla antes de enviar el Layout al excel y a la tabla de guardado de Layouts)
                                };
                    listaCuentasOrigenParaLayout = query.ToList();
                    datos.Resultado = JsonConvert.SerializeObject(listaCuentasOrigenParaLayout);//Esta linea no se requiere. Solo es para TEST
                }
                else
                {
                    listaCuentasOrigenParaLayout.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(listaCuentasOrigenParaLayout);//Esta linea no se requiere. Solo es para TEST
                    return datos;
                }

                //SEPTIMO, se integran las dos secciones, las cuentas Debito(destino) y las cuentas Credito(origen), al layout AmendmentEntryData
                //1 Se concatenan las dos listas, dejando primero la lista 'listaCuentasDestinoParaLayout'(cuentas destino) y al final la listaCuentasOrigenParaLayout(cuentas origen)
                //2 Se genera el numero consecutivo del campo LineKey y se agregan todos los campos correspondientes
                if ((listaCuentasDestinoParaLayout != null || listaCuentasDestinoParaLayout.Any()) && (listaCuentasOrigenParaLayout != null || listaCuentasOrigenParaLayout.Any()))
                {
                    var query = listaCuentasDestinoParaLayout
                                .Concat(listaCuentasOrigenParaLayout)
                                .Select((elemento, indice) => new listParamsLayoutAmendmentEntryData
                                {
                                    Bachnumb = bachNumb,//esta es la poliza que viene en el parameto del metodo obtenLayout()
                                    HeaderKey = elemento.HeaderKey,
                                    LineKey = indice + 1,
                                    FiscalTimeInterval = elemento.FiscalTimeInterval,
                                    CostCenter = elemento.CostCenter,
                                    SpendCategory = elemento.SpendCategory,
                                    RevenueCategory = elemento.RevenueCategory,
                                    Project = elemento.Project,
                                    LedgerAccount = elemento.LedgerAccount,
                                    AccountSet_a = elemento.AccountSet_a,
                                    LedgerAccountSummary = elemento.LedgerAccountSummary,
                                    AccountSet_b = elemento.AccountSet_b,
                                    BudgetCurrency = elemento.BudgetCurrency,
                                    BookCode = elemento.BookCode,
                                    BudgetDebitAmount = elemento.BudgetDebitAmount,
                                    BudgetCreditAmount = elemento.BudgetCreditAmount,
                                    QuantityChange = elemento.QuantityChange,
                                    Unit = elemento.Unit,
                                    Memo = elemento.Memo
                                });
                    ListLayoutAED = query.ToList();
                    //TotalCuentasDestino = ListLayoutAED.Count - TotalCuentasOrigen;//se calcula la cantidad de Cuentas de Debito(Destino) que el Layout de la Nomina tendra
                    //TotalDebitAmount = ListLayoutAED.Sum(x => x.BudgetDebitAmount);//se calcula el total del monto de Debito(Destino) a ser pagado
                    LayoutAmendmentEntryData.Resultado = JsonConvert.SerializeObject(ListLayoutAED);//Esta linea no se requiere. Solo es para TEST
                }
                else
                {
                    listaCuentasOrigenParaLayout.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(listaCuentasOrigenParaLayout);//Esta linea no se requiere. Solo es para TEST
                    return datos;
                }

                
                //Se obtiene la lista del CATALOGO DE PARAMETROS PARA ARMAR EL LAYOUT AmendmentEntryData
                var ListCatalogoParamsImportBudgetAmendment = Get_ImportBudgetAmendment_Params(rutaLog);

                //OCTAVO, se arma el Layout "Import Budget Amendment" el cual unicamente contendra una unica fila con los valores de sus parametros
                if ((ListCatalogoParamsImportBudgetAmendment != null || ListCatalogoParamsImportBudgetAmendment.Any())) //dbSIF.PTONOM_CAT_PARAMETERS != null || dbSIF.PTONOM_CAT_PARAMETERS.Any()
                {
                    listParamsLayoutImportBudgetAmendment ListaLayoutIBA = new listParamsLayoutImportBudgetAmendment();

                    ListaLayoutIBA.Bachnumb = bachNumb;//case 0: esta es la poliza que viene en el parameto del metodo obtenLayout()
                    ListaLayoutIBA.Description = paramDescription;//case 14: el param DESCRIPTION es el valor de la REFRENCE de toda la poliza
                    foreach (var item in ListCatalogoParamsImportBudgetAmendment)
                    {
                        switch (item.IdParam)
                        {
                            //case 0: ListaLayoutIBA.Bachnumb = bachNumb; break;//esta es la poliza que viene en el parameto del presente metodo obtenLayout()
                            case 1: ListaLayoutIBA.HeaderKey = item.Valor; break;
                            case 2: ListaLayoutIBA.AddOnly = item.Valor; break;
                            case 3: ListaLayoutIBA.BudgetAmendment = null; break;
                            case 4: ListaLayoutIBA.AutoComplete = item.Valor; break;
                            case 5: ListaLayoutIBA.Comment = null; break;
                            case 6: ListaLayoutIBA.Worker = null; break;
                            case 7: ListaLayoutIBA.Id = null; break;
                            case 8: ListaLayoutIBA.Submit = item.Valor; break;
                            case 9: ListaLayoutIBA.CompanyOrCompanyHierarchy = item.Valor; break;
                            case 10: ListaLayoutIBA.BudgetStructure = item.Valor; break;
                            case 11: ListaLayoutIBA.BudgetName = item.Valor; break;
                            case 12: ListaLayoutIBA.FiscalYear = item.Valor; break;
                            case 13: ListaLayoutIBA.AmendmentDate = item.Valor; break;
                            //case 14: ListaLayoutIBA.Description = paramDescription; break;//el param DESCRIPTION es el valor de la REFRENCE de toda la poliza
                            case 15: ListaLayoutIBA.BudgetAmendmentType = item.Valor; break;
                            case 16: ListaLayoutIBA.BalancedAmendment = null; break;
                        }
                    }
                    //List<paramsImportBudgetAmendment> ListLayoutIBA = new List<paramsImportBudgetAmendment> { ListaLayoutIBA };
                    ListLayoutIBA.Add(ListaLayoutIBA);
                    LayoutImportBudgetAmendment.Resultado = JsonConvert.SerializeObject(ListLayoutIBA);//Esta linea no se requiere. Solo es para TEST
                }
                else
                {
                    listaCuentasOrigenParaLayout.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(listaCuentasOrigenParaLayout);//Esta linea no se requiere. Solo es para TEST
                    return datos;
                }

                //NOVENO, se serializa dentro de una Lista de tipo clsResult_LayoutNominaObten todos los 6 valores del Layout final
                if ((ListLayoutIBA != null || ListLayoutIBA.Any()) && (ListLayoutAED != null || ListLayoutAED.Any()))
                {
                    listLayoutDeNomina.listLayoutIBA = ListLayoutIBA;
                    listLayoutDeNomina.listLayoutAED = ListLayoutAED;
                    listLayoutDeNomina.totalCuentasDestino = TotalCuentasDestino;
                    listLayoutDeNomina.totalCuentasOrigen = TotalCuentasOrigen;
                    listLayoutDeNomina.totalDebitAmount = TotalDebitAmount;
                    listLayoutDeNomina.totalCreditAmount = TotalCreditAmount;
                    datos.Resultado = JsonConvert.SerializeObject(listLayoutDeNomina);
                }
                else
                {
                    listLayoutDeNomina = null;
                    datos.Resultado = JsonConvert.SerializeObject(listLayoutDeNomina);
                    return datos;
                }
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "obtenLayout", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";
            }

            return datos;
        }

        //clsLayoutNomina
        //cambiaStatusPolizaProcesada - Este metodo PROCESA (simplemente cambiando el CtrlPPTO a 1) las cuentas de una Poliza, siempre y cuando esten Pendientes y No hayan sido Procesadas por Conta
        //                              Cambia estatus de cada cuenta, por Poliza recibida por parametro.
        //                              Cambia su estatus de PENDIENTES(y tambien las que en el mismo grupo ya esten PROCESADAS) a PROCESADAS de todas las cuentas que pertenescan a la Poliza 
        //                              incluyendo toda cuenta con y sin proyecto, cuentas especiales si es que estubieran incluidas, cuentas de Vales y de Origen.
        public clsResultado cambiaStatusPolizaProcesada(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            List<char> ListDigCuentaGP = new List<char>();
            int filasActualizadas = 0; //inicializa en cero la cantidad de registros que cambiaron d estatus a Procesados

            string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

            try
            {
                //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
                using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
                {
                    using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
                    {
                        Conexion.Open();
                        SqlDataReader dr = sql_command.ExecuteReader();

                        while (dr.Read())
                        {
                            char digCnts; //= new char();
                            digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString()); // en caso de que el catalogo este vacio se produce el error: "String must be exactly one character long"
                            ListDigCuentaGP.Add(digCnts); //nunca enviara una lista vacia. En caso de estarvacia antes se genera caera en el Catch, enviando un NULL al front
                        }
                        sql_command.Connection.Close();
                        Conexion.Close();
                    }
                }

                //SEGUNDO, con la lista de los prefijos(el primer numero) del 'segmento cuenta' de las cuentas-GP, se cambia el estatus de cada cuenta a PROCESADA
                if (ListDigCuentaGP.Count != 0) //Este IF esta sobrando ya que nunca recibira una lista sin registros, antes generaria un error
                {
                    char procesada = '1';//el valor 1 cambia una cuenta a Layout PROCESADA
                    string upd_estatus = "UPDATE "
                                    + "     " + tblSrcPoliciesNOM + ""

                                    + " SET CtrlPPTO = '" + procesada + "'," //se cambiando el Estatus de cada cuenta a 1(Procesada)
                                    + "     ModifyDate = CAST(GETDATE() AS DATE)" //establece en ModifyDate la fecha en la que se ejecuta el update

                                    + " WHERE"

                                    //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
                                    + " ACTNUMBER like @ACTNUMBER"
                                    //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
                                    + " AND DEBITAMT >= 0.00" //el operador >= permite obtener cuentas de debito con montos = 0.00 para los casos de Cuentas Especiales de RH
                                    //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
                                    + " AND CRDTAMT = 0.00"
                                    //Filtra cuentas-GP con estatus de layout PROCESADA o PENDIENTE (PENDIENTE = NULL || PROCESADA = 1)
                                    + " AND (CtrlPPTO IS NULL OR CtrlPPTO = 1)"
                                    //Filtra cuentas-GP solo las que NO han sido procedadas o contabilizadas por Contabilidad(Active = 1)
                                    + " AND Active = 1"
                                    //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
                                    + " AND (CreateDate BETWEEN"
                                    + "    CASE"
                                    + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
                                    + "        ELSE '" + fechaIni + "'"
                                    + "    END"
                                    + "    AND"
                                    + "    CASE"
                                    + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
                                    + "    ELSE '" + fechaFin + "'"
                                    + "    END)"

                                    //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
                                    + " AND BACHNUMB = '" + bachNumb + "'";

                    using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                    {
                        using (SqlCommand sql_command = new SqlCommand(upd_estatus, Conexion))
                        {
                            foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
                            {
                                Conexion.Open();
                                char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
                                string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
                                sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query

                                int Actualizadas = sql_command.ExecuteNonQuery();
                                filasActualizadas += Actualizadas; //total de cuentas-GP procesadas

                                sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                sql_command.Connection.Close();
                                Conexion.Close();
                            }

                            datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                        }
                    }
                }
                else //ocurre cuando la lista 'ListDigCuentaGP' esta vacia
                {
                    filasActualizadas = 0;
                    datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                    return datos;
                }
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "cambiaStatusPolizaProcesada", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";//Falla en el Update
            }

            return datos;
        }

        //clsLayoutNomina
        //cambiaStatusMultiplesPolizasProcesada - Este metodo PROCESA (simplemente cambiando el CtrlPPTO a 1) las cuentas de una Poliza, siempre y cuando esten Pendientes y No hayan sido Procesadas por Conta
        //                              Cambia estatus de cada cuenta, por Poliza recibida por parametro.
        //                              Cambia su estatus de PENDIENTES(y tambien las que en el mismo grupo ya esten PROCESADAS) a PROCESADAS de todas las cuentas que pertenescan a la Poliza 
        //                              incluyendo toda cuenta con y sin proyecto, cuentas especiales si es que estubieran incluidas, cuentas de Vales y de Origen.
        public clsResultado cambiaStatusMultiplesPolizasProcesada(string fechaIni, string fechaFin, List<string> listProcesadaMPBachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            List<char> ListDigCuentaGP = new List<char>();
            int filasActualizadas = 0; //inicializa en cero la cantidad de registros que cambiaron d estatus a Procesados

            string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

            try
            {
                //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
                using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
                {
                    using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
                    {
                        Conexion.Open();
                        SqlDataReader dr = sql_command.ExecuteReader();

                        while (dr.Read())
                        {
                            char digCnts; //= new char();
                            digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString()); // en caso de que el catalogo este vacio se produce el error: "String must be exactly one character long"
                            ListDigCuentaGP.Add(digCnts); //nunca enviara una lista vacia. En caso de estarvacia antes se genera caera en el Catch, enviando un NULL al front
                        }
                        sql_command.Connection.Close();
                        Conexion.Close();
                    }
                }

                //SEGUNDO, con la lista de los prefijos(el primer numero) del 'segmento cuenta' de las cuentas-GP, se cambia el estatus de cada cuenta a PROCESADA
                if (ListDigCuentaGP.Count != 0) //Este IF esta sobrando ya que nunca recibira una lista sin registros, antes generaria un error
                {
                    foreach (var bachNumb in listProcesadaMPBachNumb)
                    {
                        char procesada = '1';//el valor 1 cambia una cuenta a Layout PROCESADA
                        string upd_estatus = "UPDATE "
                                        + "     " + tblSrcPoliciesNOM + ""

                                        + " SET CtrlPPTO = '" + procesada + "'," //se cambiando el Estatus de cada cuenta a 1(Procesada)
                                        + "     ModifyDate = CAST(GETDATE() AS DATE)" //establece en ModifyDate la fecha en la que se ejecuta el update

                                        + " WHERE"

                                        //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
                                        + " ACTNUMBER like @ACTNUMBER"
                                        //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
                                        + " AND DEBITAMT >= 0.00" //el operador >= permite obtener cuentas de debito con montos = 0.00 para los casos de Cuentas Especiales de RH
                                                                  //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
                                        + " AND CRDTAMT = 0.00"
                                        //Filtra cuentas-GP con estatus de layout PROCESADA o PENDIENTE (PENDIENTE = NULL || PROCESADA = 1)
                                        + " AND (CtrlPPTO IS NULL OR CtrlPPTO = 1)"
                                        //Filtra cuentas-GP solo las que NO han sido procedadas o contabilizadas por Contabilidad(Active = 1)
                                        + " AND Active = 1"
                                        //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
                                        + " AND (CreateDate BETWEEN"
                                        + "    CASE"
                                        + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
                                        + "        ELSE '" + fechaIni + "'"
                                        + "    END"
                                        + "    AND"
                                        + "    CASE"
                                        + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
                                        + "    ELSE '" + fechaFin + "'"
                                        + "    END)"

                                        //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
                                        + " AND BACHNUMB = '" + bachNumb + "'";

                        using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                        {
                            using (SqlCommand sql_command = new SqlCommand(upd_estatus, Conexion))
                            {
                                foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
                                {
                                    Conexion.Open();
                                    char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
                                    string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
                                    sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query

                                    int Actualizadas = sql_command.ExecuteNonQuery();
                                    filasActualizadas += Actualizadas; //total de cuentas-GP procesadas

                                    sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                    sql_command.Connection.Close();
                                    Conexion.Close();
                                }
                                //datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                            }
                        }
                    }

                    datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                }
                else //ocurre cuando la lista 'ListDigCuentaGP' esta vacia
                {
                    filasActualizadas = 0;
                    datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                    return datos;
                }
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "cambiaStatusMultiplesPolizasProcesada", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";//Falla en el Update
            }

            return datos;
        }

        //clsLayoutNomina
        //reviertePolizaProcesada - cambia revirtiendo por Poliza sus estatus, de PROCESADAS a PENDIENTES de todas las cuentas con y sin proyecto. Pero no cambia estatus de cuentas de Vales ni de Origen
        public clsResultado reviertePolizaProcesada(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
            {
                clsResultado datos = new clsResultado();
                datos.Resultado = "Indefinido";
                List<char> ListDigCuentaGP = new List<char>();
                int filasActualizadas = 0; //inicializa en cero la cantidad de registros que cambiaron de estatus a Pendientes

                string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

                try
                {
                    //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
                    using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
                    {
                        using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
                        {
                            Conexion.Open();
                            SqlDataReader dr = sql_command.ExecuteReader();

                            while (dr.Read())
                            {
                                char digCnts; //= new char();
                                digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString()); // en caso de que el catalogo este vacio se produce el error: "String must be exactly one character long"
                                ListDigCuentaGP.Add(digCnts); //nunca enviara una lista vacia. En caso de estarvacia antes se genera caera en el Catch, enviando un NULL al front
                            }
                            sql_command.Connection.Close();
                            Conexion.Close();
                        }
                    }

                    //SEGUNDO, con la lista de los prefijos(el primer numero) del 'segmento cuenta' de las cuentas-GP, se cambia el estatus de cada cuenta a PENDIENTE
                    if (ListDigCuentaGP.Count != 0) //Este IF esta sobrando ya que nunca recibira una lista sin registros, antes generaria un error
                    {
                        //char? pendiente = null;//el valor NULL cambia una cuenta a Layout-PENDIENTE
                        string upd_estatus = "UPDATE "
                                        + "     " + tblSrcPoliciesNOM + ""

                                        + " SET CtrlPPTO = NULL ," //se cambia el Estatus de cada cuenta a NULL(Pendiente)
                                        + "     ModifyDate = CAST(GETDATE() AS DATE)" //establece en ModifyDate la fecha en la que se ejecuta el update

                                        + " WHERE"

                                        //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
                                        + " ACTNUMBER like @ACTNUMBER"
                                        //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
                                        + " AND DEBITAMT >= 0.00" //el operador >= permite obtener cuentas de debito con montos = 0.00 para los casos de Cuentas Especiales de RH
                                        //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
                                        + " AND CRDTAMT = 0.00"
                                        //Filtra cuentas-GP con estatus de layout PROCESADA PROCESADA = 1
                                        + " AND CtrlPPTO = 1"
                                        //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
                                        + " AND (CreateDate BETWEEN"
                                        + "    CASE"
                                        + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
                                        + "        ELSE '" + fechaIni + "'"
                                        + "    END"
                                        + "    AND"
                                        + "    CASE"
                                        + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
                                        + "    ELSE '" + fechaFin + "'"
                                        + "    END)"
                                        //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
                                        + " AND BACHNUMB = '" + bachNumb + "'";
                        using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                        {
                            using (SqlCommand sql_command = new SqlCommand(upd_estatus, Conexion))
                            {
                                foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
                                {
                                    Conexion.Open();
                                    char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
                                    string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
                                    sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query

                                    int Actualizadas = sql_command.ExecuteNonQuery();
                                    filasActualizadas += Actualizadas; //total de cuentas-GP procesadas

                                    sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                    sql_command.Connection.Close();
                                    Conexion.Close();
                                }

                                datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                            }
                        }
                    }
                    else //ocurre cuando la lista 'filasActualizadas' esta vacia
                    {
                        filasActualizadas = 0;
                        datos.Resultado = JsonConvert.SerializeObject(filasActualizadas);
                        return datos;
                    }
                }
                catch (Exception e)
                {
                    NameValueCollection valores = new NameValueCollection();
                    valores.Add("Error: ", e.Message.ToString());
                    valores.Add("Source:", e.Source.ToString());
                    valores.Add("StackTrace:", e.StackTrace.ToString());
                    valores.Add("Date:", System.DateTime.Now.ToString());
                    AdministradorException.Publicar(e, valores, rutaLog, "reviertePolizaProcesada", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                    clsResultado SalidaF = new clsResultado();
                    datos.Resultado = "Error";//Falla en el Update
                }

                return datos;
            }

        //clsLayoutNomina
        //updateDebitAmt - Se actualiza el debito por cuenta. Para casos de Cuentas Especiales que RH calcula en Fortia, Reclasifica y Excluye de la provision de nomina
        public clsResultado updateDebitAmt(int jrnlidx, decimal debitAmt, string ConexionNOMINAS, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            int filaActualizada = 0;//inicializa en cero la cantidad de registros anulados
            try
            {
                string upd_estatus = "UPDATE "
                                + "     " + tblSrcPoliciesNOM + ""

                                + " SET DEBITAMT = '" + debitAmt + "'," //se anula cambiando el Estatus de la cuenta a 2(Anulada)
                                + "     ModifyDate = CAST(GETDATE() AS DATE)" //establece en ModifyDate la fecha en la que se ejecuta el update

                                + " WHERE"
                                + " JRNLIDX = '" + jrnlidx + "'";

                using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                {
                    using (SqlCommand sql_command = new SqlCommand(upd_estatus, Conexion))
                    {
                        Conexion.Open();
                        filaActualizada = sql_command.ExecuteNonQuery();//total de registros actualizados
                        sql_command.Connection.Close();
                        Conexion.Close();
                        datos.Resultado = JsonConvert.SerializeObject(filaActualizada);
                    }
                }
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "updateDebitAmt", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";//Falla en el Update
            }
            return datos;
        }


        /// <summary>
        /// Devuelve los grupos de Ledger Accounts para asignarse a sus correspondeintes Cuentas de Credito(Origen)
        /// </summary>
        //Los grupos de LeyerAccounts se obtienen de las cuentasWD marcadas como "cuentas de tipo Credito" en el catalogo de Cuentas de la UDLAPSIF
        public List<cuentaWDorigenCREDIT> GetListLedgAcctCredit(string rutaLog)
        {
            List<cuentaWDorigenCREDIT> listCuentasOrigen = null;
            try
            {
                var query = from c in dbSIF.PTONOM_CAT_CUENTAS
                            join cc in dbSIF.PTONOM_CAT_COSTCENTER on c.ID_CC equals cc.ID_CC
                            join la in dbSIF.PTONOM_CAT_LEDGERACCOUNT on c.ID_LA equals la.ID_LA
                            join sc in dbSIF.PTONOM_CAT_SPENDCATEGORY on c.ID_SC equals sc.ID_SC
                            where c.IDTIPOCUENTA == 2
                            select new cuentaWDorigenCREDIT
                            {
                                prefixLedgAcct = la.LA.Substring(0, 3),
                                CostCenter = cc.CC,
                                LedgerAccount = la.LA,
                                SpendCategory =sc.SC,
                                Credit = 0.0M,
                                BudgetCurrency = c.PTONOM_CAT_CURRENCY.CODE
                            };
                listCuentasOrigen = query.ToList();
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "GetLedgAcctCredit", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                listCuentasOrigen = null;//"Error";
            }
            return listCuentasOrigen;
        }
        
        /// <summary>
        /// Devuelve una lista de los parámetros del Catalogo de Parámetros
        /// </summary>
        List<listCatalogParameters> Get_AmendmentEntryData_Params(string rutaLog)
        {
            List<listCatalogParameters> ListaParams = null;
            List<int> idParamsAmendmentEntryData = new List<int> { 1 //Header key
                                                                  ,18 //Fiscal Time Interval
                                                                  ,24 //Account Set (A)
                                                                  ,33 //Memo
                                                                 };
            try
            {
                var query = (from p in dbSIF.PTONOM_CAT_PARAMETERS
                                   where idParamsAmendmentEntryData.Contains(p.IDPARAM)//p.IDPARAM == 1 || p.IDPARAM == 11 || p.IDPARAM == 12
                             select new listCatalogParameters
                                   {
                                       IdParam = p.IDPARAM,
                                       //DescribeParam = p.DESCRIBEPARAM,
                                       Valor = p.VALUEPARAM
                                   }).ToList();
                ListaParams = query;
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "Get_AmendmentEntryData_Params", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                ListaParams = null;//"Error";
            }
            return ListaParams;
        }
        
        List<listCatalogParameters> Get_ImportBudgetAmendment_Params(string rutaLog)
        {
            List<listCatalogParameters> ListaParams = null;
            List<int> idParamsImportBudgetAmendment = new List<int> { 1 //Header key
                                                                     ,2 //Add only
                                                                     ,4 //Auto complete
                                                                     ,8 //Submit
                                                                     ,9 //Company or Company Hierarchy
                                                                     ,10 //Budget Structure
                                                                     ,11 //Budget Name
                                                                     ,12 //Fiscal Year
                                                                     ,13 //Amendment Date
                                                                     ,15 //Budget Amendment Type
                                                                     }; 
            try
            {
                var query = (from p in dbSIF.PTONOM_CAT_PARAMETERS
                             where idParamsImportBudgetAmendment.Contains(p.IDPARAM) //p.IDPARAM == 1 || p.IDPARAM == 11 || p.IDPARAM == 12 //IDPARAM=1 -> Header key, IDPARAM=11 -> Fiscal time interval, IDPARAM=12 -> Account set
                             select new listCatalogParameters
                             {
                                 IdParam = p.IDPARAM,
                                 //DescribeParam = p.DESCRIBEPARAM,
                                 Valor = p.VALUEPARAM
                             }).ToList();

                ListaParams = query;
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "Get_AmendmentEntryData_Params", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                ListaParams = null;//"Error";
            }
            return ListaParams;
        }

        
        //public List<string> Get_ErrorBatchnumbs2(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        public List<string> Get_ErrorBatchnumbs(string fechaIni, string fechaFin, string bachNumb, char? statusLayout, bool? procesaConta, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            clsResultado datos = new clsResultado();
            datos.Resultado = "Indefinido";
            List<char> ListDigCuentaGP = new List<char>();
            List<listCuentasGP> ListPolizaCuentaGP = new List<listCuentasGP>();
            List<listErrorPolizas> ListErrorPolizas = new List<listErrorPolizas>();
            List<string> ListPolizasError = new List<string>();

            string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

            try
            {
                //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
                using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
                {
                    using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
                    {
                        Conexion.Open();
                        SqlDataReader dr = sql_command.ExecuteReader();

                        while (dr.Read())
                        {
                            char digCnts; //= new char();
                            digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString());
                            ListDigCuentaGP.Add(digCnts);
                        }
                        sql_command.Connection.Close();
                        Conexion.Close();
                    }
                }
                //SEGUNDO, con la lista de los prefijos(el primer numero) del 'segmento cuenta' de las cuentas-GP, genera la lista de "todas las CuentasGP" que perteneces a cada una de las Polizas de Nomina.
                //Todas las cuentasGP generadas incluye todas las 'cuentas' duplicadas, de vales, de origen, de destino y de proyecto.
                if (ListDigCuentaGP.Count != 0)
                {
                    string query = "SELECT "
                                    + "    BACHNUMB as IDPOLIZA,"
                                    + "    CONCAT(SUBSTRING(ACTNUMBER,1,2),'-',SUBSTRING(ACTNUMBER,3,2),'-',SUBSTRING(ACTNUMBER,5,6),'-',SUBSTRING(ACTNUMBER,11,3),'-',SUBSTRING(ACTNUMBER,14,8)) as CUENTAGP"

                                    + " FROM " + tblSrcPoliciesNOM + ""

                                    + " WHERE"

                                    //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
                                    + " ACTNUMBER like @ACTNUMBER"
                                    //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
                                    + " AND DEBITAMT >= 0.00" //el operador >= permite obtener cuentas de debito con montos = 0.00 para los casos de Cuentas Especiales de RH
                                    //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
                                    + " AND CRDTAMT = 0.00"

                                    //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
                                    + " AND (CreateDate BETWEEN"
                                    + "    CASE"
                                    + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
                                    + "        ELSE '" + fechaIni + "'"
                                    + "    END"
                                    + "    AND"
                                    + "    CASE"
                                    + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
                                    + "    ELSE '" + fechaFin + "'"
                                    + "    END)"

                                    //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
                                    + " AND BACHNUMB LIKE CASE"
                                    + "                     WHEN '" + bachNumb + "'='' THEN BACHNUMB"
                                    + "                     WHEN '" + bachNumb + "' IS NULL THEN BACHNUMB"
                                    + "                     ELSE '" + "%" + bachNumb + "%" + "'"
                                    + "                   END"

                                    //PARAMETRO4 "ESTATUS LAYOUT"
                                    + " AND (('" + statusLayout + "'='0')"
                                    + "            AND (CtrlPPTO IS NULL)"
                                    + "        OR"
                                    + "        ('" + statusLayout + "' = '1' OR '" + statusLayout + "' = '2' OR '" + statusLayout + "' = '9')"
                                    + "            AND CtrlPPTO = CASE"
                                    + "                                WHEN '" + statusLayout + "' = '1' THEN '1'"
                                    + "                                WHEN '" + statusLayout + "' = '2' THEN '2'"
                                    + "                                WHEN '" + statusLayout + "' = '9' THEN '9'"
                                    + "                            END"
                                    + "        OR"
                                    + "        ('" + statusLayout + "' IS NULL OR '" + statusLayout + "'='')"
                                    + "            AND(CtrlPPTO IS NULL"
                                    + "                OR CtrlPPTO = '1'"
                                    + "                OR CtrlPPTO = '2'"
                                    + "                OR CtrlPPTO = '9')"
                                    + "      )"

                                    //PARAMETRO5 "PROCESADO POR CONTA". 1=No contabilizada / 0=Si contabilizada
                                    + " AND Active = CASE"
                                    + "                  WHEN @ACTIVE IS NULL THEN Active"//el valor nulo "null" solo lo puede recibir por parametro el @ACTIVE y no de manera directa
                                    + "                  ELSE @ACTIVE"
                                    + "              END"

                                    //Ordenamiento de los registros obternidos
                                    + " Group by CONCAT(SUBSTRING(ACTNUMBER,1,2),'-',SUBSTRING(ACTNUMBER,3,2),'-',SUBSTRING(ACTNUMBER,5,6),'-',SUBSTRING(ACTNUMBER,11,3),'-',SUBSTRING(ACTNUMBER,14,8)), BACHNUMB"
                                    + " Order by BACHNUMB";

                    using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
                    {
                        using (SqlCommand sql_command = new SqlCommand(query, Conexion))
                        {
                            foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
                            {
                                Conexion.Open();
                                char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
                                string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
                                sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query
                                sql_command.Parameters.AddWithValue("@ACTIVE", procesaConta == null ? (object)DBNull.Value : procesaConta);
                                sql_command.CommandType = CommandType.Text;
                                SqlDataReader dr = sql_command.ExecuteReader();
                                while (dr.Read())
                                {
                                    listCuentasGP Cuenta = new listCuentasGP();
                                    Cuenta.idPoliza = dr["IDPOLIZA"].ToString();
                                    Cuenta.cuentaGP = dr["CUENTAGP"].ToString();
                                    ListPolizaCuentaGP.Add(Cuenta);//obtiene lista de todas las cuentasGP agrupadas por su poliza a la que petenece y en orden ascendente por poliza
                                }
                                sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                sql_command.Parameters.RemoveAt("@ACTIVE");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
                                sql_command.Connection.Close();
                                Conexion.Close();
                            }
                            datos.Resultado = JsonConvert.SerializeObject(ListPolizaCuentaGP);
                        }
                    }
                }
                else //ocurre cuando la lista 'ListDigCuentaGP' esta vacia
                {
                    ListDigCuentaGP.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(ListDigCuentaGP);
                    return null;
                }



                if (ListPolizaCuentaGP.Count != 0)
                {
                    var ListCuentaGPdistincts = ListPolizaCuentaGP
                                            .Select(x => x.cuentaGP)
                                            .Distinct()
                                            .ToList();

                    //var ListPolizaGPdistincts = ListPolizaCuentaGP
                    //    .Select(x => x.idPoliza)
                    //    .Distinct()
                    //    .ToList();

                    //var catCuentas = dbSIF.PTONOM_CAT_CUENTAS.Select(c => c.CUENTA);
                    //cuentaGPdistincts.RemoveAll(c => catCuentas.Contains(c.ToString()));
                    ListCuentaGPdistincts.RemoveAll(cntGP => dbSIF.PTONOM_CAT_CUENTAS.Any(catalogo => catalogo.CUENTA == cntGP));

                    

                    foreach (var cntGPError in ListCuentaGPdistincts)
                    {
                        //foreach (var poliza in ListPolizaGPdistincts)
                        //{
                            // Buscar la primera coincidencia en la segunda columna de ListaB
                            // var itemCoincidencia = ListPolizaCuentaGP.FirstOrDefault(lp => lp.cuentaGP == cntGP);
                            var listaPolizasCuentasError = ListPolizaCuentaGP.Where(lp => lp.cuentaGP == cntGPError).ToList();

                            if (listaPolizasCuentasError.Count != 0)
                            {
                                //var polizaError = itemCoincidencia.idPoliza;

                                // Agregar a ListaC
                                var listaTempPolizasError = listaPolizasCuentasError.Select(ce => ce.idPoliza).ToList();
                                ListPolizasError.AddRange(listaTempPolizasError);

                                // Eliminar todas las filas de ListaB donde la primera columna coincide
                                //ListPolizaCuentaGP.RemoveAll(b => b.idPoliza == polizaError); //&& b.idPoliza == poliza

                                //var rowsParaEliminar = ListPolizaCuentaGP.Where(lPC => listaTempPolizasError.Contains(lPC.idPoliza));
                                ListPolizaCuentaGP.RemoveAll(lPC => listaTempPolizasError.Contains(lPC.idPoliza));
                                listaTempPolizasError.Clear();
                            }
                        //}
                    }

                }
                else //ocurre cuando la lista 'ListErrorPolizas' esta vacia
                {
                    ListPolizaCuentaGP.Clear();
                    datos.Resultado = JsonConvert.SerializeObject(ListPolizaCuentaGP);
                    return null;
                }
                


              /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //if (ListPolizaCuentaGP.Count != 0)
                //{
                //    List<listErrorPolizas> ListErrorPolizas1 = new List<listErrorPolizas>();
                //    foreach (var lc in ListPolizaCuentaGP)
                //    {
                //        var query = dbSIF.PTONOM_CAT_CUENTAS.Where(cat_Cnta => cat_Cnta.CUENTA == lc.cuentaGP).Select(cat_Cnta => cat_Cnta.CUENTA).FirstOrDefault();
                //        ListErrorPolizas1.Add(new listErrorPolizas { idPoliza = lc.idPoliza, errorPoliza = false });
                //        //new listErrorPolizas { idPoliza = lc.idPoliza, errorPoliza = false };

                //    }

                //    foreach (var CC in dbSIF.PTONOM_CAT_CUENTAS)
                //    {
                //        //var query = ListCuentas.Where(x => x.cuentaGP == CC.CUENTA).Select(x => x.idPoliza).FirstOrDefault();
                //        var query = (from lc in ListPolizaCuentaGP
                //                    where lc.cuentaGP == CC.CUENTA && CC.ESTATUS == true
                //                     select new listErrorPolizas
                //                    {
                //                        idPoliza = lc.idPoliza,
                //                        errorPoliza = lc.idPoliza != null ? true : false
                //                    }).FirstOrDefault();
                //        ListErrorPolizas.Add(query);

                        //IQueryable<listErrorPolizas> query = (from lc in ListCuentas
                        //                                      where lc.cuentaGP == CC.CUENTA
                        //                                      select new listErrorPolizas
                        //                                      {
                        //                                          idPoliza = lc.idPoliza,
                        //                                          errorPoliza = lc.idPoliza != null ? true : false
                        //                                      }).AsQueryable();
                //   }
                //}


                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                
            }
            catch (Exception e)
            {
                NameValueCollection valores = new NameValueCollection();
                valores.Add("Error: ", e.Message.ToString());
                valores.Add("Source:", e.Source.ToString());
                valores.Add("StackTrace:", e.StackTrace.ToString());
                valores.Add("Date:", System.DateTime.Now.ToString());
                AdministradorException.Publicar(e, valores, rutaLog, "Get_ErrorBatchnumbs", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                clsResultado SalidaF = new clsResultado();
                datos.Resultado = "Error";
            }

            return ListPolizasError;
        }

        public clsResultado guardaNuevaPoliza(string bachNumb, string reference, string createDate, List<clsActnumberDebitamt> actnumberDebitamt, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            clsResultado guardo = new clsResultado();
            guardo.Resultado = "Indefinido";
            try
            {
                //recibe por separado el string de fechaCarrera y hraCarrera, las convierte en Date & Time y las concatena como DateTime fechaHraCarrera
                DateTime fCreateDate = DateTime.Now;
                if (createDate != null && createDate.Trim() != "")
                {
                    fCreateDate = DateTime.ParseExact(createDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                //var existePoliza = bachNumb.Where(b => dbSIF.xNOMExcelRH.Any(x => x.BACHNUMB == bachNumb && x.CreateDate == fCreateDate)).FirstOrDefault();
                bool existePoliza = dbSIF.xNOMExcelRH.Any(b => b.BACHNUMB.Equals(bachNumb, StringComparison.OrdinalIgnoreCase) &&
                                                               b.CreateDate == fCreateDate);//valida que no exista la misma poliza con la misma fecha-de-poliza en la tabla xNOMExcelRH
                                                               //b.CreateDate.Value.Date == fCreateDate.Date
                if (existePoliza == false)
                {
                    //foreach (var ad in actnumberDebitamt)
                    //{
                    //    xNOMExcelRH newPoliza = new xNOMExcelRH();
                    //    newPoliza.BACHNUMB = bachNumb;
                    //    newPoliza.ACTINDX = 0;
                    //    newPoliza.ACTNUMBER = ad.actnumber;
                    //    newPoliza.CRDTAMT = 0;
                    //    newPoliza.CURNCYID = "MN";
                    //    newPoliza.CURRNIDX = 0;
                    //    newPoliza.CURYCRDTAMT = 0;
                    //    newPoliza.CURYDEBITAMT = 0;
                    //    newPoliza.DEBITAMT = ad.debitamt;
                    //    newPoliza.DSCRIPTN = "01";
                    //    newPoliza.EXCHDATE = new DateTime(1900, 1, 1); // fecha 1900-01-01
                    //    newPoliza.REFRENCE = reference;
                    //    newPoliza.SOURCDOC = "NOM";
                    //    newPoliza.TRXDATE = DateTime.Now;
                    //    newPoliza.XCHGRATE = 0;
                    //    newPoliza.CreateDate = fCreateDate;
                    //    newPoliza.ModifyDate = new DateTime(1900, 1, 1); // fecha 1900-01-01
                    //    newPoliza.Active = true;//por default = TRUE
                    //    newPoliza.isDeleted = true;//por default = TRUE
                    //    newPoliza.LoginName = null;
                    //    newPoliza.ReadDate = new DateTime(1900, 1, 1); // fecha 1900-01-01
                    //    newPoliza.isRead = false;
                    //    //newPoliza.JRNLIDX = null; //Este valor es una Identity, por lo que su valor NO debe ser modificado ni insertado
                    //    newPoliza.CtrlPPTO = "0"; //por default el valor 0 indica que la cuenta esta PENDIENTE de ser integrada a un Layout
                    //    dbSIF.xNOMExcelRH.Add(newPoliza);
                    //    dbSIF.SaveChanges();
                    //}
                    //guardo.Resultado = actnumberDebitamt.Count().ToString();

                    foreach (var ad in actnumberDebitamt)
                    {
                        xNOMExcelRH newPoliza = new xNOMExcelRH
                        {
                            BACHNUMB = bachNumb,
                            ACTINDX = 0,
                            ACTNUMBER = ad.actnumber,
                            CRDTAMT = 0,
                            CURNCYID = "MN",
                            CURRNIDX = 0,
                            CURYCRDTAMT = 0,
                            CURYDEBITAMT = 0,
                            DEBITAMT = ad.debitamt,
                            DSCRIPTN = "01",
                            EXCHDATE = new DateTime(1900, 1, 1), // fecha 1900-01-01
                            REFRENCE = reference,
                            SOURCDOC = "NOM",
                            TRXDATE = DateTime.Now,
                            XCHGRATE = 0,
                            CreateDate = fCreateDate,
                            ModifyDate = new DateTime(1900, 1, 1), // fecha 1900-01-01
                            Active = true,//por default = TRUE
                            isDeleted = true,//por default = TRUE
                            LoginName = null,
                            ReadDate = new DateTime(1900, 1, 1), // fecha 1900-01-01
                            isRead = false,
                            //newPoliza.JRNLIDX = null, //Este valor es una Identity, por lo que su valor NO debe ser modificado ni insertado
                            CtrlPPTO = null, //por default el valor NULO indica que la cuenta esta PENDIENTE de ser integrada a un Layout
                        };
                        dbSIF.xNOMExcelRH.Add(newPoliza);                        
                    }
                    dbSIF.SaveChanges();
                    guardo.Resultado = actnumberDebitamt.Count().ToString();
                }
                else
                {
                    Exception msgError = new Exception("Intento agregar una póliza existente. No se permite guardar pólizas repetidas en la tabla dbo.xNOMExcelRH.");
                    AdministradorException.Publicar(msgError, rutaLog, "guardaNuevaPoliza", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                    guardo.Resultado = "Error: Intento agregar una póliza existente";
                }

            }
            catch (Exception e)
            {
                AdministradorException.Publicar(e, rutaLog, "guardaNuevaPoliza", Udla.Exceptions.TipoSalidaExcepcion.Txt);
                guardo.Resultado = "Error: Falló al intentar importar una póliza, por favor inténtelo nuevamente";
            }

            return guardo;
        }

        #region METODOS DEPRECADOS
        //public static List<string> ConvertAnonymousArrayToStringList(object[] array)
        //{
        //    List<string> stringList = new List<string>();

        //    foreach (var item in array)
        //    {
        //        stringList.Add(item.ToString());
        //    }

        //    return stringList;
        //}

        //clsLayoutNomina
        //obtenLayoutAEDprocesadas - Es igual al metodo obtenLayout(), pero la diferencia es que unicamente obtiene las cunetas que han sido Procesadas, posterior a la generacion de su layout. Con propocito de visualizar el cambio de estado
        public clsResultado obtenLayoutAEDprocesadas(string fechaIni, string fechaFin, string bachNumb, string paramDescription, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog) //string catParamsDescription,
        {
            //    int TotalCuentasDestino = 0;
            //    int TotalCuentasOrigen = 0;
            //    decimal TotalDebitAmount = 0.0M;
            //    decimal TotalCreditAmount = 0.0M;
            //    clsResultado datos = new clsResultado();
            //    datos.Resultado = "Indefinido";
            //    clsResultado LayoutImportBudgetAmendment = new clsResultado();
            //    LayoutImportBudgetAmendment.Resultado = "Indefinido";
            //    clsResultado LayoutAmendmentEntryData = new clsResultado();
            //    LayoutAmendmentEntryData.Resultado = "Indefinido";

            //    List<char> ListDigCuentaGP = new List<char>();
            //    //ListDigCuentaGP = null;
            //    List<clsResult_CuentaNomBuscar> ListCuentas = new List<clsResult_CuentaNomBuscar>();
            //    //ListCuentas = null;
            //    List<listParamsLayoutAmendmentEntryData> listaCuentasDestinoParaLayout = new List<listParamsLayoutAmendmentEntryData>();
            //    listaCuentasDestinoParaLayout = null;
            //    List<listParamsLayoutAmendmentEntryData> listaCuentasOrigenParaLayout = new List<listParamsLayoutAmendmentEntryData>();
            //    listaCuentasOrigenParaLayout = null;
            //    List<cuentaWDorigenCREDIT> listCreditosPreviaSinAgrupar = new List<cuentaWDorigenCREDIT>();
            //    listCreditosPreviaSinAgrupar = null;
            //    List<cuentaWDorigenCREDIT> listCreditosAgrupada = new List<cuentaWDorigenCREDIT>();
            //    listCreditosAgrupada = null;
            //    List<listParamsLayoutAmendmentEntryData> ListLayoutAED = new List<listParamsLayoutAmendmentEntryData>();
            //    ListLayoutAED = null;
            //    List<listParamsLayoutImportBudgetAmendment> ListLayoutIBA = new List<listParamsLayoutImportBudgetAmendment>();
            //    //ListLayoutIBA = null;
            //    clsResult_LayoutNominaObten listLayoutDeNomina = new clsResult_LayoutNominaObten();
            //    //listLayoutDeNomina = null;

            //    try
            //    {
            //        //Inicia preparando la lista del CATALOGO DE PARAMETROS PARA ARMAR EL LAYOUT AmendmentEntryData 
            //        var ListCatalogoParamsAmendmentEntryData = Get_AmendmentEntryData_Params(rutaLog);

            //        //PRIMERO, busca y genera una lista de todos los prefijos(el primer numero) del 'segmento cuenta' de todas las cuentas-GP del Catalogo PTONOM_CAT_CUENTAS
            //        string qryDigCuenta = "SELECT DISTINCT SUBSTRING(CUENTA, 14, 1) AS SegmentoDeCuenta FROM PTONOM_CAT_CUENTAS WHERE ESTATUS=1"; //Busca todaas las series de cuentas y que esten habilitadas. En catalogo de Cuentas el formato de una cuentas-GP va separado con guiones nn-nn-nnnnnn-nnn-nnnnnnnn

            //        using (SqlConnection Conexion = new SqlConnection(ConexionUDLAPSIF))
            //        {
            //            using (SqlCommand sql_command = new SqlCommand(qryDigCuenta, Conexion))
            //            {
            //                Conexion.Open();
            //                SqlDataReader dr = sql_command.ExecuteReader();

            //                while (dr.Read())
            //                {
            //                    char digCnts; //= new char();
            //                    digCnts = char.Parse(dr["SegmentoDeCuenta"].ToString());
            //                    ListDigCuentaGP.Add(digCnts);
            //                }
            //                sql_command.Connection.Close();
            //                Conexion.Close();
            //            }
            //        }

            //        //SEGUNDO, con la lista de prefijos del 'segmento cuenta' de cuentas-GP, se genera una lista de Cuentas-GP no duplicadas
            //        //De la BD Fortia genera una lista de cuentasGP con guiones intermedios candidatas a ser provisionadas. La lista no contiene cuentaWD
            //        //Cada registro contiene 3 columnas: Num | cuentaGP | debito
            //        //La lista generada solo es de cuentasGP PENDIENTES (Estatus = NULL)
            //        //La lista generada solo es de cuentasGP NO PROCESADAS CONTABILIDAD (Active = 1)
            //        //La lista generada va FILTRADA por rango de FECHAS INICIAL y FINAL
            //        //La lista generada contiene CuentasGP unicamente FILTRADAS por el BACHNUMB recibido por param
            //        if (ListDigCuentaGP.Count != 0)
            //        {
            //            int rowCount = 1;//inicia contador para enumerar cada fila de todo el SqlDataReader(ListPolizas) generado con el query
            //            string query = "SELECT "
            //                            + "    CONCAT(SUBSTRING(ACTNUMBER,1,2),'-',SUBSTRING(ACTNUMBER,3,2),'-',SUBSTRING(ACTNUMBER,5,6),'-',SUBSTRING(ACTNUMBER,11,3),'-',SUBSTRING(ACTNUMBER,14,8)) as cuentaGP,"
            //                            + "    DEBITAMT as debito"

            //                            + " FROM " + tblSrcPoliciesNOM + ""

            //                            + " WHERE"

            //                            //Filtra solo cuentas-GP donde el segmento de cuenta inicia con el digito 4, 5 u otro dependiendo de la cuenta capturada en el 'catalogo de cuentas'
            //                            + " ACTNUMBER like @ACTNUMBER"
            //                            //Filtra cuentas-GP donde su columna DEBITAMT sea mayor a 0.00
            //                            + " AND DEBITAMT > 0.00"
            //                            //Filtra cuentas-GP donde su columna CRDTAMT sea igual a 0.00
            //                            + " AND CRDTAMT = 0.00"
            //                            //Filtra cuentas-GP solo PROCESADAS (Estatus = 1)
            //                            + " AND CtrlPPTO = 1"
            //                            //Filtra cuentas-GP solo las que NO han sido procedadas o contabilizadas por Contabilidad(Active = 1)
            //                            + " AND Active = 1"

            //                            //PARAMETRO1 "FECHA-INICIO / FECHA-FIN"
            //                            + " AND (CreateDate BETWEEN"
            //                            + "    CASE"
            //                            + "        WHEN '" + fechaIni + "' IS NULL OR '" + fechaIni + "'='' THEN CreateDate"
            //                            + "        ELSE '" + fechaIni + "'"
            //                            + "    END"
            //                            + "    AND"
            //                            + "    CASE"
            //                            + "        WHEN '" + fechaFin + "' IS NULL OR '" + fechaFin + "'='' THEN CreateDate"
            //                            + "    ELSE '" + fechaFin + "'"
            //                            + "    END)"
            //                            //PARAMETRO2 "IDPOLIZA". Filtra por poliza de Nomina
            //                            + " AND BACHNUMB = '" + bachNumb + "'"
            //                            //Ordenamiento de los registros obternidos
            //                            + " Order by cuentaGP";

            //            using (SqlConnection Conexion = new SqlConnection(ConexionNOMINAS))
            //            {
            //                using (SqlCommand sql_command = new SqlCommand(query, Conexion))
            //                {
            //                    foreach (var d in ListDigCuentaGP)//ListDigCuentaGP
            //                    {
            //                        Conexion.Open();
            //                        //rowCount++; este contador solo enumera las veces que entra en el foreach, pero no las veces que entra en el while del Reader
            //                        char digitoPrefijoCuentaGP = d;//primer item "prefigo [digito de la cuentaGP]" recuperado de cada registro de la Lista ListBachNumbs
            //                        string actNumber = "[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]['" + digitoPrefijoCuentaGP + "']%";
            //                        sql_command.Parameters.AddWithValue("@ACTNUMBER", actNumber);//parametro ACTNUMBER enviado al where del query
            //                        sql_command.CommandType = CommandType.Text;
            //                        SqlDataReader dr = sql_command.ExecuteReader();
            //                        while (dr.Read())
            //                        {
            //                            clsResult_CuentaNomBuscar Cuenta = new clsResult_CuentaNomBuscar();
            //                            Cuenta.Num = (rowCount++).ToString();//inserta un contador por cada nueva fila generada
            //                            Cuenta.CuentaGP = dr["cuentaGP"].ToString();
            //                            Cuenta.Debito = Decimal.Parse(dr["debito"].ToString());
            //                            ListCuentas.Add(Cuenta);
            //                        }
            //                        sql_command.Parameters.RemoveAt("@ACTNUMBER");//elimina variable usada en el query, por cada loop finalizado, para volverla a usar en el siguiente loop
            //                        sql_command.Connection.Close();
            //                        Conexion.Close();
            //                    }
            //                    //la siguiente linea no se requiere. Solo es para TEST
            //                    datos.Resultado = JsonConvert.SerializeObject(ListCuentas);//Lista resultante con las cuenta-GP y su correspondiente Cuenta-WD
            //                }
            //            }
            //        }
            //        else //ocurre cuando la lista 'ListDigCuentaGP' esta vacia
            //        {
            //            ListCuentas.Clear();
            //            datos.Resultado = JsonConvert.SerializeObject(ListCuentas);
            //            return datos;
            //        }

            //        /****Inicia el proceso para generar las cuentas de debito o cuentas destino, es decir las cuentas de la nomina que seran provisionadas****/

            //        //TERCERO, se obtiene la lista depurada de todas las cuentasWD que seran provisionadas 
            //        //Cada registro se coloca en la columna correspondiente al layout AmendmentEntryData
            //        //1 Se cruza la lista 'ListCuentas' de cuentasGP, con el 'Catalogo de Cuentas' para obtener su correspondiente cuenta de debito o destino
            //        //2 Se depura la lista resultante, eliminando cuentasWD duplicadas para sumarizar sus montos en una sola cuenta por cada cuenta duplicada
            //        //3 Se quitan todas las cuentas de Vales de despensa
            //        //4 Se quitan todas las cuentas de credito u origen
            //        //5 Se identifica y separa en la misma lista las cuentasWD con posible proyecto asignado
            //        if ((ListCuentas != null || ListCuentas.Any()) && (ListCatalogoParamsAmendmentEntryData != null || ListCatalogoParamsAmendmentEntryData.Any()))
            //        {
            //            var ListDebitosPrevia = from f in ListCuentas
            //                                    join c in dbSIF.PTONOM_CAT_CUENTAS on f.CuentaGP equals c.CUENTA//usando como referencia las CuentaGP se relaciona una cuentaGP con su cuentaWD
            //                                    join cc in dbSIF.PTONOM_CAT_COSTCENTER on c.ID_CC equals cc.ID_CC
            //                                    join la in dbSIF.PTONOM_CAT_LEDGERACCOUNT on c.ID_LA equals la.ID_LA
            //                                    join sc in dbSIF.PTONOM_CAT_SPENDCATEGORY on c.ID_SC equals sc.ID_SC
            //                                    //join pr in dbSIF.PTONOM_CAT_PROJECT on c.IDPROJECT equals pr.IDPROJECT
            //                                    where c.VALES == false //discrimina todas las cuentas que son de tipo Vales de Despensa, donde VALES=1(si es cuenta de vales) VALES=0(no es cuenta de vales)
            //                                          && c.IDTIPOCUENTA == 1 //discrimina todas las cuentasWD de tipo CREDITO(origen), para dejar una lista unica de cuentas de debito(destino)
            //                                          && c.ESTATUS == true //cuando la cuentaWD relacionada esta Activa en catalogo de Cuentas
            //                                          && cc.ESTATUS == true //cuando el CostCenter relacionado esta Activo en catalogo CostCenter
            //                                          && la.ESTATUS == true //cuando el LedgerAccount relacionado esta Activo en catalogo LedgerAccount
            //                                          && sc.ESTATUS == true //cuando el SpendCategory relacionado esta Activo en catalogo SpendCategory
            //                                    group new { cc, la, sc, c, f } by new //pr, 
            //                                    {
            //                                        cc.CC,
            //                                        la.LA,
            //                                        sc.SC,
            //                                        //c,
            //                                        c.IDPROJECT
            //                                    } into grupo
            //                                    orderby grupo.Key.CC
            //                                    select new cuentaWDdestinoSinValesYsinCredits
            //                                    {
            //                                        CostCenter = grupo.Key.CC,
            //                                        LedgerAccount = grupo.Key.LA,
            //                                        SpendCategory = grupo.Key.SC,
            //                                        Debito = grupo.Sum(d => d.f.Debito),
            //                                        BudgetCurrency = grupo.Where(catCu => catCu.c.PTONOM_CAT_CURRENCY.IDCURRENCY == catCu.c.IDCURRENCY)
            //                                                              .Select(catCy => catCy.c.PTONOM_CAT_CURRENCY.CODE).FirstOrDefault(),//obtiene el code de la moneda, es decir el nombre abreviado de la moneda
            //                                                                                                                                  //BudgetCurrency = grupo.Select(cy => cy.c.IDCURRENCY).FirstOrDefault().ToString(),
            //                                        idProject = grupo.Select(pr => pr.c.IDPROJECT).DefaultIfEmpty(null).FirstOrDefault(),
            //                                    };
            //            //se itera la lista ListDebitosPrevia para poder asignar cada parte de una cuentaWD a sus correspondientes campos del layout AmendmentEntryData
            //            //tambien se cruza y agrupa por idProject para poder mostrar en la misma lista las cuentas 'sin proyecto' y con las posibles cuentas 'con proyecto'
            //            var query = from f in ListDebitosPrevia
            //                        join pr in dbSIF.PTONOM_CAT_PROJECT on f.idProject equals pr.IDPROJECT
            //                        into grupoProjects
            //                        from pr in grupoProjects.DefaultIfEmpty() //este es un Left-Join para obtener todos los registros donde PTONOM_CAT_PROJECT.idProject IS NULL
            //                        orderby f.CostCenter
            //                        select new listParamsLayoutAmendmentEntryData //cuentaWDdestinoSinValesYsinCredits
            //                        {
            //                            HeaderKey = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 1).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=1 -> Header key
            //                            LineKey = null,// SIN VALOR DESDE EL CATALOGO (Este valor siempre inicia en 1 y aumenta de uno en uno)
            //                            FiscalTimeInterval = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 18).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=18 -> Fiscal time interval
            //                            CostCenter = f.CostCenter,
            //                            SpendCategory = f.SpendCategory,
            //                            RevenueCategory = null,// SIN VALOR
            //                            Project = f.idProject != null ? pr.PROJECT : null,//Cuentas de Debito(destino) sí puede tener Proyectos
            //                            LedgerAccount = f.LedgerAccount,
            //                            AccountSet_a = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 24).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=24 -> Account set
            //                            LedgerAccountSummary = null,// SIN VALOR
            //                            AccountSet_b = null, // SIN VALOR (segunda Columna con titulo AccountSet)
            //                            BudgetCurrency = f.BudgetCurrency,
            //                            BookCode = null, // SIN VALOR
            //                            BudgetDebitAmount = f.Debito,
            //                            BudgetCreditAmount = 0.00M,
            //                            QuantityChange = null,// SIN VALOR
            //                            Unit = null,// SIN VALOR
            //                            Memo = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 33).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=33 -> Memo (este valor se captura desde pantalla antes de enviar el Layout al excel y a la tabla de guardado de Layouts)
            //                        };


            //            listaCuentasDestinoParaLayout = query.ToList();
            //            datos.Resultado = JsonConvert.SerializeObject(listaCuentasDestinoParaLayout);
            //        }
            //        else //ocurre cuando la lista 'listaCuentasDestinoParaLayout' esta vacia debido a que no se encontro ningunna cuenta
            //        {
            //            listaCuentasDestinoParaLayout.Clear();
            //            datos.Resultado = JsonConvert.SerializeObject(listaCuentasDestinoParaLayout);
            //            return datos;
            //        }

            //        /**** Inicia el proceso para generar las cuentas de credito o cuentas de origen de pago de la nomina ****/
            //        //El siguiente metodo obtienen unicamente las cuentas de tipo Origen o Credito sin montos. Hasta el momento solo son 3 cuentas origen
            //        //Con estas 3 cuentas se agrupara toda la lista de cuentas destino en 3 grupos es decir 3 cuentas origen

            //        var ListCuentasOrigen = GetListLedgAcctCredit(rutaLog);

            //        //CUARTO, genera lista de cuentas de debito y credito. Se agrega un campo con el prefijo del Ledger Account al que pertenece cada cuenta. 
            //        //La lista resultante tiene tanto cuentas Destino como Origen, pero pasa a ser llamada 'Lista de cuentas de Credito (Origen)'
            //        //1 Se reutiliza la Lista 'ListCuentas' generada en el segundo paso, para generar una lista de cuentas de DEBITO y cuentas de CREDITO
            //        //2 Se cruza la lista 'ListCuentas' de cuentasGP, con el 'Catalogo de Cuentas' para obtener su correspondiente cuenta de debito y credito
            //        //3 Se depura la lista resultante, eliminando cuentasWD duplicadas para sumarizar sus montos en una sola cuenta por cada cuenta duplicada
            //        //4 Se quitan todas las cuentas de Vales de despensa pero NO SE QUITAN las cuentas de CREDITO u origen
            //        //5 Se toma el mismo Ledger Account de cada registro, sele hace substring, y el prefijo resultante de 3 digitor se coloca para cada cuenta de la lista
            //        //Cada registro resultante contiene 6 columnas: Prefijo-LedgerAccount | CostCenter| LedgerAccount | SpendCategory | Credit | BudgetCurrency
            //        if (ListCuentas != null || ListCuentas.Any())
            //        {
            //            var query = from f in ListCuentas
            //                        join c in dbSIF.PTONOM_CAT_CUENTAS on f.CuentaGP equals c.CUENTA
            //                        join cc in dbSIF.PTONOM_CAT_COSTCENTER on c.ID_CC equals cc.ID_CC
            //                        join la in dbSIF.PTONOM_CAT_LEDGERACCOUNT on c.ID_LA equals la.ID_LA
            //                        join sc in dbSIF.PTONOM_CAT_SPENDCATEGORY on c.ID_SC equals sc.ID_SC
            //                        where c.VALES == false //discrimina todas las cuentas que son de tipo Vales de Despensa, donde VALES=1(si es cuenta de vales) VALES=0(no es cuenta de vales)
            //                                               //&& c.IDTIPOCUENTA == 1 en este paso no se requiere discrimina las cuentas de tipo CREDITO(origen)
            //                              && c.ESTATUS == true //cuando la cuentaWD relacionada esta Activa en catalogo de Cuentas
            //                              && cc.ESTATUS == true //cuando el CostCenter relacionado esta Activo en catalogo CostCenter
            //                              && la.ESTATUS == true //cuando el LedgerAccount relacionado esta Activo en catalogo LedgerAccount
            //                              && sc.ESTATUS == true //cuando el SpendCategory relacionado esta Activo en catalogo SpendCategory
            //                        group new { cc, la, sc, c, f } by new // c, pr, 
            //                        {
            //                            cc.CC,
            //                            la.LA,
            //                            sc.SC,
            //                            c.IDPROJECT
            //                        } into grupo
            //                        orderby grupo.Key.CC
            //                        select new cuentaWDorigenCREDIT //clsResult_cuentasLayoutObten
            //                        {
            //                            prefixLedgAcct = grupo.Key.LA.Substring(0, 3),
            //                            CostCenter = grupo.Key.CC,
            //                            LedgerAccount = grupo.Key.LA,
            //                            SpendCategory = grupo.Key.SC,
            //                            Credit = grupo.Sum(credit => credit.f.Debito),//la suma de todos los DEBITOS pasan a ser nombrados de CREDITOS asignandose a la propied 'Credit'
            //                            BudgetCurrency = grupo.Where(catCu => catCu.c.PTONOM_CAT_CURRENCY.IDCURRENCY == catCu.c.IDCURRENCY).Select(catCy => catCy.c.PTONOM_CAT_CURRENCY.CODE).FirstOrDefault(),
            //                        };
            //            listCreditosPreviaSinAgrupar = query.ToList();
            //            datos.Resultado = JsonConvert.SerializeObject(listCreditosPreviaSinAgrupar);//esta linea no se requiere. Solo es para TEST
            //        }
            //        else //ocurre cuando la lista 'ListCuentas' esta vacia debido a que el bachnumb no tiene cuentas o sus cuentas no cumplen con los filtros
            //        {
            //            listCreditosPreviaSinAgrupar.Clear();
            //            datos.Resultado = JsonConvert.SerializeObject(listCreditosPreviaSinAgrupar);
            //            return datos;
            //        }

            //        //QUINTO toda la 'listCreditosPreviaSinAgrupar' se agrupa por el prefijo de LedgerAccount de cada Cuenta origen 
            //        //previamente cada cuenta Origen fue capturada e identificada en el catalogo de Cuentas. Actualmente hay 3 cuentas de tipo Credito (Origen)
            //        //1 Cruzar las cuentas de la lista 'listCreditosPreviaSinAgrupar'(que contiene todas las cuentas, es decir cnts debitos + cuentas de credito) vs. la 'ListCuentasOrigen'(actualmente son 3 cnts)
            //        //2 filtra a travez del prefijo prefixLedgAcct que tienen en comun las 2 listas cruzadas
            //        //3 se unifican todas las cuentas de la lista listCreditosPreviaSinAgrupar a travez del perfil de sus Ledger Account prefixLedgAcct y se sumarizan sus montos y todos pasan a ser de 'Credito'
            //        //4 se asigna cada una de las 3 cuentaWD de Credito u Origen (seccionada por sus 3 componentes) a su correspondiente grupo de los 3 perfiles de LedgerAccount
            //        if ((ListCuentasOrigen != null || ListCuentasOrigen.Any()) && (listCreditosPreviaSinAgrupar != null || listCreditosPreviaSinAgrupar.Any()))
            //        {
            //            var query = from lco in ListCuentasOrigen
            //                        join lcp in listCreditosPreviaSinAgrupar on lco.prefixLedgAcct equals lcp.prefixLedgAcct
            //                        where lco.prefixLedgAcct == lcp.prefixLedgAcct
            //                        group new { lco, lcp } by new
            //                        {
            //                            lco.prefixLedgAcct
            //                        } into grupo
            //                        select new cuentaWDorigenCREDIT
            //                        {
            //                            prefixLedgAcct = grupo.Key.prefixLedgAcct,
            //                            CostCenter = grupo.Where(co => co.lco.prefixLedgAcct == grupo.Key.prefixLedgAcct).Select(x => x.lco.CostCenter).FirstOrDefault(),
            //                            LedgerAccount = grupo.Where(co => co.lco.prefixLedgAcct == grupo.Key.prefixLedgAcct).Select(x => x.lco.LedgerAccount).FirstOrDefault(),
            //                            SpendCategory = grupo.Where(co => co.lco.prefixLedgAcct == grupo.Key.prefixLedgAcct).Select(x => x.lco.SpendCategory).FirstOrDefault(),
            //                            Credit = grupo.Sum(c => c.lcp.Credit),
            //                            BudgetCurrency = grupo.Select(cy => cy.lcp.BudgetCurrency).FirstOrDefault()//no se requiere ToString()
            //                        };

            //            listCreditosAgrupada = query.ToList();
            //            TotalCuentasOrigen = listCreditosAgrupada.Count;//se calcula la cantidad de Cuentas de Credito(Origen) que el Layout de la Nomina tendra 
            //            TotalCreditAmount = listCreditosAgrupada.Sum(x => x.Credit);//se calcula el total del monto de Credito(Origen) con el que se provisionaran todas las cuentas destino
            //            datos.Resultado = JsonConvert.SerializeObject(listCreditosAgrupada);
            //        }
            //        else
            //        {
            //            listCreditosAgrupada.Clear();
            //            datos.Resultado = JsonConvert.SerializeObject(listCreditosAgrupada);
            //            return datos;
            //        }


            //        //SEXTO, se agrega las 3 Cuentas de Credito(Origen) a la estructura del layout AmendmentEntryData
            //        //se itera la lista agrupada 'listCreditosAgrupada' para poder asignar cada parte de una cuentaWD a sus correspondientes campos del layout AmendmentEntryData
            //        //tambien se asignan sus parametros a sus correspondientes campos haciendo subqueries hacia el catalogo de Parametros 
            //        if ((listCreditosAgrupada != null || listCreditosAgrupada.Any()) && (ListCatalogoParamsAmendmentEntryData != null || ListCatalogoParamsAmendmentEntryData.Any()))
            //        {
            //            var query = from lc in listCreditosAgrupada
            //                        select new listParamsLayoutAmendmentEntryData
            //                        {
            //                            HeaderKey = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 1).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=1 -> Header key
            //                            LineKey = null,// SIN VALOR DESDE EL CATALOGO (Este valor siempre inicia en 1 y aumenta de uno en uno)
            //                            FiscalTimeInterval = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 18).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=18 -> Fiscal time interval
            //                            CostCenter = lc.CostCenter,
            //                            SpendCategory = lc.SpendCategory,
            //                            RevenueCategory = null,// SIN VALOR
            //                            Project = null,//para las cuentas de Credito (origen) no hay Proyectos
            //                            LedgerAccount = lc.LedgerAccount,
            //                            AccountSet_a = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 24).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=24 -> Account set
            //                            LedgerAccountSummary = null,// SIN VALOR
            //                            AccountSet_b = null, // SIN VALOR (segunda Columna con titulo AccountSet)
            //                            BudgetCurrency = lc.BudgetCurrency,
            //                            BookCode = null, // SIN VALOR
            //                            BudgetDebitAmount = 0.00M,
            //                            BudgetCreditAmount = lc.Credit,
            //                            QuantityChange = null,// SIN VALOR
            //                            Unit = null,// SIN VALOR
            //                            Memo = ListCatalogoParamsAmendmentEntryData.Where(x => x.IdParam == 33).Select(x => x.Valor).FirstOrDefault(),//IDPARAM=33 -> Memo (este valor se captura desde pantalla antes de enviar el Layout al excel y a la tabla de guardado de Layouts)
            //                        };
            //            listaCuentasOrigenParaLayout = query.ToList();
            //            datos.Resultado = JsonConvert.SerializeObject(listaCuentasOrigenParaLayout);
            //        }
            //        else
            //        {
            //            listaCuentasOrigenParaLayout.Clear();
            //            datos.Resultado = JsonConvert.SerializeObject(listaCuentasOrigenParaLayout);
            //            return datos;
            //        }

            //        //SEPTIMO, se integran las dos secciones, las cuentas Debito(destino) y las cuentas Credito(origen), al layout AmendmentEntryData
            //        //1 Se concatenan las dos listas, dejando primero la lista 'listaCuentasDestinoParaLayout'(cuentas destino) y al final la listaCuentasOrigenParaLayout(cuentas origen)
            //        //2 Se genera el numero consecutivo del campo LineKey y se agregan todos los campos correspondientes
            //        if ((listaCuentasDestinoParaLayout != null || listaCuentasDestinoParaLayout.Any()) && (listaCuentasOrigenParaLayout != null || listaCuentasOrigenParaLayout.Any()))
            //        {
            //            var query = listaCuentasDestinoParaLayout
            //                        .Concat(listaCuentasOrigenParaLayout)
            //                        .Select((elemento, indice) => new listParamsLayoutAmendmentEntryData
            //                        {
            //                            Bachnumb = bachNumb,//esta es la poliza que viene en el parameto del metodo obtenLayout()
            //                            HeaderKey = elemento.HeaderKey,
            //                            LineKey = indice + 1,
            //                            FiscalTimeInterval = elemento.FiscalTimeInterval,
            //                            CostCenter = elemento.CostCenter,
            //                            SpendCategory = elemento.SpendCategory,
            //                            RevenueCategory = elemento.RevenueCategory,
            //                            Project = elemento.Project,
            //                            LedgerAccount = elemento.LedgerAccount,
            //                            AccountSet_a = elemento.AccountSet_a,
            //                            LedgerAccountSummary = elemento.LedgerAccountSummary,
            //                            AccountSet_b = elemento.AccountSet_b,
            //                            BudgetCurrency = elemento.BudgetCurrency,
            //                            BookCode = elemento.BookCode,
            //                            BudgetDebitAmount = elemento.BudgetDebitAmount,
            //                            BudgetCreditAmount = elemento.BudgetCreditAmount,
            //                            QuantityChange = elemento.Unit,
            //                            Unit = elemento.Unit,
            //                            Memo = elemento.Memo
            //                        });
            //            ListLayoutAED = query.ToList();
            //            TotalCuentasDestino = ListLayoutAED.Count - TotalCuentasOrigen;//se calcula la cantidad de Cuentas de Debito(Destino) que el Layout de la Nomina tendra
            //            TotalDebitAmount = ListLayoutAED.Sum(x => x.BudgetDebitAmount);//se calcula el total del monto de Debito(Destino) a ser pagado
            //            LayoutAmendmentEntryData.Resultado = JsonConvert.SerializeObject(ListLayoutAED);
            //        }
            //        else
            //        {
            //            listaCuentasOrigenParaLayout.Clear();
            //            datos.Resultado = JsonConvert.SerializeObject(listaCuentasOrigenParaLayout);
            //            return datos;
            //        }


            //        //Inicia preparando la lista del CATALOGO DE PARAMETROS PARA ARMAR EL LAYOUT ImportBudgetAmendment
            //        var ListCatalogoParamsImportBudgetAmendment = Get_ImportBudgetAmendment_Params(rutaLog);

            //        //OCTAVO, se genera el Layout ImportBudgetAmendment
            //        if ((ListCatalogoParamsImportBudgetAmendment != null || ListCatalogoParamsImportBudgetAmendment.Any())) //dbSIF.PTONOM_CAT_PARAMETERS != null || dbSIF.PTONOM_CAT_PARAMETERS.Any()
            //        {
            //            listParamsLayoutImportBudgetAmendment ListaLayoutIBA = new listParamsLayoutImportBudgetAmendment();

            //            ListaLayoutIBA.Bachnumb = bachNumb;//case 0: esta es la poliza que viene en el parameto del metodo obtenLayout()
            //            ListaLayoutIBA.Description = paramDescription;//case 14: el param DESCRIPTION es el valor de la REFRENCE de toda la poliza
            //            foreach (var item in ListCatalogoParamsImportBudgetAmendment)
            //            {
            //                switch (item.IdParam)
            //                {
            //                    //case 0: ListaLayoutIBA.Bachnumb = bachNumb; break;//esta es la poliza que viene en el parameto del presente metodo obtenLayout()
            //                    case 1: ListaLayoutIBA.HeaderKey = item.Valor; break;
            //                    case 2: ListaLayoutIBA.AddOnly = item.Valor; break;
            //                    case 3: ListaLayoutIBA.BudgetAmendment = null; break;
            //                    case 4: ListaLayoutIBA.AutoComplete = item.Valor; break;
            //                    case 5: ListaLayoutIBA.Comment = null; break;
            //                    case 6: ListaLayoutIBA.Worker = null; break;
            //                    case 7: ListaLayoutIBA.Id = null; break;
            //                    case 8: ListaLayoutIBA.Submit = item.Valor; break;
            //                    case 9: ListaLayoutIBA.CompanyOrCompanyHierarchy = item.Valor; break;
            //                    case 10: ListaLayoutIBA.BudgetStructure = item.Valor; break;
            //                    case 11: ListaLayoutIBA.BudgetName = item.Valor; break;
            //                    case 12: ListaLayoutIBA.FiscalYear = item.Valor; break;
            //                    case 13: ListaLayoutIBA.AmendmentDate = item.Valor; break;
            //                    //case 14: ListaLayoutIBA.Description = paramDescription; break;//el param DESCRIPTION es el valor de la REFRENCE de toda la poliza
            //                    case 15: ListaLayoutIBA.BudgetAmendmentType = item.Valor; break;
            //                    case 16: ListaLayoutIBA.BalancedAmendment = null; break;
            //                }
            //            }
            //            //List<paramsImportBudgetAmendment> ListLayoutIBA = new List<paramsImportBudgetAmendment> { ListaLayoutIBA };
            //            ListLayoutIBA.Add(ListaLayoutIBA);
            //            LayoutImportBudgetAmendment.Resultado = JsonConvert.SerializeObject(ListLayoutIBA);
            //        }
            //        else
            //        {
            //            listaCuentasOrigenParaLayout.Clear();
            //            datos.Resultado = JsonConvert.SerializeObject(listaCuentasOrigenParaLayout);
            //            return datos;
            //        }


            //        //
            //        if ((ListLayoutIBA != null || ListLayoutIBA.Any()) && (ListLayoutAED != null || ListLayoutAED.Any()))
            //        {
            //            listLayoutDeNomina.listLayoutIBA = ListLayoutIBA;
            //            listLayoutDeNomina.listLayoutAED = ListLayoutAED;
            //            listLayoutDeNomina.totalCuentasDestino = TotalCuentasDestino;
            //            listLayoutDeNomina.totalCuentasOrigen = TotalCuentasOrigen;
            //            listLayoutDeNomina.totalDebitAmount = TotalDebitAmount;
            //            listLayoutDeNomina.totalCreditAmount = TotalCreditAmount;
            //            datos.Resultado = JsonConvert.SerializeObject(listLayoutDeNomina);
            //        }
            //        else
            //        {
            //            listLayoutDeNomina = null;
            //            datos.Resultado = JsonConvert.SerializeObject(listLayoutDeNomina);
            //            return datos;
            //        }



            //    }
            //    catch (Exception e)
            //    {
            //        NameValueCollection valores = new NameValueCollection();
            //        valores.Add("Error: ", e.Message.ToString());
            //        valores.Add("Source:", e.Source.ToString());
            //        valores.Add("StackTrace:", e.StackTrace.ToString());
            //        valores.Add("Date:", System.DateTime.Now.ToString());
            //        AdministradorException.Publicar(e, valores, rutaLog, "obtenLayout", Udla.Exceptions.TipoSalidaExcepcion.Txt);
            //        clsResultado SalidaF = new clsResultado();
            //        datos.Resultado = "Error";
            //    }


            return null;//datos;
        }
        
        #endregion



    }

}
