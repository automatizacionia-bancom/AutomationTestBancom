using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Domain.Enums
{
    public enum PayrollSourceType
    {
        [Description("Dir/Of. Economia")] DireccionEconomia = 1,
        [Description("CPMP")]  CPMP = 2,
        [Description("OPREFA")]  OPREFA = 3,
        [Description("JEPEN")] JEPEN = 4,
        [Description("COPER")] COPER = 5
    }
}
