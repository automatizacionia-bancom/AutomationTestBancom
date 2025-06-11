using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Domain.Enums
{
    public enum RequestType
    {
        [Description("EXEPCION")] Excepcion,
        [Description("INGRESO A RIESGOS")] IngresoARiesgos
    }
}
