using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum InsuranceType
    {
        [Description("PROPIO")] Propio,
        [Description("ENDOSADO")] Endosado,
        [Description("CON DEVOLUCION")] ConDevolucion,
        [Description("INDIVIDUAL")] Individual,
        [Description("MANCOMUNADO")] Mancomunado,
    }
}
