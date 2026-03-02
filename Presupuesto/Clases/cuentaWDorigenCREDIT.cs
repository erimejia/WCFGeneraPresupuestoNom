using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class cuentaWDorigenCREDIT
    {
        public string prefixLedgAcct { get; set; }
        public string CostCenter { get; set; }
        public string LedgerAccount { get; set; }
        public string SpendCategory { get; set; }
        public decimal Credit { get; set; }
        public string BudgetCurrency { get; set; }

    }
}
