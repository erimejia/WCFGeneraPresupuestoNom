using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presupuesto.Clases
{
    public class clsResult_CuentaBuscar
    {
        public int ID_CUENTA { get; set; }
        public string NUM { get; set; }
        public string CUENTA { get; set; }
        public string DESCRIPCION { get; set; }

        //COST-CENTER
        public int ID_CC { get; set; }
        public string COSTCENTER { get; set; }
        //public string DESCRIBECC { get; set; }
        //public bool ESTATUSCC { get; set; }

        //SPEND-CATEGORY
        public int ID_SC { get; set; }
        public string SPENDCATEGORY { get; set; }
        //public string DESCRIBESC { get; set; }
        //public bool ESTATUSSC { get; set; }

        //LEDGER-ACCOUNT
        public int ID_LA { get; set; }
        public string LEDGERACCOUNT { get; set; }
        //public string DESCRIBELA { get; set; }
        //public bool ESTATUSLA { get; set; }

        //CURRENCY
        public short IDCURRENCY { get; set; }
        public string CODECURRENCY { get; set; }
        //public string DESCRIBECY { get; set; }
        //public bool ESTATUSCY { get; set; }

        //TIPO-CUENTA
        public short IDTIPOCUENTA { get; set; }
        public string TIPOCUENTA { get; set; }
        //public string DESCRIBETC { get; set; }
        //public bool ESTATUSTC { get; set; }

        //IDPROJECT
        public short? IDPROJECT { get; set; }
        public string PROJECT { get; set; }

        //CUENTAESPECIAL
        public bool CUENTAESPECIAL { get; set; }

        //VALES
        public bool VALES { get; set; }

        public bool ESTATUS { get; set; }
        public string NAMEUPDATE { get; set; }
        public DateTime FECHAUPDATE { get; set; }
    }
}
