using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class listParamsLayoutAmendmentEntryData
    {
        public string Bachnumb { get; set; }
        public string HeaderKey { get; set; } // IDPARAM=1 Este valor siempre es 1 numerico
        public int? LineKey { get; set; } // SIN VALOR DESDE EL CATALOGO (Este valor siempre inicia en 1 y aumenta de uno en uno, por lo que se genera desde el sistema)
        public string FiscalTimeInterval { get; set; }//IDPARAM=11  La captura de este valor debera corresponder al semestre en el cual se provisionara la Nomina. Solo hay 2 contenidos al año H1(ene-may) y H2 (jun-dic)
        public string CostCenter { get; set; }
        public string SpendCategory { get; set; }
        public string RevenueCategory { get; set; } // SIN VALOR
        public string Project { get; set; }
        public string LedgerAccount { get; set; }
        public string AccountSet_a { get; set; } //IDPARAM=12 Este corresponde a la primera Columna con titulo AccountSet
        public string LedgerAccountSummary { get; set; } // SIN VALOR
        public string AccountSet_b { get; set; } // SIN VALOR (segunda Columna con titulo AccountSet)
        public string BudgetCurrency { get; set; }
        public string BookCode { get; set; } // SIN VALOR
        public decimal BudgetDebitAmount { get; set; }
        public decimal BudgetCreditAmount { get; set; }
        public string QuantityChange { get; set; } // SIN VALOR
        public string Unit { get; set; } // SIN VALOR
        public string Memo { get; set; }//Este se genera por Sistema a partir de la concatenación de las columnas FECHA-CREACION-LAYOUT(este valor viene del param Amendment Date)  + FECHA DE POLIZA + BACHNUMB +REFERENCE que vienen de la BD xNOMExcelRH antes xNOM2001GPInt 
    }
}
