using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class ledgAcctCredit_ELIMINAR
    {
        public string prefixLedgAcct { get; set; }
        public string ledgerAccount { get; set; }
        public string workDayAccountFull { get; set; }
        public string description { get; set; }
        public decimal credit { get; set; }
    }
}
