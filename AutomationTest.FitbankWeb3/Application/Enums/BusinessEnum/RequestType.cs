using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum RequestType
    {
        [Description("EXCEPCION")] Excepcion,
        [Description("INGRESO A RIESGOS")] IngresoARiesgos
    }
}
