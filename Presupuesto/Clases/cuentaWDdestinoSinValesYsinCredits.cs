using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class cuentaWDdestinoSinValesYsinCredits
    {
        public string CostCenter { get; set; }
        public string LedgerAccount { get; set; }
        public string SpendCategory { get; set; }
        public decimal Debito { get; set; }
        //public decimal Credit { get; set; }
        public string BudgetCurrency { get; set; }
        //public string ProjectAux { get; set; }
        public short? idProject { get; set; }
        public string ProjectName { get; set; }
        //public string HeaderKey { get; set; }
        //public string LineKey { get; set; }
        //public string FiscalTimeInterval { get; set; }
        //public string AccountSet_a { get; set; }
        //public string Memo { get; set; }
    }
}
