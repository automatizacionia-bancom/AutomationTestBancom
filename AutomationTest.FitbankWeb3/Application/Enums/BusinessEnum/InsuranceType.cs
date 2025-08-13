using System.ComponentModel;

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
