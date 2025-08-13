using System.ComponentModel;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum PayrollSourceType
    {
        [Description("Dir/Of. Economia")] DireccionEconomia = 1,
        [Description("CPMP")] CPMP = 2,
        [Description("OPREFA")] OPREFA = 3,
        [Description("JEPEN")] JEPEN = 4,
        [Description("COPER")] COPER = 5
    }
}
