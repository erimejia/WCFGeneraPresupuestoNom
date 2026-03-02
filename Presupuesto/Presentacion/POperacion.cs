using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Presupuesto.Clases;
using Presupuesto.Logica;


namespace Presupuesto.Presentacion
{
    public class POperacion
    {
        LOperacion o = new LOperacion();

        //clsConsultaPolizas
        //buscaPoliza
        public clsResultado buscaPolizaNom(string fechaIni, string fechaFin, string bachNumb, string refrence, char? statusLayout, bool? procesaConta, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.buscaPolizaNom(fechaIni, fechaFin, bachNumb,refrence, statusLayout, procesaConta, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }

        //AnulaPoliza
        public clsResultado anulaPolizaNom(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.anulaPolizaNom(fechaIni, fechaFin, bachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }


        //clsConsultaCuentas
        public clsResultado buscaCuentaNom(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.buscaCuentaNom(fechaIni, fechaFin, bachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }

        //AnulaCuenta
        public clsResultado anulaCuentaNom(int jrnlidx, string ConexionNOMINAS, string rutaLog)
        {
            return o.anulaCuentaNom(jrnlidx, ConexionNOMINAS, rutaLog);
        }

        //clsLayoutNomina
        //obtenLayout
        public clsResultado obtenLayoutMultiplesNominas(string fechaIni, string fechaFin, List<clsParamBachnumbRefrece> ParamsBachnumbRefrence, string estatusLayout, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.obtenLayoutMultiplesNominas(fechaIni, fechaFin, ParamsBachnumbRefrence, estatusLayout, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }

        //clsLayoutNomina
        //obtenLayout
        public clsResultado obtenLayout(string fechaIni, string fechaFin, string bachNumb, string paramDescription, string estatusLayout, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.obtenLayout(fechaIni, fechaFin, bachNumb, paramDescription, estatusLayout, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }

        //clsLayoutNomina
        //cambiaStatusPolizaProcesada 
        public clsResultado cambiaStatusPolizaProcesada(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.cambiaStatusPolizaProcesada(fechaIni, fechaFin, bachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }

        //clsLayoutNomina
        //cambiaStatusPolizaProcesada 
        public clsResultado cambiaStatusMultiplesPolizasProcesada(string procesadaMPFechaIni, string procesadaMPFechaFin, List<String> listProcesadaMPBachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.cambiaStatusMultiplesPolizasProcesada(procesadaMPFechaIni, procesadaMPFechaFin, listProcesadaMPBachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }

        //clsLayoutNomina
        //cambiaStatusPolizaProcesada 
        public clsResultado reviertePolizaProcesada(string fechaIni, string fechaFin, string bachNumb, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.reviertePolizaProcesada(fechaIni, fechaFin, bachNumb, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }

        //clsLayoutNomina
        //updateDebitAmt - Se actualiza el debito por cuenta. Para casos de Cuentas Especiales que RH calcula en Fortia, Reclasifica y Excluye de la provision de nomina
        public clsResultado updateDebitAmt(int jrnlidx, decimal debitAmt, string ConexionNOMINAS, string rutaLog)
        {
            return o.updateDebitAmt(jrnlidx, debitAmt, ConexionNOMINAS, rutaLog);
        }

        public clsResultado guardaNuevaPoliza(string bachNumb, string reference, string createDate, List<clsActnumberDebitamt> actnumberDebitamt, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        {
            return o.guardaNuevaPoliza(bachNumb, reference, createDate, actnumberDebitamt, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        }

        #region METODOS DEPRECADOS
        //clsLayoutNomina
        //obtenLayoutAEDprocesadas

        //public clsResultado obtenLayoutAEDprocesadas(string fechaIni, string fechaFin, string bachNumb, string paramDescription, string ConexionNOMINAS, string ConexionUDLAPSIF, string rutaLog)
        //{
        //    return o.obtenLayoutAEDprocesadas(fechaIni, fechaFin, bachNumb, paramDescription, ConexionNOMINAS, ConexionUDLAPSIF, rutaLog);
        //}

        #endregion
    }
}
