using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class listParamsLayoutImportBudgetAmendment
    {
        public string Bachnumb { get; set; }
        public string HeaderKey { get; set; } // IDPARAM=1 Este valor siempre es 1 numerico
        public string AddOnly { get; set; }
        public string BudgetAmendment { get; set; }
        public string AutoComplete { get; set; }
        public string Comment { get; set; }
        public string Worker { get; set; }
        public string Id { get; set; }
        public string Submit { get; set; }
        public string CompanyOrCompanyHierarchy { get; set; }
        public string BudgetStructure { get; set; }
        public string BudgetName { get; set; }
        public string FiscalYear { get; set; }
        public string AmendmentDate { get; set; }
        public string Description { get; set; }
        public string BudgetAmendmentType { get; set; }
        public string BalancedAmendment { get; set; }
    }
}
