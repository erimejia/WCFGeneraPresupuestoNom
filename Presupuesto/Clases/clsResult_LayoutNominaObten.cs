using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class clsResult_LayoutNominaObten
    {
        public List<listParamsLayoutImportBudgetAmendment> listLayoutIBA { get; set; }
        public List<listParamsLayoutAmendmentEntryData> listLayoutAED { get; set; }
        public int totalCuentasDestino { get; set; }
        public int totalCuentasOrigen { get; set; }
        public decimal totalDebitAmount { get; set; }
        public decimal totalCreditAmount { get; set; }
    }
}
