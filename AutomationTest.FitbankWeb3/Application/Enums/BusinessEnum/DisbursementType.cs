using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum DisbursementType
    {
        [Description("Abono A Cuenta")] AbonoACuenta = 0,
        [Description("Orden De Pago")]  OrdenDePago = 1,
        [Description("Unspecified")] Unspecified = 3,
    }
}
